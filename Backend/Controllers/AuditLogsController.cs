using CRMPro.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRMPro.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuditLogsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        var query = _db.AuditLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Actor.Contains(search) || x.Action.Contains(search) || x.Entity.Contains(search));

        return Ok(await query.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync());
    }
}
