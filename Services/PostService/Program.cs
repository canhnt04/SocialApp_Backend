using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MediatR;
using MassTransit;
using SocialApp.PostService.Infrastructure.Data;
using SocialApp.PostService.Domain.Repositories;
using SocialApp.PostService.Infrastructure.Repositories;
using SocialApp.PostService.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PostService API", Version = "v1" });

    // Thêm XML comments từ file documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// EF Core (SQL Server)
builder.Services.AddDbContext<PostDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(typeof(Program).Assembly);

// DI
builder.Services.AddScoped<IPostRepository, PostRepository>();

// RabbitMQ (MassTransit)
builder.Services.AddMassTransit(x =>
{
    // x.AddConsumer<SomeConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
        var rabbitMqHost = rabbitConfig["Host"] ?? rabbitConfig["HostName"] ?? "localhost";
        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(rabbitConfig["Username"] ?? rabbitConfig["UserName"] ?? "guest");
            h.Password(rabbitConfig["Password"] ?? "guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

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
    var dbContext = scope.ServiceProvider.GetRequiredService<PostDbContext>();
    dbContext.Database.Migrate();
}

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
