using System.Collections.Generic;

namespace KPI_Dashboard.Models
{
    public class DashboardViewModel
    {
        public List<AdmissionKPI> AdmissionKPIs { get; set; }
        public List<VisaKPI> VisaKPIs { get; set; }
    }
}