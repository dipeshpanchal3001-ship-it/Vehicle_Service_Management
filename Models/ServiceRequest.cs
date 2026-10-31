using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoCare.Models;

public class ServiceRequest
{
    [Key]
    public int RequestId { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Request Number")]
    public string RequestNumber { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }

    [Required]
    public int VehicleId { get; set; }

    [ForeignKey(nameof(VehicleId))]
    public virtual Vehicle? Vehicle { get; set; }

    public int? ServiceId { get; set; }

    [ForeignKey(nameof(ServiceId))]
    public virtual VehicleService? Service { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Service Name")]
    public string ServiceNameSnapshot { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Estimated Cost")]
    public decimal ServicePriceSnapshot { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Preferred Service Date")]
    public DateTime PreferredDate { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Preferred Time Slot")]
    public string PreferredTime { get; set; } = "Morning (09:00 AM - 12:00 PM)";

    [Required]
    [StringLength(1000)]
    [Display(Name = "Problem Description")]
    public string ProblemDescription { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Additional Notes")]
    public string? AdditionalNotes { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = ServiceRequestStatus.Pending;

    [StringLength(500)]
    [Display(Name = "Admin Remarks")]
    public string? AdminRemarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual Invoice? Invoice { get; set; }
}

public static class ServiceRequestStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string InProgress = "In Progress";
    public const string Completed = "Completed";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = { Pending, Approved, InProgress, Completed, Rejected, Cancelled };
}
