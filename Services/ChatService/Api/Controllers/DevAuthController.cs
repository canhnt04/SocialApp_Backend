using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SocialApp.ChatService.Api.Controllers;

[ApiController]
[Route("api/v1/dev-auth")]
[AllowAnonymous]
public class DevAuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public DevAuthController(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    [HttpPost("token")]
    public IActionResult IssueToken([FromBody] DevTokenRequest request)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        if (request.UserId == Guid.Empty)
        {
            return BadRequest("userId is required and must be a valid non-empty Guid.");
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest("username is required.");
        }

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "JwtSettings is not configured correctly.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var expiresAt = now.AddHours(8);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, request.UserId.ToString()),
            new(ClaimTypes.Name, request.Username),
            new(JwtRegisteredClaimNames.UniqueName, request.Username),
            new(ClaimTypes.Role, request.Role ?? "User")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new DevTokenResponse(jwt, expiresAt));
    }
}

public record DevTokenRequest(Guid UserId, string Username, string? Role = "User");
public record DevTokenResponse(string AccessToken, DateTime ExpiresAtUtc);
