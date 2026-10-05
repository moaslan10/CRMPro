using CRMPro.Api.Data;
using CRMPro.Api;
using CRMPro.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRMPro.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        var query = _db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || x.Email.Contains(search) || x.Role.Contains(search));

        var rows = await query.OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.Id, x.Name, x.Email, x.Role, x.IsActive, x.CreatedAt })
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost]
    public async Task<IActionResult> Create(User item)
    {
        item.Id = 0;
        item.CreatedAt = DateTime.UtcNow;
        item.Password = PasswordHasher.Hash(item.Password);
        _db.Users.Add(item);
        await _db.SaveChangesAsync();
        await Log("Created", "User", item.Id);
        return Ok(new { item.Id, item.Name, item.Email, item.Role, item.IsActive, item.CreatedAt });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, User item)
    {
        var existing = await _db.Users.FindAsync(id);
        if (existing is null) return NotFound();

        existing.Name = item.Name;
        existing.Email = item.Email;
        existing.Role = item.Role;
        existing.IsActive = item.IsActive;
        if (!string.IsNullOrWhiteSpace(item.Password)) existing.Password = PasswordHasher.Hash(item.Password);

        await _db.SaveChangesAsync();
        await Log("Updated", "User", id);
        return Ok(new { existing.Id, existing.Name, existing.Email, existing.Role, existing.IsActive, existing.CreatedAt });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Users.FindAsync(id);
        if (existing is null) return NotFound();

        _db.Users.Remove(existing);
        await _db.SaveChangesAsync();
        await Log("Deleted", "User", id);
        return NoContent();
    }

    private async Task Log(string action, string entity, int id)
    {
        var actor = User.Identity?.Name ?? "Admin";
        _db.AuditLogs.Add(new AuditLog { Actor = actor, Action = action, Entity = entity, EntityId = id });
        await _db.SaveChangesAsync();
    }
}
