using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KPI_Dashboard.Data;
using KPI_Dashboard.Models;
using System.Threading.Tasks;
using System.Linq;

namespace KPI_Dashboard.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var model = new DashboardViewModel
            {
                AdmissionKPIs = _context.AdmissionKPIs.Where(k => k.UserId == user.Id).ToList(),
                VisaKPIs = _context.VisaKPIs.Where(k => k.UserId == user.Id).ToList()
            };

            return View(model);
        }
    }
}