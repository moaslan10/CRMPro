using CRMPro.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMPro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReportsController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Get()
    {
        var deals = _db.Deals.ToList();
        var leads = _db.Leads.ToList();
        var customers = _db.Customers.ToList();

        var stages = deals.GroupBy(x => x.Stage).Select(g => new { stage = g.Key, count = g.Count(), value = g.Sum(x => x.Value) }).ToList();
        var leadStatuses = leads.GroupBy(x => x.Status).Select(g => new { status = g.Key, count = g.Count(), value = g.Sum(x => x.EstimatedValue) }).ToList();

        var totalPipeline = deals.Sum(x => x.Value);
        var totalRevenue = deals.Where(x => x.Stage == "Won").Sum(x => x.Value);
        var wonCount = deals.Count(x => x.Stage == "Won");
        var conversionRate = leads.Count == 0 ? 0 : Math.Round((decimal)wonCount / leads.Count * 100, 1);

        return Ok(new
        {
            stages,
            leadStatuses,
            totalRevenue,
            totalPipeline,
            wonDeals = wonCount,
            totalDeals = deals.Count,
            conversionRate,
            customers = customers.Count,
            activeCustomers = customers.Count(x => x.Status == "Active")
        });
    }
}
