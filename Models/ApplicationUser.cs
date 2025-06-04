using Microsoft.AspNetCore.Identity;

namespace KPI_Dashboard.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}