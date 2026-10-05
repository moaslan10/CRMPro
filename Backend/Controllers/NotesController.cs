using CRMPro.Api.Data;
using CRMPro.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRMPro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notes")]
public class NotesController : ControllerBase
{
    private readonly AppDbContext _db;
    public NotesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await _db.Notes.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(Note item)
    {
        _db.Notes.Add(item);
        await _db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Note item)
    {
        var existing = await _db.Notes.FindAsync(id);
        if (existing is null) return NotFound();

        existing.Title = item.Title;
        existing.Content = item.Content;
        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Notes.FindAsync(id);
        if (existing is null) return NotFound();

        _db.Notes.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
