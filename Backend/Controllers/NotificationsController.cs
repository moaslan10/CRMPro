using CRMPro.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRMPro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;
    public NotificationsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var rows = await _db.Notifications.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(30)
            .ToListAsync();

        return Ok(new { unread = rows.Count(x => !x.IsRead), items = rows });
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> Read(int id)
    {
        var item = await _db.Notifications.FindAsync(id);
        if (item is null) return NotFound();
        item.IsRead = true;
        await _db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> ReadAll()
    {
        var items = await _db.Notifications.Where(x => !x.IsRead).ToListAsync();
        foreach (var item in items) item.IsRead = true;
        await _db.SaveChangesAsync();
        return Ok(new { count = items.Count });
    }
}
