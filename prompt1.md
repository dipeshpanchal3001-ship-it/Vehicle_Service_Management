# Modify and Improve My Existing ASP.NET Vehicle Service Management System

I have an existing ASP.NET Vehicle Service Management System mini-project for my college. I want you to inspect my entire existing project and implement the improvements listed below.

**IMPORTANT INSTRUCTIONS**

- Do not create a new project or rebuild the website from scratch.
- Do not change the existing technology stack, framework, database provider, project structure, or architecture unless a genuine technical issue requires it.
- First inspect the existing source code, database models, controllers, views, authentication, authorization, navigation, and configuration files.
- My project is expected to use ASP.NET Core MVC, C#, Entity Framework Core, and SQL Server/LocalDB, but verify the actual code before making assumptions.
- Preserve all existing working features, styling, layouts, and database records.
- Reuse existing models, controllers, services, and layouts wherever appropriate.
- Do not remove existing functionality just to simplify implementation.
- Make the actual code changes, not just recommendations or example snippets.
- Do not leave placeholder methods, TODO comments, incomplete implementations, or buttons that do nothing.
- Avoid unnecessary NuGet packages and database migrations.
- Before changing a database schema, determine whether a schema change is genuinely required.
- If a migration is required, create it properly and explain how to apply it without deleting existing data.

## 1. Add Edit Profile Functionality

Both Customer and Admin users should be able to view and edit their own profile information after logging in.

### Customer profile

- Add a "My Profile" or "Edit Profile" option to the customer navigation menu.
- Display the currently logged-in customer's saved information.
- Allow the customer to update appropriate personal details, such as:
  - Full name
  - Mobile number
  - Email address, if supported by the existing authentication system
  - Address, if the existing customer model supports it
- Pre-fill the form with the existing user's information.
- Validate all updated fields before saving.
- Save changes to the existing database record.
- Display a success message after a successful update.
- Display useful validation messages when the input is invalid.
- Preserve the customer's login session after updating the profile.
- Do not create a new customer record when updating an existing profile.

### Admin profile

- Provide a similar profile-editing option for the logged-in administrator.
- Allow the administrator to update permitted personal details without changing their role or permissions.
- Ensure that the administrator cannot accidentally promote themselves or another user to a higher privilege through the profile form.

### Security requirements

- Use the authenticated user's identity to determine which profile can be edited.
- Never trust a user ID supplied by a query string, hidden form field, or editable request field to authorize profile changes.
- Use appropriate view models and server-side validation.
- Prevent overposting and mass assignment of protected fields, including role, password hash, account status, and other authorization-related properties.
- Do not display or expose password hashes.
- If password changing is already supported, keep it separate from ordinary profile editing and use the appropriate password verification process.

## 2. Fix Customer Registration Mobile Number Validation

Currently, customer registration accepts a mobile number containing only nine digits. Fix this properly.

For this project, use Indian mobile-number validation.

### Required validation

- The mobile number must contain exactly 10 digits.
- The first digit must be between 6 and 9.
- Do not accept fewer than 10 digits.
- Do not accept more than 10 digits.
- Do not accept alphabetic characters or special characters in the mobile-number field.
- Do not accept an empty mobile number.
- Display a clear validation message, such as: "Enter a valid 10-digit Indian mobile number."
- Invalid mobile numbers must never be saved to the database.
- Apply the same validation during customer registration and profile editing.

Examples:

| Input | Expected result |
|---|---|
| 9876543210 | Valid |
| 6123456789 | Valid |
| 987654321 | Invalid — 9 digits |
| 98765432101 | Invalid — 11 digits |
| 1234567890 | Invalid — starts with 1 |
| 98765abc10 | Invalid — contains letters |
| Empty input | Invalid |

### Implementation requirements

1. Inspect the existing registration model, view model, controller, form, and database field.
2. Implement proper server-side validation using appropriate ASP.NET validation attributes or a custom validator.
3. Add client-side validation using the existing ASP.NET validation infrastructure where available.
4. Ensure that client-side validation is not the only protection; the server must reject invalid input even if client-side validation is bypassed.
5. Use an appropriate input type and HTML attributes to improve the form experience, including a 10-digit maximum length where applicable.
6. Ensure that the mobile number is stored and handled consistently as a string, not as a numeric value. A phone number is an identifier, not a number used for arithmetic.
7. Do not rely only on HTML `maxlength`, JavaScript, or the input's numeric keyboard to enforce validation.
8. If the current database column is numeric, evaluate the safest way to convert it to a string without losing existing records or breaking existing queries.
9. Check whether duplicate mobile numbers are permitted by the existing business rules. If uniqueness is required, enforce it safely at the server/database level and show a clear message.
10. Verify that registration and profile editing both reject invalid mobile numbers.

