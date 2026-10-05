using CRMPro.Api.Data;
using CRMPro.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRMPro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/activities")]
public class ActivitiesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ActivitiesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await _db.Activities.AsNoTracking().OrderBy(x => x.DueDate).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(Activity item)
    {
        _db.Activities.Add(item);
        await _db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Activity item)
    {
        var existing = await _db.Activities.FindAsync(id);
        if (existing is null) return NotFound();

        existing.Type = item.Type;
        existing.Subject = item.Subject;
        existing.RelatedTo = item.RelatedTo;
        existing.DueDate = item.DueDate;
        existing.Status = item.Status;

        await _db.SaveChangesAsync();
        return Ok(existing);
    }
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin") ]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Activities.FindAsync(id);
        if (existing is null) return NotFound();

        _db.Activities.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }

}
