using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SocialApp.UserService.Swagger;

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