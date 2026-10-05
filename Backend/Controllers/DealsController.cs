using CRMPro.Api.Data;
using CRMPro.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRMPro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/deals")]
public class DealsController : ControllerBase
{
    private readonly AppDbContext _db;
    public DealsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? stage, [FromQuery] string? search)
    {
        var query = _db.Deals.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(stage) && stage != "All") query = query.Where(x => x.Stage == stage);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Title.Contains(search) || x.CustomerName.Contains(search));
        return Ok(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Deal item)
    {
        item.Id = 0; item.CreatedAt = DateTime.UtcNow;
        _db.Deals.Add(item);
        await _db.SaveChangesAsync();
        await Log("Created", "Deal", item.Id);
        return Ok(item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Deal item)
    {
        var existing = await _db.Deals.FindAsync(id);
        if (existing is null) return NotFound();
        existing.Title = item.Title; existing.CustomerName = item.CustomerName; existing.Value = item.Value;
        existing.Stage = item.Stage; existing.ExpectedCloseDate = item.ExpectedCloseDate;
        await _db.SaveChangesAsync();
        await Log("Updated", "Deal", id);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Deals.FindAsync(id);
        if (existing is null) return NotFound();
        _db.Deals.Remove(existing);
        await _db.SaveChangesAsync();
        await Log("Deleted", "Deal", id);
        return NoContent();
    }

    private async Task Log(string action, string entity, int id)
    {
        _db.AuditLogs.Add(new AuditLog { Actor = User.Identity?.Name ?? "User", Action = action, Entity = entity, EntityId = id });
        await _db.SaveChangesAsync();
    }
}