Do not implement an unnecessarily restrictive international phone-number validator because this particular project uses Indian mobile numbers.

## 3. Fix Active Navigation Highlighting

Currently, the navbar highlights "Admin Hub" by default, even when I visit another page. Fix this behavior.

### Expected behavior

- Highlight the navigation item corresponding to the page the user is currently visiting.
- If the customer visits "Home", highlight Home.
- If the customer visits "My Vehicles", highlight My Vehicles.
- If the customer visits "Book a Service", highlight Book a Service.
- If the customer visits "My Service Requests", highlight My Service Requests.
- If the customer visits "My Profile", highlight My Profile.
- If the administrator visits "Dashboard", highlight Dashboard.
- If the administrator visits "Manage Services", highlight Manage Services.
- If the administrator visits "Service Requests", highlight Service Requests.
- Apply the same behavior to any other existing navigation links.

### Technical requirements

- Inspect the shared layout files, partial views, navigation components, and existing CSS.
- Determine the active item dynamically from the current MVC route, controller/action, or the existing routing structure.
- Do not hardcode the active state to "Admin Hub".
- Ensure that only the appropriate current navigation item is highlighted.
- Handle nested routes and relevant action methods correctly. For example, the Edit action for a vehicle should still highlight the My Vehicles navigation item when appropriate.
- Ensure that customer and administrator navigation menus use their own correct active-state logic.
- Preserve the existing visual design, colors, icons, hover effects, and responsive layout.
- Verify that navigation highlighting works after page refreshes and when navigating directly to a URL.
- Ensure that the active state is accessible and distinguishable from ordinary hover styling.

## 4. Separate Public Website Pages from Admin Dashboard Navigation

Currently, the administrator's side appears to include Home, Contact Us, and Services. Review this structure and correct it.

### Public website

The public-facing website may contain:

- Home
- Services
- Contact Us
- Customer Login
- Customer Registration

These pages should remain accessible to visitors wherever that matches the existing design and business requirements.

### Customer area

The customer navigation should contain the relevant authenticated customer functions, such as:

- Dashboard or Home
- My Vehicles
- Book a Service
- My Service Requests
- My Profile
- Logout

Use the actual existing page names and routes wherever possible.

### Admin area

The administrator dashboard navigation should prioritize administrative functions, such as:

- Admin Dashboard
- Manage Services
- Manage Service Requests
- Manage Customers, if implemented
- Reports or other existing administrative functions
- Admin Profile
- Logout

Use only functions that actually exist or are implemented as part of this modification.

### Required corrections

- Do not display public-facing Home, Contact Us, or public Services links in the administrator's dashboard sidebar merely because they are present in the shared layout.
- If the admin needs to visit the public website, provide an appropriate "View Website" link or equivalent instead of mixing public pages into the admin management menu.
- Do not remove the public Home, Contact Us, or Services pages from the website.
- Do not remove administrator functionality for managing services if such functionality already exists.
- If the admin dashboard genuinely needs service-management information, keep it separate from the public Services page.
- Ensure that the public website, customer area, and admin area use the correct navigation layout or partial view.
- Avoid duplicating navigation markup unnecessarily.
- Preserve existing routes and functionality when separating the layouts.

Review the current architecture before deciding whether to use separate layouts, shared partial views, or role-based conditional navigation.

## 5. Perform a Full Project Audit and Fix Existing Errors

Inspect the whole project for errors, inconsistencies, and practical drawbacks that could affect a college demonstration or viva.

Do not assume an issue exists without checking the code. Identify actual problems and fix those that can be safely corrected without breaking existing features.

### A. Authentication and authorization

- Verify that protected pages require authentication.
- Ensure that customers cannot access admin-only pages by manually entering their URLs.
- Ensure that administrators cannot accidentally access or modify another customer's private data through customer-specific routes.
- Verify role-based authorization on controllers/actions or the equivalent authorization mechanism used by the existing project.
- Ensure that logout correctly ends the authenticated session.
- Verify that unauthenticated users are redirected to the appropriate login page.
- Check for insecure direct object references and user-ID manipulation.

### B. Vehicle management

- Verify that customers can create, view, edit, and delete their own vehicles if CRUD functionality is intended.
- Ensure one customer cannot view or modify another customer's vehicles.
- Validate vehicle fields such as registration number, make, model, and year according to the existing model and requirements.
- Prevent invalid or missing values from being saved.
- Check whether duplicate vehicle registration numbers should be prevented according to the existing business rules.

