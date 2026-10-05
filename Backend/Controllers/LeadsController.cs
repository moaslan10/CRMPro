using CRMPro.Api.Data;
using CRMPro.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRMPro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/leads")]
public class LeadsController : ControllerBase
{
    private readonly AppDbContext _db;
    public LeadsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] string? status)
    {
        var query = _db.Leads.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || x.Company.Contains(search) || x.Email.Contains(search));
        if (!string.IsNullOrWhiteSpace(status) && status != "All")
            query = query.Where(x => x.Status == status);
        return Ok(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Lead item)
    {
        item.Id = 0; item.CreatedAt = DateTime.UtcNow;
        _db.Leads.Add(item); await _db.SaveChangesAsync(); await Log("Created", "Lead", item.Id); return Ok(item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Lead item)
    {
        var existing = await _db.Leads.FindAsync(id);
        if (existing is null) return NotFound();
        existing.Name = item.Name; existing.Company = item.Company; existing.Email = item.Email; existing.Phone = item.Phone;
        existing.Source = item.Source; existing.Status = item.Status; existing.EstimatedValue = item.EstimatedValue;
        await _db.SaveChangesAsync(); await Log("Updated", "Lead", id); return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Leads.FindAsync(id);
        if (existing is null) return NotFound();
        _db.Leads.Remove(existing); await _db.SaveChangesAsync(); await Log("Deleted", "Lead", id); return NoContent();
    }

    private async Task Log(string action, string entity, int id)
    {
        _db.AuditLogs.Add(new AuditLog { Actor = User.Identity?.Name ?? "User", Action = action, Entity = entity, EntityId = id });
        await _db.SaveChangesAsync();
    }
}
