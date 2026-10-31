# Prompt: Build a Classic Vehicle Service Management System Using ASP.NET

**Role:** Act as a senior ASP.NET developer with 10+ years of experience in ASP.NET, C#, database design, UI/UX design, and full-stack web development. Build a fully functional, professional, and beginner-friendly mini-project website called **AutoCare – Vehicle Service Management System**.

## 1. Project Overview

Develop a classic, elegant, and professional vehicle service management website using ASP.NET. The website should allow customers to explore vehicle services, register and log in, manage their vehicles, book services, and track their service requests.

Admins should have a separate dashboard to manage customers' service requests, approve or reject bookings, update service statuses, and manage available services.

The project should be suitable for a college mini-project submission. Keep the code clean, modular, easy to understand, and simple to maintain.

## 2. Technology Stack

Use the following technologies:

* **Frontend:** HTML5, CSS3, Bootstrap 5, JavaScript
* **Backend:** ASP.NET Core MVC with C#
* **Database:** SQL Server or SQL Server LocalDB
* **ORM:** Entity Framework Core
* **Authentication:** ASP.NET Core Identity or a simple secure role-based authentication implementation
* **Icons:** Font Awesome
* **Architecture:** MVC (Model-View-Controller)

Use a stable, compatible version of ASP.NET Core and its dependencies.

## 3. Website Design and Theme

Create a classic automotive service website with a premium and timeless appearance.

**Design direction:**

* Classic, elegant, minimal, and professional.
* Inspired by premium automotive service centers.
* Clean layout with balanced white space.
* Responsive design for desktops, tablets, and mobile devices.
* Simple navigation and clear visual hierarchy.

**Color palette:**

