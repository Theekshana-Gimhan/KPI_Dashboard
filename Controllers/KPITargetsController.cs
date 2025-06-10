using DocumentFormat.OpenXml.Spreadsheet;
using KPI_Dashboard.Data;
using KPI_Dashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class KPITargetsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuditService _auditService;

    public KPITargetsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        AuditService auditService)
    {
        _context = context;
        _userManager = userManager;
        _auditService = auditService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Create()
    {
        var model = new KPITarget
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1).AddDays(-1)
        };
        return View(model);
    }
    [HttpGet]
    public IActionResult GetTargetFields(string department)
    {
        var model = new KPITarget { Department = department };
        return PartialView("_TargetFields", model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(KPITarget model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        KPIAuditTrail? auditLog = null;

        try
        {
            // Pre-operation audit
            auditLog = await _auditService.LogActionAsync(
                "Attempt",
                "Create KPI Target",
                null,
                model);

            // Validate date range
            if (model.StartDate > model.EndDate)
            {
                throw new Exception("Start date cannot be after end date");
            }

            // Set additional properties
            model.SetByUserId = _userManager.GetUserId(User);
            model.SetDate = DateTime.UtcNow;

            _context.KPITargets.Add(model);
            await _context.SaveChangesAsync();

            // Update audit log with target ID
            if (auditLog != null)
            {
                auditLog.TargetId = model.Id;
                auditLog.ActionType = "Create";
                await _context.SaveChangesAsync();
            }

            // Confirm success
            await _auditService.UpdateAuditLogAsync(auditLog.Id, true);
            await transaction.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            if (auditLog != null)
            {
                await _auditService.UpdateAuditLogAsync(auditLog.Id, false, ex.Message);
            }

            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }
}