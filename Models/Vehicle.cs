using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoCare.Models;

public class Vehicle
{
    [Key]
    public int VehicleId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Vehicle Type")]
    public string VehicleType { get; set; } = "Car"; // Car, Bike, Scooter, SUV, Sedan, etc.

    [Required]
    [StringLength(50)]
    public string Brand { get; set; } = string.Empty; // Honda, Hyundai, Tata, etc.

    [Required]
    [StringLength(50)]
    public string Model { get; set; } = string.Empty; // City, Creta, Nexon, etc.

    [Required]
    [StringLength(20)]
    [Display(Name = "Registration Number")]
    public string RegistrationNumber { get; set; } = string.Empty; // e.g. MH 02 AB 1234

    [Required]
    [Range(1980, 2100)]
    [Display(Name = "Manufacturing Year")]
    public int ManufacturingYear { get; set; } = DateTime.Now.Year;

    [Required]
    [StringLength(20)]
    [Display(Name = "Fuel Type")]
    public string FuelType { get; set; } = "Petrol"; // Petrol, Diesel, Electric, Hybrid, CNG

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    [NotMapped]
    public string DisplayName => $"{Brand} {Model} ({RegistrationNumber})";
}
