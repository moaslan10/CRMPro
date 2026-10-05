using CRMPro.Api.Data;
using CRMPro.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRMPro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;
    public CustomersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] string? status)
    {
        var query = _db.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || x.Company.Contains(search) || x.Email.Contains(search));
        if (!string.IsNullOrWhiteSpace(status) && status != "All")
            query = query.Where(x => x.Status == status);
        return Ok(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    [HttpGet("{id:int}/details")]
    public async Task<IActionResult> Details(int id)
    {
        var customer = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (customer is null) return NotFound();

        var leads = await _db.Leads.AsNoTracking().Where(x => x.Company == customer.Company || x.Name == customer.Name).ToListAsync();
        var deals = await _db.Deals.AsNoTracking().Where(x => x.CustomerName == customer.Name).ToListAsync();
        var activities = await _db.Activities.AsNoTracking().Where(x => x.RelatedTo == customer.Name).ToListAsync();
        var notes = await _db.Notes.AsNoTracking().ToListAsync();

        return Ok(new { customer, leads, deals, activities, notes });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Customer item)
    {
        item.Id = 0; item.CreatedAt = DateTime.UtcNow;
        _db.Customers.Add(item);
        await _db.SaveChangesAsync();
        await Log("Created", "Customer", item.Id);
        return Ok(item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Customer item)
    {
        var existing = await _db.Customers.FindAsync(id);
        if (existing is null) return NotFound();
        existing.Name = item.Name; existing.Company = item.Company; existing.Email = item.Email;
        existing.Phone = item.Phone; existing.Status = item.Status;
        await _db.SaveChangesAsync();
        await Log("Updated", "Customer", id);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Customers.FindAsync(id);
        if (existing is null) return NotFound();
        _db.Customers.Remove(existing);
        await _db.SaveChangesAsync();
        await Log("Deleted", "Customer", id);
        return NoContent();
    }

    private async Task Log(string action, string entity, int id)
    {
        _db.AuditLogs.Add(new AuditLog { Actor = User.Identity?.Name ?? "User", Action = action, Entity = entity, EntityId = id });
        await _db.SaveChangesAsync();
    }
}
