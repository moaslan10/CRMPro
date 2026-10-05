using CRMPro.Api.Data;
using CRMPro.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMPro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProfileController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var user = await _db.Users.FindAsync(int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value));
        if (user is null) return NotFound();
        return Ok(new { user.Id, user.Name, user.Email, user.Role, user.IsActive });
    }

    [HttpPut]
    public async Task<IActionResult> Update(ProfileRequest request)
    {
        var id = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.Name = request.Name;
        if (!string.IsNullOrWhiteSpace(request.Password)) user.Password = PasswordHasher.Hash(request.Password);
        await _db.SaveChangesAsync();
        return Ok(new { user.Id, user.Name, user.Email, user.Role, user.IsActive });
    }
}

public record ProfileRequest(string Name, string? Password);
