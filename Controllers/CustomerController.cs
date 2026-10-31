using AutoCare.Data;
using AutoCare.Models;
using AutoCare.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Controllers;

[Authorize(Roles = "Customer")]
public class CustomerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public CustomerController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    private string CurrentUserId => _userManager.GetUserId(User)!;

    // GET: /Customer/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var userId = CurrentUserId;
        var user = await _userManager.GetUserAsync(User);

        var vehicles = await _context.Vehicles
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        var requests = await _context.ServiceRequests
            .Include(r => r.Vehicle)
            .Include(r => r.Service)
            .Include(r => r.Invoice)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var viewModel = new CustomerDashboardViewModel
        {
            CustomerName = user?.FullName ?? "Customer",
            TotalVehicles = vehicles.Count,
            TotalBookings = requests.Count,
            PendingBookings = requests.Count(r => r.Status == ServiceRequestStatus.Pending),
            CompletedServices = requests.Count(r => r.Status == ServiceRequestStatus.Completed),
            Vehicles = vehicles,
            RecentRequests = requests.Take(5).ToList()
        };

        return View(viewModel);
    }

    // GET: /Customer/Vehicles
    public async Task<IActionResult> Vehicles()
    {
        var userId = CurrentUserId;
        var vehicles = await _context.Vehicles
            .Include(v => v.ServiceRequests)
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        return View(vehicles);
    }

    // GET: /Customer/AddVehicle
    [HttpGet]
    public IActionResult AddVehicle()
    {
        return View(new VehicleViewModel());
    }

    // POST: /Customer/AddVehicle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVehicle(VehicleViewModel vehicleVm)
    {
        if (!ModelState.IsValid)
        {
            return View(vehicleVm);
        }

        var regNumber = vehicleVm.RegistrationNumber.Trim().ToUpper();

        // Check if registration number already exists
        var exists = await _context.Vehicles.AnyAsync(v => v.RegistrationNumber == regNumber);
        if (exists)
        {
            ModelState.AddModelError(nameof(vehicleVm.RegistrationNumber), "A vehicle with this registration number is already registered in the system.");
            return View(vehicleVm);
        }

        var vehicle = new Vehicle
        {
            UserId = CurrentUserId,
            VehicleType = vehicleVm.VehicleType,
            Brand = vehicleVm.Brand.Trim(),
            Model = vehicleVm.Model.Trim(),
            RegistrationNumber = regNumber,
            ManufacturingYear = vehicleVm.ManufacturingYear,
            FuelType = vehicleVm.FuelType,
            CreatedAt = DateTime.UtcNow
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Vehicle '{vehicle.Brand} {vehicle.Model}' added successfully!";
        return RedirectToAction(nameof(Vehicles));
    }

    // GET: /Customer/EditVehicle/{id}
    [HttpGet]
    public async Task<IActionResult> EditVehicle(int id)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == id && v.UserId == CurrentUserId);
        if (vehicle == null)
        {
            TempData["ErrorMessage"] = "Vehicle not found or you do not have permission to edit it.";
            return RedirectToAction(nameof(Vehicles));
        }

        var vm = new VehicleViewModel
        {
            VehicleId = vehicle.VehicleId,
            VehicleType = vehicle.VehicleType,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            RegistrationNumber = vehicle.RegistrationNumber,
            ManufacturingYear = vehicle.ManufacturingYear,
            FuelType = vehicle.FuelType
        };

        return View(vm);
    }

    // POST: /Customer/EditVehicle/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditVehicle(int id, VehicleViewModel vehicleVm)
    {
        if (!ModelState.IsValid)
        {
            return View(vehicleVm);
        }

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == id && v.UserId == CurrentUserId);
        if (vehicle == null)
        {
            TempData["ErrorMessage"] = "Vehicle not found.";
            return RedirectToAction(nameof(Vehicles));
        }

        var regNumber = vehicleVm.RegistrationNumber.Trim().ToUpper();
        var duplicate = await _context.Vehicles.AnyAsync(v => v.RegistrationNumber == regNumber && v.VehicleId != id);
        if (duplicate)
        {
            ModelState.AddModelError(nameof(vehicleVm.RegistrationNumber), "Another vehicle with this registration number already exists.");
            return View(vehicleVm);
        }

        vehicle.VehicleType = vehicleVm.VehicleType;
        vehicle.Brand = vehicleVm.Brand.Trim();
        vehicle.Model = vehicleVm.Model.Trim();
        vehicle.RegistrationNumber = regNumber;
        vehicle.ManufacturingYear = vehicleVm.ManufacturingYear;
        vehicle.FuelType = vehicleVm.FuelType;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Vehicle details updated successfully!";
        return RedirectToAction(nameof(Vehicles));
    }

    // POST: /Customer/DeleteVehicle/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.ServiceRequests)
            .FirstOrDefaultAsync(v => v.VehicleId == id && v.UserId == CurrentUserId);

        if (vehicle == null)
        {
            TempData["ErrorMessage"] = "Vehicle not found.";
            return RedirectToAction(nameof(Vehicles));
        }

        var hasActiveRequests = vehicle.ServiceRequests.Any(r =>
            r.Status == ServiceRequestStatus.Pending ||
            r.Status == ServiceRequestStatus.Approved ||
            r.Status == ServiceRequestStatus.InProgress);

        if (hasActiveRequests)
        {
            TempData["ErrorMessage"] = "Cannot delete this vehicle because it has active service requests.";
            return RedirectToAction(nameof(Vehicles));
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Vehicle removed successfully.";
        return RedirectToAction(nameof(Vehicles));
    }

    // GET: /Customer/BookService
    [HttpGet]
    public async Task<IActionResult> BookService(int? serviceId = null, int? vehicleId = null)
    {
        var userId = CurrentUserId;

        var vehicles = await _context.Vehicles
            .Where(v => v.UserId == userId)
            .OrderBy(v => v.Brand)
            .ToListAsync();

        if (vehicles.Count == 0)
        {
            TempData["ErrorMessage"] = "Please add at least one vehicle to your account before booking a service.";
            return RedirectToAction(nameof(AddVehicle));
        }

        var activeServices = await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.ServiceId)
            .ToListAsync();

        var model = new BookServiceViewModel
        {
            VehicleId = vehicleId,
            ServiceId = serviceId,
            PreferredDate = DateTime.Today.AddDays(1),
            VehicleOptions = vehicles.Select(v => new SelectListItem
            {
                Value = v.VehicleId.ToString(),
                Text = $"{v.Brand} {v.Model} ({v.RegistrationNumber}) - {v.FuelType}",
                Selected = vehicleId.HasValue && v.VehicleId == vehicleId.Value
            }).ToList(),
            ServiceOptions = activeServices.Select(s => new ServiceDropdownItem
            {
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                Price = s.Price,
                EstimatedDuration = s.EstimatedDuration
            }).ToList()
        };

        return View(model);
    }

    // POST: /Customer/BookService
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookService(BookServiceViewModel model)
    {
        var userId = CurrentUserId;

        if (model.PreferredDate.Date == DateTime.Today &&
    model.PreferredDate.TimeOfDay <= DateTime.Now.TimeOfDay)
        {
            ModelState.AddModelError(
                nameof(model.PreferredDate),
                "Please select a future service time."
            );
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(model, userId);
            return View(model);
        }

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == model.VehicleId && v.UserId == userId);
        if (vehicle == null)
        {
            ModelState.AddModelError(nameof(model.VehicleId), "Invalid vehicle selected.");
            await PopulateDropdowns(model, userId);
            return View(model);
        }

        var service = await _context.Services.FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId && s.IsActive);
        if (service == null)
        {
            ModelState.AddModelError(nameof(model.ServiceId), "Invalid or inactive service selected.");
            await PopulateDropdowns(model, userId);
            return View(model);
        }

        // Generate unique service request number
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomSuffix = new Random().Next(1000, 9999);
        var requestNumber = $"AC-{today}-{randomSuffix}";

        // Ensure uniqueness
        while (await _context.ServiceRequests.AnyAsync(r => r.RequestNumber == requestNumber))
        {
            randomSuffix = new Random().Next(1000, 9999);
            requestNumber = $"AC-{today}-{randomSuffix}";
        }

        var serviceRequest = new ServiceRequest
        {
            RequestNumber = requestNumber,
            UserId = userId,
            VehicleId = vehicle.VehicleId,
            ServiceId = service.ServiceId,
            ServiceNameSnapshot = service.ServiceName,
            ServicePriceSnapshot = service.Price,
            PreferredDate = model.PreferredDate.Date,
            PreferredTime = model.PreferredTime,
            ProblemDescription = model.ProblemDescription.Trim(),
            AdditionalNotes = model.AdditionalNotes?.Trim(),
            Status = ServiceRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Service booking request #{requestNumber} submitted successfully! Our team will review and approve your booking shortly.";
        return RedirectToAction(nameof(MyRequests));
    }

    // GET: /Customer/MyRequests
    public async Task<IActionResult> MyRequests(string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var userId = CurrentUserId;
        var query = _context.ServiceRequests
            .Include(r => r.Vehicle)
            .Include(r => r.Service)
            .Include(r => r.Invoice)
            .Where(r => r.UserId == userId)
            .AsQueryable();

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

        var requests = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

        ViewBag.SelectedStatus = status ?? "All";
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        return View(requests);
    }

    // GET: /Customer/RequestDetails/{id}
    public async Task<IActionResult> RequestDetails(int id)
    {
        var userId = CurrentUserId;
        var request = await _context.ServiceRequests
            .Include(r => r.Vehicle)
            .Include(r => r.Service)
            .Include(r => r.Invoice)
            .FirstOrDefaultAsync(r => r.RequestId == id && r.UserId == userId);

        if (request == null)
        {
            TempData["ErrorMessage"] = "Service request not found.";
            return RedirectToAction(nameof(MyRequests));
        }

        return View(request);
    }

    // POST: /Customer/CancelRequest/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelRequest(int id)
    {
        var userId = CurrentUserId;
        var request = await _context.ServiceRequests.FirstOrDefaultAsync(r => r.RequestId == id && r.UserId == userId);

        if (request == null)
        {
            TempData["ErrorMessage"] = "Service request not found.";
            return RedirectToAction(nameof(MyRequests));
        }

        if (request.Status != ServiceRequestStatus.Pending)
        {
            TempData["ErrorMessage"] = "Only pending service requests can be cancelled.";
            return RedirectToAction(nameof(MyRequests));
        }

        request.Status = ServiceRequestStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;
        request.AdminRemarks = "Cancelled by customer.";

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Service request #{request.RequestNumber} has been cancelled.";
        return RedirectToAction(nameof(MyRequests));
    }

    // GET: /Customer/Invoice/{id}
    public async Task<IActionResult> Invoice(int id)
    {
        var userId = CurrentUserId;
        var request = await _context.ServiceRequests
            .Include(r => r.Vehicle)
            .Include(r => r.User)
            .Include(r => r.Invoice)
            .FirstOrDefaultAsync(r => r.RequestId == id && r.UserId == userId);

        if (request == null || request.Invoice == null)
        {
            TempData["ErrorMessage"] = "Invoice not found or service has not been completed yet.";
            return RedirectToAction(nameof(MyRequests));
        }

        return View(request);
    }

    private async Task PopulateDropdowns(BookServiceViewModel model, string userId)
    {
        var vehicles = await _context.Vehicles
            .Where(v => v.UserId == userId)
            .OrderBy(v => v.Brand)
            .ToListAsync();

        var activeServices = await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.ServiceId)
            .ToListAsync();

        model.VehicleOptions = vehicles.Select(v => new SelectListItem
        {
            Value = v.VehicleId.ToString(),
            Text = $"{v.Brand} {v.Model} ({v.RegistrationNumber}) - {v.FuelType}",
            Selected = model.VehicleId.HasValue && v.VehicleId == model.VehicleId.Value
        }).ToList();

        model.ServiceOptions = activeServices.Select(s => new ServiceDropdownItem
        {
            ServiceId = s.ServiceId,
            ServiceName = s.ServiceName,
            Price = s.Price,
            EstimatedDuration = s.EstimatedDuration
        }).ToList();
    }

    // GET: /Customer/Profile
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
            Role = "Customer",
            MemberSince = user.CreatedAt
        };

        return View(model);
    }

    // POST: /Customer/Profile
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
            model.Role = "Customer";
            model.MemberSince = user.CreatedAt;
            return View(model);
        }

        // Validate mobile number uniqueness against other accounts
        var phoneInUse = await _userManager.Users.AnyAsync(u => u.PhoneNumber == model.PhoneNumber && u.Id != user.Id);
        if (phoneInUse)
        {
            ModelState.AddModelError(nameof(model.PhoneNumber), "This mobile number is already registered with another account.");
            model.Role = "Customer";
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
                model.Role = "Customer";
                model.MemberSince = user.CreatedAt;
                return View(model);
            }

            user.Email = emailTrimmed;
            user.UserName = emailTrimmed;
        }

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = model.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Your profile has been updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        model.Role = "Customer";
        model.MemberSince = user.CreatedAt;
        return View(model);
    }

    // POST: /Customer/ChangePassword
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
            TempData["SuccessMessage"] = "Your password has been changed successfully!";
            return RedirectToAction(nameof(Profile));
        }

        TempData["PasswordErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
        return RedirectToAction(nameof(Profile));
    }
}
