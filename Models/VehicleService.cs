using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoCare.Models;

[Table("Services")]
public class VehicleService
{
    [Key]
    public int ServiceId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Service Name")]
    public string ServiceName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0, 100000)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Estimated Duration")]
    public string EstimatedDuration { get; set; } = "1-2 Hours";

    [StringLength(100)]
    [Display(Name = "Icon Class")]
    public string IconClass { get; set; } = "fa-solid fa-wrench";

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
