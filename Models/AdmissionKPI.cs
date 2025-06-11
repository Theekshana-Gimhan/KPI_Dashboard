using System.ComponentModel.DataAnnotations;

namespace KPI_Dashboard.Models
{
    public class AdmissionKPI
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateTime EntryDate { get; set; }

        [Required(ErrorMessage = "Number of Applications is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Number of Applications must be non-negative.")]
        public int Applications { get; set; }

        [Required(ErrorMessage = "Number of Consultations is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Number of Consultations must be non-negative.")]
        public int Consultations { get; set; }

        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
    }
}