using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutoCare.ViewModels;

public class BookServiceViewModel
{
    [Required(ErrorMessage = "Please select a vehicle from your registered vehicles.")]
    [Display(Name = "Select Vehicle")]
    public int? VehicleId { get; set; }

    [Required(ErrorMessage = "Please select a service.")]
    [Display(Name = "Select Service")]
    public int? ServiceId { get; set; }

    [Required(ErrorMessage = "Preferred service date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Preferred Service Date")]
    public DateTime PreferredDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Please select a preferred time slot.")]
    [Display(Name = "Preferred Time Slot")]
    public string PreferredTime { get; set; } = "Morning (09:00 AM - 12:00 PM)";

    [Required(ErrorMessage = "Please provide a brief description of the issue or requirement.")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    [Display(Name = "Problem Description / Service Notes")]
    public string ProblemDescription { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Additional notes cannot exceed 500 characters.")]
    [Display(Name = "Additional Notes (Optional)")]
    public string? AdditionalNotes { get; set; }

    // Helpers for dropdowns and dynamic pricing display
    public List<SelectListItem> VehicleOptions { get; set; } = new();
    public List<ServiceDropdownItem> ServiceOptions { get; set; } = new();
}

public class ServiceDropdownItem
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string EstimatedDuration { get; set; } = string.Empty;
}
