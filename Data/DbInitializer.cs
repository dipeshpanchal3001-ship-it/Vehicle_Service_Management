using AutoCare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // 1. Seed Roles
        string[] roles = { "Admin", "Customer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Admin User
        var adminEmail = "admin@autocare.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Administrator",
                PhoneNumber = "9999988888",
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // 3. Seed Demo Customer
        var customerEmail = "customer@autocare.com";
        var customerUser = await userManager.FindByEmailAsync(customerEmail);
        if (customerUser == null)
        {
            customerUser = new ApplicationUser
            {
                UserName = customerEmail,
                Email = customerEmail,
                EmailConfirmed = true,
                FullName = "Rahul Sharma",
                PhoneNumber = "9876543210",
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(customerUser, "Customer@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customerUser, "Customer");
            }
        }

        // 3b. Normalize any legacy phone numbers in the database to 10-digit Indian format
        var allUsers = await userManager.Users.ToListAsync();
        foreach (var u in allUsers)
        {
            if (!string.IsNullOrEmpty(u.PhoneNumber))
            {
                var digitsOnly = new string(u.PhoneNumber.Where(char.IsDigit).ToArray());
                if (digitsOnly.Length > 10 && digitsOnly.StartsWith("91"))
                {
                    digitsOnly = digitsOnly.Substring(2);
                }
                if (digitsOnly.Length == 10 && digitsOnly != u.PhoneNumber)
                {
                    u.PhoneNumber = digitsOnly;
                    await userManager.UpdateAsync(u);
                }
            }
        }

        // 4. Seed Services
        if (!await context.Services.AnyAsync())
        {
            var services = new List<VehicleService>
            {
                new()
                {
                    ServiceName = "General Servicing",
                    Description = "Comprehensive 50-point vehicle checkup, fluid top-ups, filter cleaning, and computer diagnostics.",
                    Price = 1499.00m,
                    EstimatedDuration = "3 Hours",
                    IconClass = "fa-solid fa-wrench",
                    IsActive = true
                },
                new()
                {
                    ServiceName = "Engine Oil Replacement",
                    Description = "High-grade synthetic engine oil change with premium oil filter replacement and level inspection.",
                    Price = 899.00m,
                    EstimatedDuration = "1 Hour",
                    IconClass = "fa-solid fa-oil-can",
                    IsActive = true
                },
                new()
                {
                    ServiceName = "Brake Inspection",
                    Description = "Front and rear brake pads inspection, rotor check, caliper cleaning, and hydraulic brake fluid check.",
                    Price = 599.00m,
                    EstimatedDuration = "1.5 Hours",
                    IconClass = "fa-solid fa-compact-disc",
                    IsActive = true
                },
                new()
                {
                    ServiceName = "Car Washing",
                    Description = "Exterior high-pressure foam wash, interior deep vacuuming, dashboard polishing, and tyre gloss dressing.",
                    Price = 499.00m,
                    EstimatedDuration = "45 Mins",
                    IconClass = "fa-solid fa-soap",
                    IsActive = true
                },
                new()
                {
                    ServiceName = "Wheel Alignment",
                    Description = "Computerized 3D laser wheel alignment, tyre pressure calibration, rotation, and dynamic wheel balancing.",
                    Price = 799.00m,
                    EstimatedDuration = "1 Hour",
                    IconClass = "fa-solid fa-dharmachakra",
                    IsActive = true
                },
                new()
                {
                    ServiceName = "Full Vehicle Inspection",
                    Description = "Thorough health check of engine, transmission, electricals, suspension, battery health, and exhaust system.",
                    Price = 1199.00m,
                    EstimatedDuration = "2.5 Hours",
                    IconClass = "fa-solid fa-clipboard-check",
                    IsActive = true
                }
            };

            await context.Services.AddRangeAsync(services);
            await context.SaveChangesAsync();
        }

        // 5. Seed Demo Vehicles for Customer
        if (customerUser != null && !await context.Vehicles.AnyAsync(v => v.UserId == customerUser.Id))
        {
            var vehicles = new List<Vehicle>
            {
                new()
                {
                    UserId = customerUser.Id,
                    VehicleType = "Car",
                    Brand = "Honda",
                    Model = "City V MT",
                    RegistrationNumber = "MH 02 AB 1234",
                    ManufacturingYear = 2022,
                    FuelType = "Petrol"
                },
                new()
                {
                    UserId = customerUser.Id,
                    VehicleType = "Bike",
                    Brand = "Royal Enfield",
                    Model = "Classic 350",
                    RegistrationNumber = "MH 02 CD 5678",
                    ManufacturingYear = 2023,
                    FuelType = "Petrol"
                },
                new()
                {
                    UserId = customerUser.Id,
                    VehicleType = "SUV",
                    Brand = "Hyundai",
                    Model = "Creta SX",
                    RegistrationNumber = "DL 01 XY 9999",
                    ManufacturingYear = 2024,
                    FuelType = "Diesel"
                }
            };

            await context.Vehicles.AddRangeAsync(vehicles);
            await context.SaveChangesAsync();
        }

        // 6. Seed Demo Service Requests
        if (customerUser != null && !await context.ServiceRequests.AnyAsync())
        {
            var customerVehicles = await context.Vehicles.Where(v => v.UserId == customerUser.Id).ToListAsync();
            var serviceList = await context.Services.ToListAsync();

            if (customerVehicles.Count >= 2 && serviceList.Count >= 4)
            {
                // Request 1: Completed with invoice
                var req1 = new ServiceRequest
                {
                    RequestNumber = "AC-20261001-0001",
                    UserId = customerUser.Id,
                    VehicleId = customerVehicles[0].VehicleId,
                    ServiceId = serviceList[0].ServiceId,
                    ServiceNameSnapshot = serviceList[0].ServiceName,
                    ServicePriceSnapshot = serviceList[0].Price,
                    PreferredDate = DateTime.Today.AddDays(-3),
                    PreferredTime = "Morning (09:00 AM - 12:00 PM)",
                    ProblemDescription = "Scheduled general periodic maintenance and checkup.",
                    AdditionalNotes = "Please check air conditioning cooling.",
                    Status = ServiceRequestStatus.Completed,
                    AdminRemarks = "Full 50-point service completed. AC filter replaced and refrigerant topped up.",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                };
                context.ServiceRequests.Add(req1);
                await context.SaveChangesAsync();

                var inv1 = new Invoice
                {
                    InvoiceNumber = "INV-2026-0001",
                    RequestId = req1.RequestId,
                    ServiceCharge = req1.ServicePriceSnapshot,
                    AdditionalCharges = 350.00m, // Wiper blades + AC cleaner
                    TotalAmount = req1.ServicePriceSnapshot + 350.00m,
                    InvoiceDate = DateTime.Today.AddDays(-2),
                    PaymentStatus = "Paid",
                    Notes = "Payment received via UPI. 60-day service warranty applied."
                };
                context.Invoices.Add(inv1);

                // Request 2: In Progress
                var req2 = new ServiceRequest
                {
                    RequestNumber = "AC-20261001-0002",
                    UserId = customerUser.Id,
                    VehicleId = customerVehicles[1].VehicleId,
                    ServiceId = serviceList[1].ServiceId,
                    ServiceNameSnapshot = serviceList[1].ServiceName,
                    ServicePriceSnapshot = serviceList[1].Price,
                    PreferredDate = DateTime.Today,
                    PreferredTime = "Afternoon (12:00 PM - 03:00 PM)",
                    ProblemDescription = "Engine oil change overdue by 500 km. Engine running slightly hot.",
                    AdditionalNotes = "Use full synthetic 15W-50 oil.",
                    Status = ServiceRequestStatus.InProgress,
                    AdminRemarks = "Old oil drained, engine flushed. New synthetic oil and filter being installed.",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow
                };
                context.ServiceRequests.Add(req2);

                // Request 3: Approved
                var req3 = new ServiceRequest
                {
                    RequestNumber = "AC-20261001-0003",
                    UserId = customerUser.Id,
                    VehicleId = customerVehicles[2].VehicleId,
                    ServiceId = serviceList[2].ServiceId,
                    ServiceNameSnapshot = serviceList[2].ServiceName,
                    ServicePriceSnapshot = serviceList[2].Price,
                    PreferredDate = DateTime.Today.AddDays(1),
                    PreferredTime = "Morning (09:00 AM - 12:00 PM)",
                    ProblemDescription = "Slight squeaking sound when applying front brakes at low speeds.",
                    Status = ServiceRequestStatus.Approved,
                    AdminRemarks = "Booking confirmed. Bay #3 reserved. Please bring the vehicle by 9:30 AM.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.ServiceRequests.Add(req3);

                // Request 4: Pending (Ready for Admin testing)
                var req4 = new ServiceRequest
                {
                    RequestNumber = "AC-20261001-0004",
                    UserId = customerUser.Id,
                    VehicleId = customerVehicles[0].VehicleId,
                    ServiceId = serviceList[4].ServiceId,
                    ServiceNameSnapshot = serviceList[4].ServiceName,
                    ServicePriceSnapshot = serviceList[4].Price,
                    PreferredDate = DateTime.Today.AddDays(2),
                    PreferredTime = "Evening (03:00 PM - 06:00 PM)",
                    ProblemDescription = "Mild steering pull towards left on expressway above 70 km/h.",
                    AdditionalNotes = "Check tyre tread wear as well.",
                    Status = ServiceRequestStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                context.ServiceRequests.Add(req4);

                await context.SaveChangesAsync();
            }
        }
    }
}
