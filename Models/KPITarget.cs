// File: Models/KPITarget.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace KPI_Dashboard.Models
{
    public class KPITarget
    {
        public int Id { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        public string PeriodType { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // Admissions fields
        public int? TargetApplications { get; set; }
        public int? TargetConsultations { get; set; }

        // Visa fields
        public int? TargetInquiries { get; set; }
        public int? TargetConversions { get; set; }

        // Audit fields
        public string? SetByUserId { get; set; }
        public DateTime? SetDate { get; set; }
    }
}
