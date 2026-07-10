using Microsoft.EntityFrameworkCore;
using MediatR;
using MassTransit;
using SocialApp.UserService.Infrastructure.Data;
using SocialApp.UserService.Domain.Repositories;
using SocialApp.UserService.Infrastructure.Repositories;
using SocialApp.UserService.Infrastructure.Authentication;
using SocialApp.UserService.Middleware.ExceptionMiddleware;
using Microsoft.OpenApi.Models;
using SocialApp.UserService.Swagger;
using SocialApp.UserService.Application.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "UserService API", Version = "v1" });

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
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MassTransit & RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

// MediatR
builder.Services.AddMediatR(typeof(Program).Assembly);

// DI
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// JWT Authentication
builder.Services.AddSocialAppJwtAuthentication(builder.Configuration);

var app = builder.Build();

// Auto migrate
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
