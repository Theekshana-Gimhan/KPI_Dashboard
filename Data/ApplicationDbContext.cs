using KPI_Dashboard.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

        public DbSet<KPITarget> KPITargets { get; set; }
        public DbSet<KPIAuditTrail> KPIAuditTrails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<KPITarget>(entity =>
            {
                entity.HasIndex(t => new { t.Department, t.StartDate, t.EndDate })
                      .IsUnique();

                // Removed: entity.HasMany(t => t.AuditLogs)
                //           .WithOne(a => a.Target)
                //           .HasForeignKey(a => a.TargetId)
                //           .OnDelete(DeleteBehavior.Cascade);
                // The navigation property 'AuditLogs' does not exist on KPITarget.
            });

            modelBuilder.Entity<KPIAuditTrail>(entity =>
            {
                entity.Property(a => a.Timestamp)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(a => a.Timestamp);
                entity.HasIndex(a => a.ActionType);
                entity.HasIndex(a => a.IsSuccess);
            });
        }

    }
}
