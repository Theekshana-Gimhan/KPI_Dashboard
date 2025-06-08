using KPI_Dashboard.Data;
using KPI_Dashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KPI_Dashboard.Controllers
{
    [Authorize(Roles = "Admission")]
    public class AdmissionKPIController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdmissionKPIController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var model = new AdmissionKPI
            {
                UserId = user.Id
            };
            return View(model);
        }

        // POST: Create (Initial Submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AdmissionKPI model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return View("Confirm", model);
        }

        // POST: Confirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(AdmissionKPI model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            model.EntryDate = DateTime.Today; // Set to start of the day
            var existingKpi = _context.AdmissionKPIs
                .FirstOrDefault(k => k.UserId == user.Id && k.EntryDate.Date == model.EntryDate.Date);

            if (existingKpi != null)
            {
                existingKpi.Applications = model.Applications;
                existingKpi.Consultations = model.Consultations;
                _context.AdmissionKPIs.Update(existingKpi);
            }
            else
            {
                model.UserId = user.Id;
                _context.AdmissionKPIs.Add(model);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Admission KPI updated successfully!";
            return RedirectToAction("Index");
        }

        // GET: Index
        public IActionResult Index()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var kpis = _context.AdmissionKPIs.Where(k => k.UserId == user.Id).ToList();
            return View(kpis);
        }
    }
}