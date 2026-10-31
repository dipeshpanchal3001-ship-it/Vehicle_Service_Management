using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoCare.Models;

public class Invoice
{
    [Key]
    public int InvoiceId { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Invoice Number")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public int RequestId { get; set; }

    [ForeignKey(nameof(RequestId))]
    public virtual ServiceRequest? ServiceRequest { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Service Charge (₹)")]
    public decimal ServiceCharge { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Additional Charges (₹)")]
    public decimal AdditionalCharges { get; set; } = 0.00m;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Total Amount (₹)")]
    public decimal TotalAmount { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Invoice Date")]
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(30)]
    [Display(Name = "Payment Status")]
    public string PaymentStatus { get; set; } = "Paid"; // Paid, Pending

    [StringLength(500)]
    public string? Notes { get; set; }
}
