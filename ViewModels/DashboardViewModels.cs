using AutoCare.Models;

namespace AutoCare.ViewModels;

public class CustomerDashboardViewModel
{
    public string CustomerName { get; set; } = string.Empty;
    public int TotalVehicles { get; set; }
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int CompletedServices { get; set; }
    public List<Vehicle> Vehicles { get; set; } = new();
    public List<ServiceRequest> RecentRequests { get; set; } = new();
}

public class AdminDashboardViewModel
{
    public int TotalCustomers { get; set; }
    public int TotalVehicles { get; set; }
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int InProgressRequests { get; set; }
    public int CompletedRequests { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<ServiceRequest> RecentRequests { get; set; } = new();
}
