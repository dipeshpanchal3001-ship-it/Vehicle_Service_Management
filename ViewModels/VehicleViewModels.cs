using System.ComponentModel.DataAnnotations;

namespace AutoCare.ViewModels;

public class VehicleViewModel
{
    public int VehicleId { get; set; }

    [Required(ErrorMessage = "Please select a vehicle type.")]
    [Display(Name = "Vehicle Type")]
    public string VehicleType { get; set; } = "Car";

    [Required(ErrorMessage = "Vehicle brand is required (e.g. Honda, Maruti, Tata).")]
    [StringLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vehicle model is required (e.g. City, Swift, Nexon).")]
    [StringLength(50)]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "Registration number is required.")]
    [StringLength(20, ErrorMessage = "Registration number cannot exceed 20 characters.")]
    [RegularExpression(@"^[A-Z0-9\s-]+$", ErrorMessage = "Please enter a valid vehicle registration number format.")]
    [Display(Name = "Registration Number")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Manufacturing year is required.")]
    [Range(1990, 2030, ErrorMessage = "Please enter a valid manufacturing year between 1990 and current year.")]
    [Display(Name = "Manufacturing Year")]
    public int ManufacturingYear { get; set; } = DateTime.Now.Year;

    [Required(ErrorMessage = "Please select a fuel type.")]
    [Display(Name = "Fuel Type")]
    public string FuelType { get; set; } = "Petrol";
}
