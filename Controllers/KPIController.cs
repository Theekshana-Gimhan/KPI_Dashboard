using KPI_Dashboard.Data;
using KPI_Dashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class KPIController : Controller
{
    private readonly ApplicationDbContext _context;

    public KPIController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /KPI/EditList
    [HttpGet]
    public async Task<IActionResult> EditList(string department, string userName, DateTime? date)
    {
        var kpis = new List<KPIEditViewModel>();

        // Find userId by userName if provided
        string? userId = null;

        if (!string.IsNullOrEmpty(userName))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == userName);
            if (user != null)
                userId = user.Id;
            else
                userId = "___NO_MATCH___"; // Ensures no results if name not found
        }

        if (department == "Admission" || string.IsNullOrEmpty(department))
        {
            var admission = _context.AdmissionKPIs.AsQueryable();
            if (!string.IsNullOrEmpty(userId))
                admission = admission.Where(k => k.UserId == userId);
            if (date.HasValue)
                admission = admission.Where(k => k.EntryDate.Date == date.Value.Date);

            kpis.AddRange(await admission.Select(k => new KPIEditViewModel
            {
                Id = k.Id,
                Type = "Admission",
                EntryDate = k.EntryDate,
                Applications = k.Applications,
                Consultations = k.Consultations,
                UserId = k.UserId
            }).ToListAsync());
        }

        if (department == "Visa" || string.IsNullOrEmpty(department))
        {
            var visa = _context.VisaKPIs.AsQueryable();
            if (!string.IsNullOrEmpty(userId))
                visa = visa.Where(k => k.UserId == userId);
            if (date.HasValue)
                visa = visa.Where(k => k.EntryDate.Date == date.Value.Date);

            kpis.AddRange(await visa.Select(k => new KPIEditViewModel
            {
                Id = k.Id,
                Type = "Visa",
                EntryDate = k.EntryDate,
                Inquiries = k.Inquiries,
                Consultations = k.Consultations,
                Conversions = k.Conversions,
                UserId = k.UserId
            }).ToListAsync());
        }

        return View(kpis);
    }

    // GET: /KPI/Edit/{id}?type=Admission or type=Visa
    [HttpGet]
    public async Task<IActionResult> Edit(int id, string type)
    {
        if (type == "Admission")
        {
            var kpi = await _context.AdmissionKPIs.FindAsync(id);
            if (kpi == null) return NotFound();
            var vm = new KPIEditViewModel
            {
                Id = kpi.Id,
                Type = "Admission",
                EntryDate = kpi.EntryDate,
                Applications = kpi.Applications,
                Consultations = kpi.Consultations,
                UserId = kpi.UserId
            };
            return View(vm);
        }
        else if (type == "Visa")
        {
            var kpi = await _context.VisaKPIs.FindAsync(id);
            if (kpi == null) return NotFound();
            var vm = new KPIEditViewModel
            {
                Id = kpi.Id,
                Type = "Visa",
                EntryDate = kpi.EntryDate,
                Inquiries = kpi.Inquiries,
                Consultations = kpi.Consultations,
                Conversions = kpi.Conversions,
                UserId = kpi.UserId
            };
            return View(vm);
        }
        return NotFound();
    }

    // POST: /KPI/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(KPIEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Validate UserId
        if (string.IsNullOrWhiteSpace(model.UserId) ||
            !await _context.Users.AnyAsync(u => u.Id == model.UserId))
        {
            ModelState.AddModelError(nameof(model.UserId), "Selected user does not exist.");
            return View(model);
        }

        if (model.Type == "Admission")
        {
            var kpi = await _context.AdmissionKPIs.FindAsync(model.Id);
            if (kpi == null) return NotFound();
            kpi.EntryDate = model.EntryDate;
            kpi.Applications = model.Applications ?? 0;
            kpi.Consultations = model.Consultations ?? 0;
            kpi.UserId = model.UserId;
        }
        else if (model.Type == "Visa")
        {
            var kpi = await _context.VisaKPIs.FindAsync(model.Id);
            if (kpi == null) return NotFound();
            kpi.EntryDate = model.EntryDate;
            kpi.Inquiries = model.Inquiries ?? 0;
            kpi.Consultations = model.Consultations ?? 0;
            kpi.Conversions = model.Conversions ?? 0;
            kpi.UserId = model.UserId;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(EditList));
    }

}
