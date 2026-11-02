# 🚗 AutoCare — Vehicle Service Management System

> A web application built with **ASP.NET Core MVC** where customers can register their vehicles, book services and track progress online, while service-centre staff manage bookings, services and invoices from an admin dashboard.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-MVC-239120?logo=csharp&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-Code%20First-512BD4)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?logo=bootstrap&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?logo=microsoftsqlserver&logoColor=white)
![Type](https://img.shields.io/badge/Type-Academic%20Project-green)

---

## 📌 Overview

In a traditional workshop, bookings are taken by phone or walk-in and written in paper registers. Customers can't easily see prices, check whether their vehicle is ready, or look up past services.

**AutoCare** moves this process online. Customers book and track services from their own account, and administrators handle every request from a protected dashboard — all backed by a single database that keeps customer, vehicle, service, booking and invoice records together.

---

## ✨ Features

### 👤 Visitor
- Home page with hero section, service catalogue with prices, and a "Why choose AutoCare" section
- Register and log in

### 🧑‍🔧 Customer
- Register, log in (with *Remember Me*) and log out
- **Dashboard** with vehicle and booking summaries and recent requests
- **My Vehicles** — add, view, edit and delete vehicles (duplicate registration numbers are rejected; vehicles used in active bookings can't be deleted)
- **Book a Service** — choose a vehicle, service, preferred date and time slot; price shown automatically; a unique request number is generated
- **My Service Requests** — filter by status and date, view details and admin remarks, cancel requests while still *Pending*

### 🛡️ Administrator
- **Admin dashboard** with counts of customers, vehicles, requests, pending and completed services
- **Request management** — search by customer name or request number, filter by status and date, approve or reject (with reason), update status and add remarks
- **Manage Services** — add, edit, delete, activate or deactivate services (services linked to active bookings can only be deactivated)
- **Invoices** — generate a printable invoice for completed services

### 🔐 Security
- ASP.NET Core Identity with password hashing and role-based authorization (`Customer`, `Admin`)
- Users cannot register themselves as administrators
- Client- and server-side validation, anti-forgery tokens on forms
- Customers can only see and modify their own records
- Admin pages protected with `[Authorize(Roles = "Admin")]`
- Parameterized database access through EF Core

---

## 🔄 Service Request Lifecycle

| Status | Meaning |
| --- | --- |
| **Pending** | Submitted, waiting for the administrator's decision |
| **Approved** | Booking accepted by the administrator |
| **Rejected** | Booking declined with a recorded reason |
| **In Progress** | Work on the vehicle has started |
| **Completed** | Service finished; an invoice can be generated |
| **Cancelled** | Cancelled by the customer while pending |

Status changes follow a defined sequence that is validated on the server.

---

## 🛠️ Tech Stack

| Layer | Technology |
| --- | --- |
| Framework | ASP.NET Core MVC (.NET 8) |
| Language | C# |
| Views | Razor (`.cshtml`) |
| Front end | HTML5, CSS3, JavaScript, Bootstrap 5, Font Awesome |
| ORM | Entity Framework Core (Code First with migrations) |
| Database | SQL Server LocalDB |
| Authentication | ASP.NET Core Identity |
| IDE | Visual Studio / Command Prompt |

---

## 📁 Project Structure

```
AutoCare/
├── Controllers/
│   ├── AccountController.cs          # Register, login, logout, access denied
│   ├── HomeController.cs             # Home page and service catalogue
│   ├── DashboardController.cs        # Customer dashboard
│   ├── VehiclesController.cs         # Customer vehicle CRUD
│   ├── ServiceRequestsController.cs  # Booking, my requests, details, cancel
│   ├── AdminController.cs            # Admin dashboard and request management
│   ├── ManageServicesController.cs   # Admin service management
│   └── InvoicesController.cs         # Invoice generation and display
├── Data/
│   ├── ApplicationDbContext.cs       # EF Core context (Identity + app entities)
│   └── DbInitializer.cs              # Seeds roles, admin account, sample services
├── Models/
│   ├── ApplicationUser.cs
│   ├── Vehicle.cs
│   ├── Service.cs
│   ├── ServiceRequest.cs
│   └── Invoice.cs
├── Services/                         # Business logic kept out of controllers
│   ├── VehicleService.cs
│   ├── BookingService.cs
│   ├── ServiceRequestManager.cs
│   └── InvoiceService.cs
├── ViewModels/
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── VehicleViewModel.cs
│   ├── BookServiceViewModel.cs
│   └── DashboardViewModel.cs
├── Views/
│   ├── Account/  Home/  Dashboard/  Vehicles/
│   ├── ServiceRequests/  Admin/  ManageServices/  Invoices/
│   ├── Shared/
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── Migrations/
├── wwwroot/
│   ├── css/site.css
│   ├── js/site.js
│   └── images/
├── appsettings.json                  # Config and LocalDB connection string
├── Program.cs                        # Service registration, middleware, seeding
├── AutoCare.csproj
├── AutoCare.sln
└── README.md
```

---

## 🗄️ Database Design

### Entity relationships

| Relationship | Description |
| --- | --- |
| `ApplicationUser` → `Vehicle` | One customer owns many vehicles |
| `ApplicationUser` → `ServiceRequest` | One customer makes many requests |
| `Vehicle` → `ServiceRequest` | One vehicle can have many requests over time |
| `Service` → `ServiceRequest` | One service can be used in many requests |
| `ServiceRequest` → `Invoice` | A completed request can have one invoice |

### Main entities

| Entity | Key fields |
| --- | --- |
| **Vehicle** | `VehicleId`, `UserId`, `VehicleType`, `Brand`, `Model`, `RegistrationNumber` (unique), `ManufacturingYear`, `FuelType` |
| **Service** | `ServiceId`, `ServiceName`, `Description`, `Price`, `EstimatedDuration`, `IsActive` |
| **ServiceRequest** | `RequestId`, `RequestNumber`, `UserId`, `VehicleId`, `ServiceId`, `PreferredDate`, `PreferredTime`, `ProblemDescription`, `Status`, `AdminRemarks`, `CreatedAt`, `UpdatedAt` |
| **Invoice** | `InvoiceId`, `InvoiceNumber`, `RequestId`, `ServiceCharge`, `AdditionalCharges`, `TotalAmount`, `InvoiceDate` |
| **ApplicationUser** | `Id`, `FullName`, `Email`, `PhoneNumber`, `Role` (Customer / Admin) |

> The service name and price at the time of booking are stored with each request, so old records stay accurate even if the catalogue changes later.

### Sample seed services

| Service | Price |
| --- | ---: |
| General Servicing | ₹1,499 |
| Engine Oil Replacement | ₹899 |
| Brake Inspection | ₹599 |
| Car Washing | ₹499 |
| Wheel Alignment | ₹799 |
| Full Vehicle Inspection | ₹1,199 |

---

## ✅ Prerequisites

- Windows (documented local setup)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (included with Visual Studio)
- A modern browser (Chrome, Edge or Firefox)
- Internet access is helpful for restoring NuGet packages and loading fonts/icons

**Recommended hardware:** dual-core processor, 4 GB RAM minimum (8 GB+ recommended)

---

## 🚀 Getting Started

### Option 1 — Command Prompt

```bash
# 1. Clone the repository
git clone https://github.com/dipeshpanchal3001-ship-it/Vehicle_Service_Management.git
cd Vehicle_Service_Management/AutoCare

# 2. Check the installed SDKs
dotnet --list-sdks

# 3. Restore NuGet packages
dotnet restore

# 4. Install the EF Core CLI tool (only needed once)
dotnet tool install --global dotnet-ef

# 5. Create / update the database
dotnet ef database update

# 6. Run the application
dotnet run
```

Keep the terminal open and open the exact `localhost` URL printed by `dotnet run` in your browser.

### Option 2 — Visual Studio

1. Open `AutoCare.sln`.
2. In **Package Manager Console**, run `Update-Database`.
3. Press **F5** to run.

> If you use a SQL Server instance other than LocalDB, update the connection string in `appsettings.json` first.

### 🔑 Admin account

The seed routine (`DbInitializer.cs`) creates the `Admin` and `Customer` roles, an initial admin account and the sample services during development setup. Admin credentials should be supplied through configuration (e.g. user-secrets or environment variables) and **not** stored in source code.

---


## ⚠️ Limitations

- No online payment — invoices record amounts, payment is handled at the service centre
- No email/SMS notifications
- Time slot is a request only; no slot-availability or capacity checking
- No mechanic assignment or spare-parts inventory
- No upload of vehicle photos or documents
- No reports or analytics
- Single service centre only (no multi-branch support)
- Basic invoice with no tax breakdown or discounts
- Initial admin account and local connection settings are for development only and must be secured before any production deployment

---

## 🌱 Future Enhancements

- Online payment gateway with payment status on invoices
- Email / SMS notifications for approval, rejection and completion
- Time-slot availability checking and calendar view
- Mechanic assignment and per-vehicle service history
- Vehicle image and document upload with validation
- Reports and charts (revenue, popular services, monthly bookings)
- Tax calculation, discounts and downloadable PDF invoices
- Multiple branches and service reminders (by date or mileage)
- Secure secrets management, production database and HTTPS deployment
- Improved accessibility and responsive refinements

---

## 👨‍🎓 Author

**Dipesh Panchal**
BCA (Honours) — Asia Pacific BCA College, Gujarat University
Academic Year: 2026–2027

**Project Guide:** Mrs. Naina Asnani

---

## 📚 References

- [ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core/mvc/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity)
- [.NET Documentation](https://learn.microsoft.com/dotnet/)
- [Bootstrap 5.3](https://getbootstrap.com/docs/5.3/)
- [Font Awesome](https://fontawesome.com/docs)
- [SQL Server Documentation](https://learn.microsoft.com/sql/)

---

## 📝 Disclaimer

AutoCare is an academic demonstration and starter application. It is not presented as a production-ready public service.
