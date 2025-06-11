using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class KPITargetsController : Controller
{
    // Replace with your data access logic
    private static int admissionsApplications = 100;
    private static int admissionsConsultations = 50;
    private static int visaInquiries = 80;
    private static int visaConsultations = 40;
    private static int visaConversions = 20;

    [HttpGet]
    public IActionResult Index()
    {
        var model = new KPITargetsViewModel
        {
            AdmissionsApplications = admissionsApplications,
            AdmissionsConsultations = admissionsConsultations,
            VisaInquiries = visaInquiries,
            VisaConsultations = visaConsultations,
            VisaConversions = visaConversions
        };
        return View(model);
    }

    [HttpPost]
    public IActionResult Update(string department, KPITargetsViewModel model)
    {
        if (department == "Admissions")
        {
            admissionsApplications = model.AdmissionsApplications;
            admissionsConsultations = model.AdmissionsConsultations;
            model.AdmissionsSuccessMessage = "Admissions targets updated successfully!";
            // Keep Visa values
            model.VisaInquiries = visaInquiries;
            model.VisaConsultations = visaConsultations;
            model.VisaConversions = visaConversions;
        }
        else if (department == "Visa")
        {
            visaInquiries = model.VisaInquiries;
            visaConsultations = model.VisaConsultations;
            visaConversions = model.VisaConversions;
            model.VisaSuccessMessage = "Visa targets updated successfully!";
            // Keep Admissions values
            model.AdmissionsApplications = admissionsApplications;
            model.AdmissionsConsultations = admissionsConsultations;
        }
        return View("Index", model);
    }
}
