using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KPI_Dashboard.Data;
using KPI_Dashboard.Models;
using System.Linq;

[Authorize(Roles = "Admin")]
public class KPITargetsController : Controller
{
    private readonly ApplicationDbContext _context;

    public KPITargetsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var admissions = _context.KPITargets.Where(t => t.Department == "Admissions").ToList();
        var visa = _context.KPITargets.Where(t => t.Department == "Visa").ToList();

        var model = new KPITargetsViewModel
        {
            AdmissionsApplications = admissions.FirstOrDefault(t => t.KPIName == "Applications")?.TargetValue ?? 0,
            AdmissionsConsultations = admissions.FirstOrDefault(t => t.KPIName == "Consultations")?.TargetValue ?? 0,
            VisaInquiries = visa.FirstOrDefault(t => t.KPIName == "Inquiries")?.TargetValue ?? 0,
            VisaConsultations = visa.FirstOrDefault(t => t.KPIName == "Consultations")?.TargetValue ?? 0,
            VisaConversions = visa.FirstOrDefault(t => t.KPIName == "Conversions")?.TargetValue ?? 0
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult Update(string department, KPITargetsViewModel model)
    {
        if (department == "Admissions")
        {
            UpdateOrCreateTarget("Admissions", "Applications", model.AdmissionsApplications);
            UpdateOrCreateTarget("Admissions", "Consultations", model.AdmissionsConsultations);
            model.AdmissionsSuccessMessage = "Admissions targets updated successfully!";
        }
        else if (department == "Visa")
        {
            UpdateOrCreateTarget("Visa", "Inquiries", model.VisaInquiries);
            UpdateOrCreateTarget("Visa", "Consultations", model.VisaConsultations);
            UpdateOrCreateTarget("Visa", "Conversions", model.VisaConversions);
            model.VisaSuccessMessage = "Visa targets updated successfully!";
        }

        // Save all changes at once
        _context.SaveChanges();

        // Reload all values to keep the form populated
        var admissions = _context.KPITargets.Where(t => t.Department == "Admissions").ToList();
        var visa = _context.KPITargets.Where(t => t.Department == "Visa").ToList();

        model.AdmissionsApplications = admissions.FirstOrDefault(t => t.KPIName == "Applications")?.TargetValue ?? 0;
        model.AdmissionsConsultations = admissions.FirstOrDefault(t => t.KPIName == "Consultations")?.TargetValue ?? 0;
        model.VisaInquiries = visa.FirstOrDefault(t => t.KPIName == "Inquiries")?.TargetValue ?? 0;
        model.VisaConsultations = visa.FirstOrDefault(t => t.KPIName == "Consultations")?.TargetValue ?? 0;
        model.VisaConversions = visa.FirstOrDefault(t => t.KPIName == "Conversions")?.TargetValue ?? 0;

        return View("Index", model);
    }

    private void UpdateOrCreateTarget(string department, string kpiName, int value)
    {
        var target = _context.KPITargets.FirstOrDefault(t => t.Department == department && t.KPIName == kpiName);
        if (target != null)
        {
            target.TargetValue = value;
            _context.KPITargets.Update(target);
        }
        else
        {
            _context.KPITargets.Add(new KPITarget { Department = department, KPIName = kpiName, TargetValue = value });
        }
    }
}
