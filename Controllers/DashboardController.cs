using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KPI_Dashboard.Data;
using KPI_Dashboard.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace KPI_Dashboard.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMemoryCache memoryCache)
        {
            _context = context;
            _userManager = userManager;
            _cache = memoryCache;
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
        public async Task<JsonResult> GetDashboardData(DateTime? startDate, DateTime? endDate, string department, string user)
        {
            startDate = startDate ?? DateTime.Today.AddDays(-30);
            endDate = endDate ?? DateTime.Today;

            // Adjust end date to include the whole day
            endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);

            var cacheKey = $"DashboardData_{startDate}_{endDate}_{department}_{user}";
            if (!_cache.TryGetValue(cacheKey, out DashboardViewModel dashboardData))
            {
                dashboardData = new DashboardViewModel();

                // Base queries with AsNoTracking for performance
                var admissionQuery = _context.AdmissionKPIs.AsNoTracking();
                var visaQuery = _context.VisaKPIs.AsNoTracking();

                // Apply date filter
                admissionQuery = admissionQuery.Where(k => k.EntryDate >= startDate && k.EntryDate <= endDate);
                visaQuery = visaQuery.Where(k => k.EntryDate >= startDate && k.EntryDate <= endDate);

                // Apply user filter
                if (user != "All users")
                {
                    var selectedUser = await _userManager.FindByNameAsync(user);
                    if (selectedUser != null)
                    {
                        admissionQuery = admissionQuery.Where(k => k.UserId == selectedUser.Id);
                        visaQuery = visaQuery.Where(k => k.UserId == selectedUser.Id);
                    }
                }

                // Apply department filter
                if (department == "Admission")
                {
                    visaQuery = visaQuery.Where(k => false); // Exclude Visa data
                }
                else if (department == "Visa")
                {
                    admissionQuery = admissionQuery.Where(k => false); // Exclude Admission data
                }

                // Line Chart: Group by date (daily for simplicity)
                var allDates = Enumerable.Range(0, (endDate.Value.Date - startDate.Value.Date).Days + 1)
                    .Select(d => startDate.Value.Date.AddDays(d))
                    .ToList();

                var admissionGrouped = admissionQuery
                    .GroupBy(k => k.EntryDate.Date)
                    .Select(g => new { Date = g.Key, Applications = g.Sum(k => k.Applications), Consultations = g.Sum(k => k.Consultations) })
                    .ToList();

                var visaGrouped = visaQuery
                    .GroupBy(k => k.EntryDate.Date)
                    .Select(g => new { Date = g.Key, Inquiries = g.Sum(k => k.Inquiries), Consultations = g.Sum(k => k.Consultations), Conversions = g.Sum(k => k.Conversions) })
                    .ToList();

                foreach (var date in allDates)
                {
                    dashboardData.LineData.Dates.Add(date.ToString("yyyy-MM-dd"));
                    var admissionData = admissionGrouped.FirstOrDefault(g => g.Date == date);
                    var visaData = visaGrouped.FirstOrDefault(g => g.Date == date);

                    dashboardData.LineData.Applications.Add(admissionData?.Applications ?? 0);
                    dashboardData.LineData.AdmissionConsultations.Add(admissionData?.Consultations ?? 0);
                    dashboardData.LineData.VisaConsultations.Add(visaData?.Consultations ?? 0);
                    dashboardData.LineData.Inquiries.Add(visaData?.Inquiries ?? 0);
                    dashboardData.LineData.Conversions.Add(visaData?.Conversions ?? 0);
                }

                // Pie Chart: Distribution of consultations by user
                var admissionConsultationsByUser = admissionQuery
                    .GroupBy(k => k.UserId)
                    .Select(g => new { UserId = g.Key, Consultations = g.Sum(k => k.Consultations) })
                    .ToList();

                var visaConsultationsByUser = visaQuery
                    .GroupBy(k => k.UserId)
                    .Select(g => new { UserId = g.Key, Consultations = g.Sum(k => k.Consultations) })
                    .ToList();

                var consultationsByUser = admissionConsultationsByUser
                    .Concat(visaConsultationsByUser)
                    .GroupBy(x => x.UserId)
                    .Select(g => new { UserId = g.Key, TotalConsultations = g.Sum(x => x.Consultations) })
                    .ToList();

                foreach (var entry in consultationsByUser)
                {
                    var username = (await _userManager.FindByIdAsync(entry.UserId))?.UserName ?? "Unknown";
                    dashboardData.PieData.Labels.Add(username);
                    dashboardData.PieData.Values.Add(entry.TotalConsultations);
                }

                // Bar Chart: Applications vs Consultations, Consultations vs Conversions
                dashboardData.BarData.Labels.Add("Applications vs Consultations (Admission)");
                dashboardData.BarData.Labels.Add("Consultations vs Conversions (Visa)");
                dashboardData.BarData.Applications.Add(admissionQuery.Sum(k => k.Applications));
                dashboardData.BarData.Consultations.Add(admissionQuery.Sum(k => k.Consultations));
                dashboardData.BarData.Consultations.Add(visaQuery.Sum(k => k.Consultations));
                dashboardData.BarData.Conversions.Add(visaQuery.Sum(k => k.Conversions));

                // Metric Cards
                dashboardData.Metrics.TotalApplications = admissionQuery.Sum(k => k.Applications);
                dashboardData.Metrics.TotalConsultations = admissionQuery.Sum(k => k.Consultations) + visaQuery.Sum(k => k.Consultations);
                var totalInquiries = visaQuery.Sum(k => k.Inquiries);
                var totalConversions = visaQuery.Sum(k => k.Conversions);
                dashboardData.Metrics.VisaConversionRate = totalInquiries > 0 ? (double)totalConversions / totalInquiries * 100 : 0;

                // Top Performing User (by total consultations)
                var topUser = consultationsByUser.OrderByDescending(x => x.TotalConsultations).FirstOrDefault();
                dashboardData.Metrics.TopPerformingUser = topUser != null
                    ? (await _userManager.FindByIdAsync(topUser.UserId))?.UserName ?? "Unknown"
                    : "N/A";

                // Cache the data for 5 minutes
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
                _cache.Set(cacheKey, dashboardData, cacheEntryOptions);
            }

            return Json(dashboardData);
        }
    }
}