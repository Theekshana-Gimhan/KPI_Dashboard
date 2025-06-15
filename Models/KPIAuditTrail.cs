using KPI_Dashboard.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class KPIAuditTrail
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    [Required]
    [StringLength(20)]
    public string ActionType { get; set; } = "Update";

    [Required]
    [StringLength(100)]
    public string Action { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public int? TargetId { get; set; }

    [ForeignKey("TargetId")]
    public KPITarget? Target { get; set; }

    public bool IsSuccess { get; set; }

    [StringLength(500)]
    public string? ErrorMessage { get; set; }
}