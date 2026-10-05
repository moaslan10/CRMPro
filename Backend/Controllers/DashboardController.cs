using CRMPro.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMPro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Get()
    {
        // SQLite does not reliably translate decimal aggregate operations.
        // Materialize the small CRM collections first, then calculate totals in C#.
        var customers = _db.Customers.ToList();
        var leads = _db.Leads.ToList();
        var deals = _db.Deals.ToList();
        var activities = _db.Activities.ToList();

        return Ok(new
        {
            customers = customers.Count,
            activeCustomers = customers.Count(x => x.Status == "Active"),
            leads = leads.Count,
            deals = deals.Count,
            wonDeals = deals.Count(x => x.Stage == "Won"),
            pipelineValue = deals.Sum(x => x.Value),
            leadValue = leads.Sum(x => x.EstimatedValue),
            pendingActivities = activities.Count(x => x.Status == "Pending")
        });
    }
}
