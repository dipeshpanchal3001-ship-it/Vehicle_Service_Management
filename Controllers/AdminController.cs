using AutoCare.Data;
using AutoCare.Models;
using AutoCare.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: /Admin/Dashboard
public async Task<IActionResult> Dashboard()
{
    // 1. Get counts efficiently using database-side queries
    var totalCustomers = (await _userManager.GetUsersInRoleAsync("Customer")).Count;
    var totalVehicles = await _context.Vehicles.CountAsync();
    
    var totalRequests = await _context.ServiceRequests.CountAsync();
    var pendingRequests = await _context.ServiceRequests.CountAsync(r => r.Status == ServiceRequestStatus.Pending);
    var inProgressRequests = await _context.ServiceRequests.CountAsync(r => r.Status == ServiceRequestStatus.InProgress);
    var completedRequests = await _context.ServiceRequests.CountAsync(r => r.Status == ServiceRequestStatus.Completed);

    // 2. Fix for SQLite decimal sum error
    var invoiceAmounts = await _context.Invoices.Select(i => i.TotalAmount).ToListAsync();
    var totalRevenue = invoiceAmounts.Sum();

    // 3. Fetch only the recent requests needed for display
    var recentRequests = await _context.ServiceRequests
        .Include(r => r.User)
        .Include(r => r.Vehicle)
        .Include(r => r.Service)
        .Include(r => r.Invoice)
        .OrderByDescending(r => r.CreatedAt)
        .Take(7)
        .ToListAsync();

    // 4. Map to ViewModel
    var viewModel = new AdminDashboardViewModel
    {
        TotalCustomers = totalCustomers,
        TotalVehicles = totalVehicles,
        TotalRequests = totalRequests,
        PendingRequests = pendingRequests,
        InProgressRequests = inProgressRequests,
        CompletedRequests = completedRequests,
        TotalRevenue = totalRevenue,
        RecentRequests = recentRequests
    };

    return View(viewModel);
}

    // GET: /Admin/Requests
    public async Task<IActionResult> Requests(string? search = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.ServiceRequests
            .Include(r => r.User)
            .Include(r => r.Vehicle)
            .Include(r => r.Service)
            .Include(r => r.Invoice)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchClean = search.Trim().ToLower();
            query = query.Where(r =>
                r.RequestNumber.ToLower().Contains(searchClean) ||
                (r.User != null && r.User.FullName.ToLower().Contains(searchClean)) ||
                (r.User != null && r.User.Email!.ToLower().Contains(searchClean)) ||
                (r.Vehicle != null && r.Vehicle.RegistrationNumber.ToLower().Contains(searchClean)) ||
                (r.Vehicle != null && r.Vehicle.Brand.ToLower().Contains(searchClean)) ||
                (r.Vehicle != null && r.Vehicle.Model.ToLower().Contains(searchClean)));
        }

        if (!string.IsNullOrEmpty(status) && status != "All")
        {
            query = query.Where(r => r.Status == status);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.PreferredDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.PreferredDate <= toDate.Value.Date);
        }

        var list = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

        ViewBag.SearchTerm = search;
        ViewBag.SelectedStatus = status ?? "All";
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        return View(list);
    }

    // GET: /Admin/RequestDetails/{id}
    public async Task<IActionResult> RequestDetails(int id)
    {
        var request = await _context.ServiceRequests
            .Include(r => r.User)
            .Include(r => r.Vehicle)
            .Include(r => r.Service)
            .Include(r => r.Invoice)
            .FirstOrDefaultAsync(r => r.RequestId == id);

        if (request == null)
        {
            TempData["ErrorMessage"] = "Service request not found.";
            return RedirectToAction(nameof(Requests));
        }

        var updateModel = new RequestStatusUpdateViewModel
        {
            RequestId = request.RequestId,
            RequestNumber = request.RequestNumber,
            CustomerName = request.User?.FullName ?? "N/A",
            VehicleDetails = request.Vehicle != null ? $"{request.Vehicle.Brand} {request.Vehicle.Model} ({request.Vehicle.RegistrationNumber})" : "N/A",
            ServiceName = request.ServiceNameSnapshot,
            ServicePrice = request.ServicePriceSnapshot,
            CurrentStatus = request.Status,
            NewStatus = request.Status,
            AdminRemarks = request.AdminRemarks
        };

        ViewBag.ServiceRequest = request;
        return View(updateModel);
    }

    // POST: /Admin/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(RequestStatusUpdateViewModel model)
    {
        var request = await _context.ServiceRequests
            .Include(r => r.User)
            .Include(r => r.Vehicle)
            .Include(r => r.Invoice)
            .FirstOrDefaultAsync(r => r.RequestId == model.RequestId);

        if (request == null)
        {
            TempData["ErrorMessage"] = "Service request not found.";
            return RedirectToAction(nameof(Requests));
        }

        if (model.NewStatus == ServiceRequestStatus.Rejected && string.IsNullOrWhiteSpace(model.AdminRemarks))
        {
            ModelState.AddModelError(nameof(model.AdminRemarks), "Please specify a reason when rejecting a service request.");
            ViewBag.ServiceRequest = request;
            return View("RequestDetails", model);
        }

        request.Status = model.NewStatus;
        request.AdminRemarks = model.AdminRemarks?.Trim();
        request.UpdatedAt = DateTime.UtcNow;

        // If status is Completed, generate or update Invoice
        if (model.NewStatus == ServiceRequestStatus.Completed)
        {
            if (request.Invoice == null)
            {
                var invoiceNumber = $"INV-{DateTime.UtcNow:yyyy}-{request.RequestId:D4}";
                var invoice = new Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    RequestId = request.RequestId,
                    ServiceCharge = request.ServicePriceSnapshot,
                    AdditionalCharges = model.AdditionalCharges,
                    TotalAmount = request.ServicePriceSnapshot + model.AdditionalCharges,
                    InvoiceDate = DateTime.Today,
                    PaymentStatus = "Paid",
                    Notes = model.InvoiceNotes?.Trim() ?? "Service completed satisfactorily. 60-day AutoCare service warranty applies."
                };
                _context.Invoices.Add(invoice);
            }
            else
            {
                request.Invoice.AdditionalCharges = model.AdditionalCharges;
                request.Invoice.TotalAmount = request.ServicePriceSnapshot + model.AdditionalCharges;
                if (!string.IsNullOrEmpty(model.InvoiceNotes))
                {
                    request.Invoice.Notes = model.InvoiceNotes.Trim();
                }
            }
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Service Request #{request.RequestNumber} status has been updated to '{model.NewStatus}'.";
        return RedirectToAction(nameof(Requests));
    }

    // POST: /Admin/QuickApprove/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickApprove(int id, string? remarks = null)
    {
        var request = await _context.ServiceRequests.FindAsync(id);
        if (request == null)
        {
            TempData["ErrorMessage"] = "Request not found.";
            return RedirectToAction(nameof(Requests));
        }

        request.Status = ServiceRequestStatus.Approved;
        request.AdminRemarks = !string.IsNullOrWhiteSpace(remarks) ? remarks : "Booking approved. Service bay scheduled.";
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Request #{request.RequestNumber} approved successfully.";
        return RedirectToAction(nameof(Requests));
    }

    // POST: /Admin/QuickReject/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickReject(int id, string rejectionReason)
    {
        if (string.IsNullOrWhiteSpace(rejectionReason))
        {
            TempData["ErrorMessage"] = "Rejection reason is required.";
            return RedirectToAction(nameof(Requests));
        }

        var request = await _context.ServiceRequests.FindAsync(id);
        if (request == null)
        {
            TempData["ErrorMessage"] = "Request not found.";
            return RedirectToAction(nameof(Requests));
        }

        request.Status = ServiceRequestStatus.Rejected;
        request.AdminRemarks = rejectionReason.Trim();
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Request #{request.RequestNumber} rejected.";
        return RedirectToAction(nameof(Requests));
    }

    // GET: /Admin/Services
    public async Task<IActionResult> Services()
    {
        var services = await _context.Services
            .Include(s => s.ServiceRequests)
            .OrderBy(s => s.ServiceName)
            .ToListAsync();

        return View(services);
    }

    // GET: /Admin/CreateService
    [HttpGet]
    public IActionResult CreateService()
    {
        return View(new VehicleServiceViewModel());
    }

    // POST: /Admin/CreateService
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateService(VehicleServiceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var service = new VehicleService
        {
            ServiceName = model.ServiceName.Trim(),
            Description = model.Description.Trim(),
            Price = model.Price,
            EstimatedDuration = model.EstimatedDuration.Trim(),
            IconClass = model.IconClass.Trim(),
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Service '{service.ServiceName}' created successfully!";
        return RedirectToAction(nameof(Services));
    }

    // GET: /Admin/EditService/{id}
    [HttpGet]
    public async Task<IActionResult> EditService(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
        {
            TempData["ErrorMessage"] = "Service not found.";
            return RedirectToAction(nameof(Services));
        }

        var model = new VehicleServiceViewModel
        {
            ServiceId = service.ServiceId,
            ServiceName = service.ServiceName,
            Description = service.Description,
            Price = service.Price,
            EstimatedDuration = service.EstimatedDuration,
            IconClass = service.IconClass,
            IsActive = service.IsActive
        };

        return View(model);
    }

    // POST: /Admin/EditService/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditService(int id, VehicleServiceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var service = await _context.Services.FindAsync(id);
        if (service == null)
        {
            TempData["ErrorMessage"] = "Service not found.";
            return RedirectToAction(nameof(Services));
        }

        service.ServiceName = model.ServiceName.Trim();
        service.Description = model.Description.Trim();
        service.Price = model.Price;
        service.EstimatedDuration = model.EstimatedDuration.Trim();
        service.IconClass = model.IconClass.Trim();
        service.IsActive = model.IsActive;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Service '{service.ServiceName}' updated successfully!";
        return RedirectToAction(nameof(Services));
    }

    // POST: /Admin/ToggleServiceStatus/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleServiceStatus(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
        {
            TempData["ErrorMessage"] = "Service not found.";
            return RedirectToAction(nameof(Services));
        }

        service.IsActive = !service.IsActive;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Service '{service.ServiceName}' is now {(service.IsActive ? "Active" : "Inactive")}.";
        return RedirectToAction(nameof(Services));
    }

    // POST: /Admin/DeleteService/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteService(int id)
    {
        var service = await _context.Services
            .Include(s => s.ServiceRequests)
            .FirstOrDefaultAsync(s => s.ServiceId == id);

        if (service == null)
        {
            TempData["ErrorMessage"] = "Service not found.";
            return RedirectToAction(nameof(Services));
        }

        var hasActiveRequests = service.ServiceRequests.Any(r =>
            r.Status == ServiceRequestStatus.Pending ||
            r.Status == ServiceRequestStatus.Approved ||
            r.Status == ServiceRequestStatus.InProgress);

        if (hasActiveRequests)
        {
            TempData["ErrorMessage"] = "Cannot delete this service because it has active service bookings. Consider marking it Inactive instead.";
            return RedirectToAction(nameof(Services));
        }

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Service deleted successfully.";
        return RedirectToAction(nameof(Services));
    }

    // GET: /Admin/Customers
    public async Task<IActionResult> Customers()
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");
        var customerIds = customers.Select(c => c.Id).ToList();

        var vehicleCounts = await _context.Vehicles
            .Where(v => customerIds.Contains(v.UserId))
            .GroupBy(v => v.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var requestCounts = await _context.ServiceRequests
            .Where(r => customerIds.Contains(r.UserId))
            .GroupBy(r => r.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var list = customers.Select(c => new CustomerItemViewModel
        {
            Id = c.Id,
            FullName = c.FullName,
            Email = c.Email ?? "N/A",
            PhoneNumber = c.PhoneNumber ?? "N/A",
            RegisteredDate = c.CreatedAt,
            VehicleCount = vehicleCounts.TryGetValue(c.Id, out var vc) ? vc : 0,
            RequestCount = requestCounts.TryGetValue(c.Id, out var rc) ? rc : 0
        }).OrderByDescending(c => c.RegisteredDate).ToList();

        return View(list);
    }

    // GET: /Admin/Invoice/{id}
    public async Task<IActionResult> Invoice(int id)
    {
        var request = await _context.ServiceRequests
            .Include(r => r.User)
            .Include(r => r.Vehicle)
            .Include(r => r.Invoice)
            .FirstOrDefaultAsync(r => r.RequestId == id);

        if (request == null || request.Invoice == null)
        {
            TempData["ErrorMessage"] = "Invoice not found for this service request.";
            return RedirectToAction(nameof(Requests));
        }

        return View(request);
    }

    // GET: /Admin/Profile
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = new EditProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Role = "Admin",
            MemberSince = user.CreatedAt
        };

        return View(model);
    }

    // POST: /Admin/Profile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(EditProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (model.PhoneNumber != null)
        {
            model.PhoneNumber = model.PhoneNumber.Trim();
        }

        if (!ModelState.IsValid)
        {
            model.Role = "Admin";
            model.MemberSince = user.CreatedAt;
            return View(model);
        }

        // Validate mobile number uniqueness against other accounts
        var phoneInUse = await _userManager.Users.AnyAsync(u => u.PhoneNumber == model.PhoneNumber && u.Id != user.Id);
        if (phoneInUse)
        {
            ModelState.AddModelError(nameof(model.PhoneNumber), "This mobile number is already registered with another account.");
            model.Role = "Admin";
            model.MemberSince = user.CreatedAt;
            return View(model);
        }

        // Validate email uniqueness if changed
        var emailTrimmed = model.Email.Trim();
        if (!string.Equals(user.Email, emailTrimmed, StringComparison.OrdinalIgnoreCase))
        {
            var emailInUse = await _userManager.FindByEmailAsync(emailTrimmed);
            if (emailInUse != null && emailInUse.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "This email address is already in use by another account.");
                model.Role = "Admin";
                model.MemberSince = user.CreatedAt;
                return View(model);
            }

            user.Email = emailTrimmed;
            user.UserName = emailTrimmed;
        }

        // Security check: strictly modify personal details, never alter roles or privileges
        user.FullName = model.FullName.Trim();
        user.PhoneNumber = model.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Administrator profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        model.Role = "Admin";
        model.MemberSince = user.CreatedAt;
        return View(model);
    }

    // POST: /Admin/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            TempData["PasswordErrorMessage"] = "Please fill in all required password fields correctly.";
            return RedirectToAction(nameof(Profile));
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Your administrator password has been changed successfully!";
            return RedirectToAction(nameof(Profile));
        }

        TempData["PasswordErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
        return RedirectToAction(nameof(Profile));
    }
}
