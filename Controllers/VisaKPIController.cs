using KPI_Dashboard.Data;
using KPI_Dashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KPI_Dashboard.Controllers
{
    [Authorize(Roles = "Visa")]
    public class VisaKPIController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VisaKPIController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

            var model = new VisaKPI
            {
                UserId = user.Id
            };
            return View(model);
        }

        // POST: Create (Initial Submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(VisaKPI model)
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
        public async Task<IActionResult> Confirm(VisaKPI model)
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
            var existingKpis = _context.VisaKPIs
                .Where(k => k.UserId == user.Id && k.EntryDate.Date == model.EntryDate.Date)
                .ToList();

            if (existingKpis.Any())
            {
                _context.VisaKPIs.RemoveRange(existingKpis);
            }

            model.UserId = user.Id;
            _context.VisaKPIs.Add(model);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Visa KPI updated successfully!";
            return RedirectToAction("Index");
        }

        // GET: Index
        public IActionResult Index()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return Unauthorized();
            }
            var kpis = _context.VisaKPIs.Where(k => k.UserId == user.Id).ToList();
            return View(kpis);
        }

    }
}