using KPI_Dashboard.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Add this for Include and ToListAsync

[Authorize(Roles = "Admin")]
public class AuditTrailController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuditTrailController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? actionType = null, // Make nullable reference type
        bool? isSuccess = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.KPIAuditTrails
            .Include(a => a.User)
            .Include(a => a.Target)
            .AsQueryable();

        if (!string.IsNullOrEmpty(actionType))
        {
            query = query.Where(a => a.ActionType == actionType);
        }

        if (isSuccess.HasValue)
        {
            query = query.Where(a => a.IsSuccess == isSuccess.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= toDate.Value);
        }

        var audits = await query
            .OrderByDescending(a => a.Timestamp)
            .Take(1000)
            .ToListAsync();

        ViewBag.ActionTypes = await _context.KPIAuditTrails
            .Select(a => a.ActionType)
            .Distinct()
            .ToListAsync();

        return View(audits);
    }
}
