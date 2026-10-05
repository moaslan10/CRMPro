using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CRMPro.Api.Data;
using CRMPro.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CRMPro.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var user = _db.Users.FirstOrDefault(x =>
            x.Email.ToLower() == request.Email.Trim().ToLower());

        if (user is null || !user.IsActive || !PasswordHasher.Verify(request.Password, user.Password))
            return Unauthorized(new { message = "Invalid email or password, or the account is disabled." });

        // Upgrade legacy plaintext demo passwords after a successful login.
        if (!user.Password.StartsWith("PBKDF2$", StringComparison.Ordinal))
        {
            user.Password = PasswordHasher.Hash(request.Password);
            _db.SaveChanges();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256));

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            user = new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role
            }
        });
    }
}

public record LoginRequest(string Email, string Password);
