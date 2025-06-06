using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KPI_Dashboard.Data;
using KPI_Dashboard.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            var users = new List<string> { "All users" };
            users.AddRange((await _userManager.GetUsersInRoleAsync("Admission"))
                .Concat(await _userManager.GetUsersInRoleAsync("Visa"))
                .Select(u => u.UserName));
            ViewBag.Users = users;
            return View();
        }

        [HttpPost]
        public JsonResult GetDashboardData(DateTime? startDate, DateTime? endDate, string department, string user)
        {
            // Placeholder: Will implement data aggregation in Step 2
            var data = new
            {
                message = "Data will be populated in Step 2",
                lineData = new { },
                pieData = new { },
                barData = new { },
                metrics = new { }
            };
            return Json(data);
        }
    }
}