* Primary: Deep Navy Blue (#142B45)
* Secondary: Steel Blue (#3F6385)
* Background: Off White (#F7F8FA)
* Accent: Warm Gold (#C9A45C)
* Text: Charcoal (#242B33)
* Success: Muted Green
* Error: Muted Red

**Typography:**

* Headings: Playfair Display or another elegant serif font.
* Body: Inter, Lato, or another clean sans-serif font.

**UI elements:**

* Elegant navigation bar with the AutoCare logo.
* Professional hero section featuring a vehicle workshop or car servicing image.
* Classic service cards with subtle shadows.
* Rounded buttons with restrained hover effects.
* Elegant forms with proper labels and validation messages.
* Clean tables for service requests and admin management.
* Professional dashboard cards with simple icons.
* Consistent footer with contact information and quick links.

Avoid excessive animations, flashy gradients, unnecessary visual effects, and overly rounded modern UI components.

## 4. Website Pages and Functional Requirements

### Page 1: Home / Service List

Create an attractive homepage featuring:

* A navigation bar with Home, Services, About, Contact, Login, and Register.
* A professional hero section with the heading:
  "Reliable Vehicle Care, Every Mile."
* A short description about the vehicle service center.
* A prominent "Book a Service" button.
* A service list displaying available services and their charges.
* Service cards containing:

  * Service name
  * Description
  * Price
  * Relevant icon
  * Book Now button
* A section highlighting why customers should choose AutoCare.
* A footer with contact details, address, and social links.

Initially, include sample services such as:

* General Servicing – ₹1,499
* Engine Oil Replacement – ₹899
* Brake Inspection – ₹599
* Car Washing – ₹499
* Wheel Alignment – ₹799
* Full Vehicle Inspection – ₹1,199

Display services dynamically from the database wherever appropriate.

### Page 2: Login / Register

Create a secure authentication system for customers and admins.

**Registration:**

* Full Name
* Email
* Mobile Number
* Password
* Confirm Password

**Login:**

* Email
* Password
* Remember Me option

Requirements:

* Validate user inputs.
* Display clear error and success messages.
* Hash passwords securely.
* Implement role-based access control.
* Customers should be redirected to their customer dashboard after login.
* Admins should be redirected to their admin dashboard after login.
* Do not allow users to register themselves as admins.
* Protect admin pages from unauthorized access.

### Page 3: My Vehicles

Create a customer vehicle management page.

Customers should be able to:

* Add a new vehicle.
* View their registered vehicles.
* Edit vehicle details.
* Delete a vehicle.

**Vehicle fields:**

* Vehicle ID
* Vehicle Owner ID
* Vehicle Type (Car, Bike, Scooter, etc.)
* Brand
* Model
* Registration Number
* Manufacturing Year
* Fuel Type

Requirements:

* Associate each vehicle with its logged-in customer.
* Validate required fields.
* Prevent duplicate registration numbers.
* Show vehicle details in a clean table or card layout.
* Add confirmation before deleting a vehicle.

### Page 4: Book a Service

Create a professional service booking form.

**Booking fields:**

* Select Vehicle
* Select Service
* Preferred Service Date
* Preferred Time Slot
* Problem Description
* Additional Notes (optional)

Requirements:

* Only show vehicles belonging to the logged-in customer.
* Fetch service types and prices from the database.
* Automatically display the selected service price.
* Validate all required fields.
* Save the booking in the database.
* Assign a unique service request number.
* Set the initial status to "Pending".
* Display a success message after successful submission.

### Page 5: My Service Requests

Create a page where customers can track all their service bookings.

Display:

* Request Number
* Vehicle
* Service Name
* Booking Date
* Preferred Service Date
* Estimated Cost
* Current Status
* Admin Remarks
* Action buttons where applicable

**Service statuses:**

* Pending
* Approved
* Rejected
* In Progress
* Completed
* Cancelled

Requirements:

* Customers can only see their own service requests.
* Provide filters for status and date.
* Show status labels with appropriate colors.
* Display detailed information when the customer clicks a request.
* Allow customers to cancel pending requests, if appropriate.

### Page 6: Admin Service Requests

Create a separate admin dashboard.

**Dashboard overview:**

* Total Customers
* Total Vehicles
* Total Service Requests
* Pending Requests
* Completed Services

**Request management:**

Display all service requests in a professional data table.

Admin should be able to:

* View all service requests.
* View detailed customer and vehicle information.
* Approve pending requests.
* Reject requests with a reason.
* Update service status.
* Add admin remarks.
* Search by customer name or request number.
* Filter by service status and date.

Requirements:

* Implement secure admin-only access.
* Use confirmation messages for important actions.
* Update service statuses in the database.
* Ensure that status transitions are handled consistently.
* Display success and error notifications.

### Page 7: Manage Services / Invoice

Create a service management page for admins.

**Service management:**

* Add new services.
* Edit existing services.
* Delete services that are not currently associated with active bookings.
* View all available services.

**Service fields:**

* Service ID
* Service Name
* Description
* Price
* Estimated Duration
* Active/Inactive Status

**Optional simple invoice feature:**

Generate a basic invoice for completed services.

Invoice details:

* Invoice Number
* Customer Name
* Vehicle Details
* Service Name
* Service Charge
* Additional Charges (optional)
* Total Amount
* Invoice Date

Provide a clean printable invoice layout.

## 5. Database Design

Use Entity Framework Core Code First with the following entities:

**ApplicationUser**

* Id
* FullName
* Email
* PhoneNumber
* Role

**Vehicle**

* VehicleId
* UserId
* VehicleType
* Brand
* Model
* RegistrationNumber
* ManufacturingYear
* FuelType

**Service**

* ServiceId
* ServiceName
* Description
* Price
* EstimatedDuration
* IsActive

**ServiceRequest**

* RequestId
* RequestNumber
* UserId
* VehicleId
* ServiceId
* PreferredDate
* PreferredTime
* ProblemDescription
* Status
* AdminRemarks
* CreatedAt
* UpdatedAt

**Invoice** (optional)

* InvoiceId
* InvoiceNumber
* RequestId
* ServiceCharge
* AdditionalCharges
* TotalAmount
* InvoiceDate

Define appropriate primary keys, foreign keys, relationships, validation rules, and delete behaviors.

Ensure that historical service request records retain accurate service names and prices even if an admin later modifies the service catalog.

## 6. Navigation and Dashboard

**Customer dashboard:**

* Welcome message.
* Total vehicles.
* Total bookings.
* Pending bookings.
* Completed services.
* Quick action buttons for Add Vehicle and Book Service.
* Recent service requests.

**Admin dashboard:**

* Overview cards.
* Recent service requests.
* Pending request summary.
* Quick links to manage services, requests, and customers.

Use a shared layout for navigation and footer, with role-appropriate menu items.

## 7. Validation and Security

Implement the following:

* Server-side validation using data annotations and model validation.
* Client-side validation using unobtrusive validation.
* Secure password hashing.
* Role-based authorization.
* Anti-forgery token protection for forms.
* Protection against unauthorized record access.
* Parameterized database access through Entity Framework Core.
* Proper error handling.
* Safe deletion rules.
* Avoid storing sensitive data in plain text.

Never rely only on frontend validation for security.

## 8. User Experience

Make the website easy to use for someone who is not technically experienced.

Include:

* Clear navigation.
* Helpful validation messages.
* Confirmation dialogs for destructive actions.
* Success and error alerts.
* Empty-state messages when no records are available.
* Search and filter functionality where needed.
* Consistent button styles and spacing.
* Responsive data tables.
* Simple pagination if records become numerous.

## 9. Project Structure

Organize the ASP.NET MVC application using a clear folder structure:

* Models
* Views
* Controllers
* Data
* Services
* ViewModels
* wwwroot/css
* wwwroot/js
* wwwroot/images

Separate business logic from controllers wherever practical. Avoid putting all application logic into a single controller.

## 10. Sample Data and Initial Setup

Include seed data for:

* An initial admin account created securely during development setup.
* A few sample service types.
* Optional demo customer and vehicle data.

Document how to configure the database connection, apply migrations, create the database, and run the application.

Do not hardcode production credentials or expose passwords in source code.

## 11. Deliverables

Generate the project in a step-by-step and practical manner.

Provide:

1. Complete project structure.
2. Entity models.
3. Database context and relationships.
4. Authentication and role setup.
5. Controllers and business logic.
6. Razor views for all required pages.
7. CSS styling and JavaScript functionality.
8. Database migrations and sample data setup.
9. Instructions for running the application in Visual Studio.
10. A short explanation of how each module works.

**Important development instructions:**

* Build a genuinely functional application, not just a static UI prototype.
* Ensure all navigation links, forms, and CRUD operations work.
* Use consistent naming conventions.
* Write clean, readable code suitable for a BCA-level college project.
* Do not skip necessary files or replace functional code with pseudocode.
* Explain where each file belongs and how it connects with other files.
* Implement and test one module at a time.
* If the response becomes too long, divide the development into logical phases and wait for confirmation before moving to the next phase.

**Final goal:** Deliver a classic, polished, secure, and fully functional AutoCare Vehicle Service Management System using ASP.NET Core MVC, C#, and SQL Server, suitable for a college mini-project demonstration.
