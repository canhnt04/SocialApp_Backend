using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MediatR;
using SocialApp.AuthService.Infrastructure.Data;
using SocialApp.AuthService.Domain.Repositories;
using SocialApp.AuthService.Infrastructure.Repositories;
using SocialApp.AuthService.Infrastructure.Messaging;
using SocialApp.AuthService.Application.Interfaces;
using SocialApp.AuthService.Infrastructure.Security;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AuthService API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter only the JWT token. Example: eyJhbGciOi..."
    });

    // Thêm XML comments từ file documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    c.OperationFilter<AuthorizeOperationFilter>();
});

// EF Core (SQL Server)
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(typeof(Program).Assembly);

// Dependency Injection
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

// RabbitMQ MessageBroker (optional - không crash nếu RabbitMQ chưa chạy)
try
{
    var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
    var messageBroker = new MessageBroker(
        hostName: rabbitConfig["HostName"] ?? "localhost",
        port: int.Parse(rabbitConfig["Port"] ?? "5672"),
        userName: rabbitConfig["UserName"] ?? "guest",
        password: rabbitConfig["Password"] ?? "guest"
    );
    builder.Services.AddSingleton(messageBroker);
}
catch
{
    // RabbitMQ chưa sẵn sàng, bỏ qua
}

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Auto migrate
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate();
}

// HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var allowAnonymous = context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() == true
            || context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

        if (allowAnonymous)
        {
            return;
        }

        var authorizeOnMethod = context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();
        var authorizeOnController = context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true;

        if (!authorizeOnMethod && !authorizeOnController)
        {
            return;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = Array.Empty<string>()
        });
    }
}
