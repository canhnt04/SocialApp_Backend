using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Swagger tổng hợp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SocialApp API Gateway",
        Version = "v1",
        Description = "API Gateway tổng hợp tất cả các service: Auth, User, Chat, Post"
    });
});

// CORS
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

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Swagger UI tổng hợp hiển thị tài liệu của tất cả các service
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/auth/v1/swagger.json", "AuthService API v1");
        c.SwaggerEndpoint("/swagger/user/v1/swagger.json", "UserService API v1");
        c.SwaggerEndpoint("/swagger/chat/v1/swagger.json", "ChatService API v1");
        c.SwaggerEndpoint("/swagger/post/v1/swagger.json", "PostService API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "SocialApp - API Documentation";
    });
}

app.UseCors();
app.MapHealthChecks("/healthz");

// Redirect root to swagger UI
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.MapReverseProxy();

app.Run();
