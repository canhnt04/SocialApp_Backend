using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MediatR;
using SocialApp.ChatService.Infrastructure.Data;
using SocialApp.ChatService.Domain.Repositories;
using SocialApp.ChatService.Infrastructure.Repositories;
using SocialApp.ChatService.Hubs;
using SocialApp.ChatService.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ChatService API", Version = "v1" });
    
    // Cấu hình bảo mật JWT trong Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter only your token in the input box below (no 'Bearer ' prefix needed).",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Cấu hình XML comments hiển thị mô tả API
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// SignalR
builder.Services.AddSignalR();

// EF Core (PostgreSQL)
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(typeof(Program).Assembly);

// DI
builder.Services.AddScoped<IChatRepository, ChatRepository>();

// RabbitMQ (optional)
builder.Services.AddSingleton<MessageBroker>(sp =>
{
    var rabbitConfig = sp.GetRequiredService<IConfiguration>().GetSection("RabbitMQ");
    var logger = sp.GetRequiredService<ILogger<MessageBroker>>();
    
    if (!rabbitConfig.Exists())
    {
        logger.LogWarning("Không tìm thấy section 'RabbitMQ' trong file cấu hình!");
    }

    var hostName = rabbitConfig["HostName"] ?? "localhost";
    var port = rabbitConfig.GetValue<int>("Port", 5672); 
    var userName = rabbitConfig["UserName"] ?? "guest";
    var password = rabbitConfig["Password"] ?? "guest";

    return new MessageBroker(hostName, port, userName, password, logger);
});

builder.Services.AddSingleton<SocialApp.ChatService.Application.Interfaces.IMessagePublisher>(sp => sp.GetRequiredService<MessageBroker>());

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
    // Cho phép SignalR gửi token qua query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// CORS cho SignalR
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

// Auto migrate
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    dbContext.Database.Migrate();
}

// Warm up MessageBroker to establish connection early
try
{
    app.Services.GetRequiredService<MessageBroker>();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error warming up MessageBroker");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var viewsPath = Path.Combine(builder.Environment.ContentRootPath, "Views");
if (Directory.Exists(viewsPath))
{
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(viewsPath),
        RequestPath = ""
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(viewsPath),
        RequestPath = ""
    });
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
