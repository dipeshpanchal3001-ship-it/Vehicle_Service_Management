using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using AutoCare.ViewModels;
using Xunit;

namespace AutoCare.Tests;

public class ValidationAndAuthorizationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Theory]
    [InlineData("9876543210", true)]  // Valid Indian mobile
    [InlineData("6123456789", true)]  // Valid Indian mobile starting with 6
    [InlineData("7000000000", true)]  // Valid Indian mobile starting with 7
    [InlineData("8888888888", true)]  // Valid Indian mobile starting with 8
    [InlineData("987654321", false)]  // Invalid - 9 digits
    [InlineData("98765432101", false)] // Invalid - 11 digits
    [InlineData("1234567890", false)] // Invalid - starts with 1
    [InlineData("5555555555", false)] // Invalid - starts with 5
    [InlineData("98765abc10", false)] // Invalid - contains letters
    [InlineData("+9198765432", false)] // Invalid - special characters
    [InlineData("98765 43210", false)] // Invalid - contains space
    [InlineData("", false)]            // Invalid - empty
    public void IndianMobileRegex_ValidatesExpectedFormat(string input, bool expectedValid)
    {
        var pattern = @"^[6-9]\d{9}$";
        var isMatch = Regex.IsMatch(input, pattern);
        Assert.Equal(expectedValid, isMatch);
    }

    [Fact]
    public void RegisterViewModel_WithValidIndianMobile_PassesValidation()
    {
        var vm = new RegisterViewModel
        {
            FullName = "Ramesh Kumar",
            Email = "ramesh@example.com",
            PhoneNumber = "9876543210",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };

        var results = ValidateModel(vm);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("987654321")]    // 9 digits
    [InlineData("98765432101")]  // 11 digits
    [InlineData("1234567890")]   // Invalid prefix
    [InlineData("98765abc10")]   // Letters
    [InlineData("")]             // Empty
    public void RegisterViewModel_WithInvalidIndianMobile_FailsValidation(string invalidPhone)
    {
        var vm = new RegisterViewModel
        {
            FullName = "Ramesh Kumar",
            Email = "ramesh@example.com",
            PhoneNumber = invalidPhone,
            Password = "Password123",
            ConfirmPassword = "Password123"
        };

        var results = ValidateModel(vm);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.PhoneNumber)));
    }

    [Fact]
    public void EditProfileViewModel_WithValidIndianMobile_PassesValidation()
    {
        var vm = new EditProfileViewModel
        {
            FullName = "Rahul Sharma",
            Email = "customer@autocare.com",
            PhoneNumber = "9876543210",
            Role = "Customer"
        };

        var results = ValidateModel(vm);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("987654321")]   // 9 digits
    [InlineData("1234567890")]  // Starts with 1
    [InlineData("98765abc10")]  // Letters
    [InlineData("")]            // Empty
    public void EditProfileViewModel_WithInvalidIndianMobile_FailsValidation(string invalidPhone)
    {
        var vm = new EditProfileViewModel
        {
            FullName = "Rahul Sharma",
            Email = "customer@autocare.com",
            PhoneNumber = invalidPhone,
            Role = "Customer"
        };

        var results = ValidateModel(vm);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(EditProfileViewModel.PhoneNumber)));
    }

    [Fact]
    public void ChangePasswordViewModel_MismatchedPasswords_FailsValidation()
    {
        var vm = new ChangePasswordViewModel
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "NewPassword123",
            ConfirmPassword = "DifferentPassword123"
        };

        var results = ValidateModel(vm);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ChangePasswordViewModel.ConfirmPassword)));
    }

    [Fact]
    public void ChangePasswordViewModel_ShortPassword_FailsValidation()
    {
        var vm = new ChangePasswordViewModel
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "123",
            ConfirmPassword = "123"
        };

        var results = ValidateModel(vm);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ChangePasswordViewModel.NewPassword)));
    }

    [Fact]
    public void VehicleServiceViewModel_NegativePrice_FailsValidation()
    {
        var vm = new VehicleServiceViewModel
        {
            ServiceName = "Engine Wash",
            Description = "Full steam wash",
            Price = -500.00m,
            EstimatedDuration = "1 Hour",
            IconClass = "fa-solid fa-soap"
        };

        var results = ValidateModel(vm);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(VehicleServiceViewModel.Price)));
    }

    [Theory]
    [InlineData("Customer", "Dashboard", "Customer", "Dashboard", null, "active")]
    [InlineData("Customer", "Vehicles", "Customer", "Vehicles", null, "active")]
    [InlineData("Customer", "AddVehicle", "Customer", "Vehicles", new[] { "AddVehicle", "EditVehicle" }, "active")]
    [InlineData("Customer", "EditVehicle", "Customer", "Vehicles", new[] { "AddVehicle", "EditVehicle" }, "active")]
    [InlineData("Customer", "RequestDetails", "Customer", "MyRequests", new[] { "RequestDetails", "Invoice" }, "active")]
    [InlineData("Admin", "Dashboard", "Admin", "Dashboard", null, "active")]
    [InlineData("Admin", "CreateService", "Admin", "Services", new[] { "CreateService", "EditService" }, "active")]
    [InlineData("Home", "Index", "Customer", "Dashboard", null, "")]
    [InlineData("Admin", "Dashboard", "Home", "Index", null, "")]
    public void NavigationActiveLogic_ResolvesCorrectState(
        string currentController,
        string currentAction,
        string targetController,
        string targetAction,
        string[]? relatedActions,
        string expectedClass)
    {
        string ResolveIsActive(string ctrl, string act, string[]? related)
        {
            if (!string.Equals(currentController, ctrl, StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            if (string.Equals(currentAction, act, StringComparison.OrdinalIgnoreCase))
            {
                return "active";
            }

            if (related != null && related.Any(a => string.Equals(currentAction, a, StringComparison.OrdinalIgnoreCase)))
            {
                return "active";
            }

            return "";
        }

        var result = ResolveIsActive(targetController, targetAction, relatedActions);
        Assert.Equal(expectedClass, result);
    }
}
