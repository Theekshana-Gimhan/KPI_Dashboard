public class KPITarget
{
    public int Id { get; set; }
    public string Department { get; set; } = string.Empty; // "Admissions" or "Visa"
    public string KPIName { get; set; } = string.Empty;    // e.g., "Applications", "Consultations", etc.
    public int TargetValue { get; set; }
}
