using ClosedXML.Excel;
using iText.Kernel.Font; // For PdfFontFactory
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using KPI_Dashboard.Data;
using KPI_Dashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For AsNoTracking
using Microsoft.Extensions.Caching.Memory;

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

            users.AddRange(
                (await _userManager.GetUsersInRoleAsync("Admission"))
                    .Concat(await _userManager.GetUsersInRoleAsync("Visa"))
                    .Select(u => u.UserName)
                    .Where(u => !string.IsNullOrEmpty(u))
                    .Select(u => u!) // Now safe: only non-null, non-empty strings
            );

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
            if (!_cache.TryGetValue(cacheKey, out DashboardViewModel? dashboardData))

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

        [HttpGet]
        public async Task<IActionResult> DownloadExcel(DateTime? startDate, DateTime? endDate, string department, string user)
        {
            startDate = startDate ?? DateTime.Today.AddDays(-30);
            endDate = endDate ?? DateTime.Today;
            endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);

            var admissionQuery = _context.AdmissionKPIs.AsNoTracking();
            var visaQuery = _context.VisaKPIs.AsNoTracking();

            admissionQuery = admissionQuery.Where(k => k.EntryDate >= startDate && k.EntryDate <= endDate);
            visaQuery = visaQuery.Where(k => k.EntryDate >= startDate && k.EntryDate <= endDate);

            if (user != "All users")
            {
                var selectedUser = await _userManager.FindByNameAsync(user);
                if (selectedUser != null)
                {
                    admissionQuery = admissionQuery.Where(k => k.UserId == selectedUser.Id);
                    visaQuery = visaQuery.Where(k => k.UserId == selectedUser.Id);
                }
            }

            if (department == "Admission")
            {
                visaQuery = visaQuery.Where(k => false);
            }
            else if (department == "Visa")
            {
                admissionQuery = admissionQuery.Where(k => false);
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("KPI Data");
                worksheet.Cell("A1").Value = "Date";
                worksheet.Cell("B1").Value = "Applications";
                worksheet.Cell("C1").Value = "Admission Consultations";
                worksheet.Cell("D1").Value = "Inquiries";
                worksheet.Cell("E1").Value = "Visa Consultations";
                worksheet.Cell("F1").Value = "Conversions";

                var row = 2;
                foreach (var date in admissionQuery.Select(k => k.EntryDate.Date).Distinct()
                    .Concat(visaQuery.Select(k => k.EntryDate.Date).Distinct())
                    .OrderBy(d => d))
                {
                    var admissionData = admissionQuery.FirstOrDefault(k => k.EntryDate.Date == date);
                    var visaData = visaQuery.FirstOrDefault(k => k.EntryDate.Date == date);

                    worksheet.Cell(row, 1).Value = date.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 2).Value = admissionData?.Applications ?? 0;
                    worksheet.Cell(row, 3).Value = admissionData?.Consultations ?? 0;
                    worksheet.Cell(row, 4).Value = visaData?.Inquiries ?? 0;
                    worksheet.Cell(row, 5).Value = visaData?.Consultations ?? 0;
                    worksheet.Cell(row, 6).Value = visaData?.Conversions ?? 0;
                    row++;
                }

                worksheet.Cell($"A{row + 2}").Value = "Summary Metrics";
                worksheet.Cell($"A{row + 3}").Value = "Total Applications";
                worksheet.Cell($"B{row + 3}").Value = admissionQuery.Sum(k => k.Applications);
                worksheet.Cell($"A{row + 4}").Value = "Total Consultations";
                worksheet.Cell($"B{row + 4}").Value = admissionQuery.Sum(k => k.Consultations) + visaQuery.Sum(k => k.Consultations);
                worksheet.Cell($"A{row + 5}").Value = "Visa Conversion Rate";
                worksheet.Cell($"B{row + 5}").Value = $"{(visaQuery.Sum(k => k.Inquiries) > 0 ? (double)visaQuery.Sum(k => k.Conversions) / visaQuery.Sum(k => k.Inquiries) * 100 : 0):F2}%";
                worksheet.Cell($"A{row + 6}").Value = "Top Performing User";
                worksheet.Cell($"B{row + 6}").Value = "N/A"; // Placeholder, will update with real data in future

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"KPI_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPdf(DateTime? startDate, DateTime? endDate, string department, string user)
        {
            startDate = startDate ?? DateTime.Today.AddDays(-30);
            endDate = endDate ?? DateTime.Today;
            endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);

            var admissionQuery = _context.AdmissionKPIs.AsNoTracking();
            var visaQuery = _context.VisaKPIs.AsNoTracking();

            admissionQuery = admissionQuery.Where(k => k.EntryDate >= startDate && k.EntryDate <= endDate);
            visaQuery = visaQuery.Where(k => k.EntryDate >= startDate && k.EntryDate <= endDate);

            if (user != "All users")
            {
                var selectedUser = await _userManager.FindByNameAsync(user);
                if (selectedUser != null)
                {
                    admissionQuery = admissionQuery.Where(k => k.UserId == selectedUser.Id);
                    visaQuery = visaQuery.Where(k => k.UserId == selectedUser.Id);
                }
            }

            if (department == "Admission")
            {
                visaQuery = visaQuery.Where(k => false);
            }
            else if (department == "Visa")
            {
                admissionQuery = admissionQuery.Where(k => false);
            }

            using (var stream = new System.IO.MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Create a bold font
                var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

                // Add title with bold font
                document.Add(new Paragraph($"KPI Report - {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.CENTER));

                var table = new Table(6, false);
                table.AddHeaderCell(new Cell().Add(new Paragraph("Date")));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Applications")));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Admission Consultations")));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Inquiries")));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Visa Consultations")));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Conversions")));

                foreach (var date in admissionQuery.Select(k => k.EntryDate.Date).Distinct()
                    .Concat(visaQuery.Select(k => k.EntryDate.Date).Distinct())
                    .OrderBy(d => d))
                {
                    var admissionData = admissionQuery.FirstOrDefault(k => k.EntryDate.Date == date);
                    var visaData = visaQuery.FirstOrDefault(k => k.EntryDate.Date == date);

                    table.AddCell(new Cell().Add(new Paragraph(date.ToString("yyyy-MM-dd"))));
                    table.AddCell(new Cell().Add(new Paragraph((admissionData?.Applications ?? 0).ToString())));
                    table.AddCell(new Cell().Add(new Paragraph((admissionData?.Consultations ?? 0).ToString())));
                    table.AddCell(new Cell().Add(new Paragraph((visaData?.Inquiries ?? 0).ToString())));
                    table.AddCell(new Cell().Add(new Paragraph((visaData?.Consultations ?? 0).ToString())));
                    table.AddCell(new Cell().Add(new Paragraph((visaData?.Conversions ?? 0).ToString())));
                }

                document.Add(table);

                // Add summary metrics with bold heading
                document.Add(new Paragraph("\nSummary Metrics")
                    .SetFont(boldFont));
                document.Add(new Paragraph($"Total Applications: {admissionQuery.Sum(k => k.Applications)}"));
                document.Add(new Paragraph($"Total Consultations: {admissionQuery.Sum(k => k.Consultations) + visaQuery.Sum(k => k.Consultations)}"));
                document.Add(new Paragraph($"Visa Conversion Rate: {(visaQuery.Sum(k => k.Inquiries) > 0 ? (double)visaQuery.Sum(k => k.Conversions) / visaQuery.Sum(k => k.Inquiries) * 100 : 0):F2}%"));
                document.Add(new Paragraph($"Top Performing User: N/A")); // Placeholder

                document.Close();
                return File(stream.ToArray(), "application/pdf", $"KPI_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
        }
    }
}
