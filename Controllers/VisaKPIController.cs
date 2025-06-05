using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KPI_Dashboard.Data;
using KPI_Dashboard.Models;
using System.Threading.Tasks;
using System.Linq;

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
        public IActionResult Create()
        {
            return View(new VisaKPI());
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            model.UserId = user.Id;
            model.EntryDate = DateTime.Now;

            _context.VisaKPIs.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Visa KPI added successfully!";
            return RedirectToAction("Index");
        }

        // GET: Index
        public IActionResult Index()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var kpis = _context.VisaKPIs.Where(k => k.UserId == user.Id).ToList();
            return View(kpis);
        }
    }
}