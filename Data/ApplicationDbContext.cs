using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KPI_Dashboard.Models;

namespace KPI_Dashboard.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AdmissionKPI> AdmissionKPIs { get; set; }
        public DbSet<VisaKPI> VisaKPIs { get; set; }
    }
}