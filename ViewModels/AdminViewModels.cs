using System.ComponentModel.DataAnnotations;

namespace AutoCare.ViewModels;

public class RequestStatusUpdateViewModel
{
    public int RequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string VehicleDetails { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal ServicePrice { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a status.")]
    [Display(Name = "Update Status")]
    public string NewStatus { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
    [Display(Name = "Admin Remarks / Reason")]
    public string? AdminRemarks { get; set; }

    [Display(Name = "Additional Charges (₹) - For parts/extras")]
    [Range(0, 100000, ErrorMessage = "Additional charges must be a positive number.")]
    public decimal AdditionalCharges { get; set; } = 0.00m;

    [StringLength(500)]
    [Display(Name = "Invoice Notes")]
    public string? InvoiceNotes { get; set; }
}

public class VehicleServiceViewModel
{
    public int ServiceId { get; set; }

    [Required(ErrorMessage = "Service name is required.")]
    [StringLength(100)]
    [Display(Name = "Service Name")]
    public string ServiceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required.")]
    [Range(1, 200000, ErrorMessage = "Price must be greater than 0.")]
    [Display(Name = "Price (₹)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Estimated duration is required (e.g. 1-2 Hours).")]
    [StringLength(50)]
    [Display(Name = "Estimated Duration")]
    public string EstimatedDuration { get; set; } = "1-2 Hours";

    [Required(ErrorMessage = "Icon class is required (e.g. fa-solid fa-wrench).")]
    [StringLength(100)]
    [Display(Name = "FontAwesome Icon Class")]
    public string IconClass { get; set; } = "fa-solid fa-wrench";

    [Display(Name = "Is Active?")]
    public bool IsActive { get; set; } = true;
}

public class CustomerItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime RegisteredDate { get; set; }
    public int VehicleCount { get; set; }
    public int RequestCount { get; set; }
}
