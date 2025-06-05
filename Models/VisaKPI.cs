using System.ComponentModel.DataAnnotations;

namespace KPI_Dashboard.Models
{
    public class VisaKPI
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateTime EntryDate { get; set; }

        [Required(ErrorMessage = "Number of Inquiries is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Number of Inquiries must be non-negative.")]
        public int Inquiries { get; set; }

        [Required(ErrorMessage = "Number of Consultations is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Number of Consultations must be non-negative.")]
        public int Consultations { get; set; }

        [Required(ErrorMessage = "Number of Conversions is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Number of Conversions must be non-negative.")]
        public int Conversions { get; set; }

        public string UserId { get; set; }
    }
}