### C. Service booking and requests

- Verify that customers can book a service using valid vehicle and service records.
- Ensure the selected vehicle belongs to the currently logged-in customer.
- Validate required booking fields and dates.
- Prevent invalid service IDs, missing records, and inappropriate status changes.
- Ensure that administrators can approve, reject, or update service requests according to the existing workflow.
- Ensure customers can view only their own requests and relevant status information.
- Prevent customers from changing administrator-controlled approval statuses by manipulating form data.
- Ensure that deleting or modifying a service does not silently corrupt existing bookings.

### D. Service management and prices

- Validate service names, descriptions, charges, and other existing service fields.
- Ensure prices cannot be negative.
- Use appropriate decimal types and precision for monetary amounts.
- Check how existing bookings behave when a service's price or details change.
- Preserve historical booking information wherever the current business logic requires it.

### E. Database and data integrity

- Inspect entity relationships, foreign keys, required fields, indexes, and delete behaviors.
- Check for null-reference risks and missing-record errors.
- Verify that Entity Framework Core queries and database updates work correctly.
- Avoid duplicate database contexts, unnecessary queries, and unsafe manual SQL.
- Preserve existing customer, vehicle, service, and booking records.
- Do not delete or recreate the database just to make the modifications work.
- If a migration is required, explain the migration and database update commands.

### F. Forms and user experience

- Check every form for missing required-field validation.
- Ensure validation messages appear near the relevant fields.
- Verify success and error messages after create, update, delete, registration, login, and booking operations.
- Prevent accidental duplicate submissions where appropriate.
- Check that Cancel and Back links work.
- Verify that tables, forms, navigation, and dashboard cards work on desktop, tablet, and mobile.
- Check for broken links, missing views, incorrect routes, empty pages, and buttons that do not perform their advertised actions.

### G. Security and reliability

- Check that passwords are handled using the existing secure ASP.NET authentication/password-hashing system, not stored as plain text.
- Check anti-forgery protection on state-changing form submissions where applicable.
- Avoid exposing exception details, connection strings, secrets, or sensitive user information to ordinary users.
- Ensure server-side validation is applied even when client-side validation is bypassed.
- Handle missing database records and expected errors gracefully.
- Do not log passwords or other sensitive authentication data.

### H. Code quality

- Check for compilation errors, missing dependencies, incorrect namespaces, broken Razor syntax, and invalid model bindings.
- Remove unnecessary duplicate code only when safe.
- Follow the conventions of the existing project.
- Keep controllers, models/view models, views, and database operations organized appropriately.
- Do not introduce a new architecture or unnecessary design patterns just for these changes.

## 6. Testing Requirements

After implementing the changes, build and test the project.

At minimum, verify the following:

1. A customer can register with a valid 10-digit Indian mobile number.
2. Registration rejects nine-digit, eleven-digit, empty, alphabetic, and invalid-prefix mobile numbers.
3. A customer can update their own profile and see the saved changes after refreshing.
4. A customer cannot edit another customer's profile by changing a URL or submitted ID.
5. The administrator can update permitted profile details without changing their role.
6. The correct navbar item is highlighted on every relevant page.
7. The default Admin Hub highlight is removed from unrelated pages.
8. Public Home, Contact Us, and Services remain available as intended.
9. Public website navigation is separated from the admin dashboard navigation.
10. Customers cannot access administrative actions.
11. Vehicle ownership and service-request ownership are enforced.
12. Existing service booking, approval, rejection, and status-update workflows continue to work.
13. Existing database records remain intact.
14. All modified pages work at desktop and mobile sizes.
15. The project builds successfully without new compilation errors.

Where automated tests already exist, update or extend them. If practical, add focused tests for mobile-number validation and profile authorization.

Do not claim that a test passed unless you actually ran it. If you cannot run the application or connect to the database, clearly identify the tests that remain unverified.

## 7. Final Response Required

After completing the modifications, provide a concise implementation report containing:

1. The actual problems found in my existing code.
2. The files changed and what each change does.
3. The profile-editing workflow.
4. The mobile-number validation rules and where they are enforced.
5. How active navigation highlighting works now.
6. How public, customer, and admin navigation have been separated.
7. Any additional bugs or security issues fixed.
8. Any database migrations or configuration changes required.
9. Build and test results, distinguishing completed tests from unverified tests.
10. Any remaining limitations that I should know before my college viva.

If a requirement cannot be implemented safely without clarification, explain the specific issue instead of silently making a destructive change.

**Final priority:** Keep my existing Vehicle Service Management System working, implement the requested improvements properly, preserve the current data and design, and leave the project in a stable state for my college viva.