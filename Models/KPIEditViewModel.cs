namespace KPI_Dashboard.Models
{
    public class KPIEditViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        // "Admission" or "Visa"
        public DateTime EntryDate { get; set; }
        public int? Applications { get; set; }
        public int? Consultations { get; set; }
        public int? Inquiries { get; set; }
        public int? Conversions { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
