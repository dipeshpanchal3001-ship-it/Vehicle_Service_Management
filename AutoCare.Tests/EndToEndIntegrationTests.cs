using System.Net;
using System.Text.RegularExpressions;
using Xunit;

namespace AutoCare.Tests;

public class EndToEndIntegrationTests
{
    private const string BaseUrl = "http://localhost:5287";

    private static (HttpClient client, CookieContainer cookies) CreateClient()
    {
        var cookies = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookies,
            AllowAutoRedirect = true
        };
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        return (client, cookies);
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        var match = Regex.Match(html, @"name=""__RequestVerificationToken""\s+type=""hidden""\s+value=""([^""]+)""");
        if (!match.Success)
        {
            match = Regex.Match(html, @"value=""([^""]+)""\s+name=""__RequestVerificationToken""");
        }
        return match.Groups[1].Value;
    }

    [Fact]
    public async Task Customer_CanLogin_ViewProfile_AndAdminAreaIsRestricted()
    {
        var (client, _) = CreateClient();

        // 1. Get Login Page & Token
        var loginPageResp = await client.GetAsync("/Account/Login");
        Assert.Equal(HttpStatusCode.OK, loginPageResp.StatusCode);
        var loginHtml = await loginPageResp.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(loginHtml);

        // 2. Login as Customer
        var loginPostData = new Dictionary<string, string>
        {
            ["Email"] = "customer@autocare.com",
            ["Password"] = "Customer@123",
            ["__RequestVerificationToken"] = token
        };

        var loginPostResp = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(loginPostData));
        Assert.Equal(HttpStatusCode.OK, loginPostResp.StatusCode);
        var postLoginHtml = await loginPostResp.Content.ReadAsStringAsync();
        Assert.Contains("Welcome back, Rahul Sharma!", postLoginHtml);

        // 3. Visit Customer Profile
        var profileResp = await client.GetAsync("/Customer/Profile");
        Assert.Equal(HttpStatusCode.OK, profileResp.StatusCode);
        var profileHtml = await profileResp.Content.ReadAsStringAsync();
        Assert.Contains("Account & Profile Settings", profileHtml);
        Assert.Contains("customer@autocare.com", profileHtml);
        Assert.Contains("Rahul Sharma", profileHtml);
        // Verify My Profile is marked active in navbar
        Assert.Contains("nav-link nav-link-autocare active", profileHtml);
        Assert.Contains("My Profile", profileHtml);

        // 4. Update Profile
        var profileToken = ExtractAntiForgeryToken(profileHtml);
        var updateData = new Dictionary<string, string>
        {
            ["FullName"] = "Rahul Sharma Updated",
            ["Email"] = "customer@autocare.com",
            ["PhoneNumber"] = "9876543210",
            ["__RequestVerificationToken"] = profileToken
        };
        var updateResp = await client.PostAsync("/Customer/Profile", new FormUrlEncodedContent(updateData));
        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);
        var updateResultHtml = await updateResp.Content.ReadAsStringAsync();
        Assert.Contains("Your profile has been updated successfully!", updateResultHtml);
        Assert.Contains("Rahul Sharma Updated", updateResultHtml);

        // Revert name back to Rahul Sharma
        var revertToken = ExtractAntiForgeryToken(updateResultHtml);
        var revertData = new Dictionary<string, string>
        {
            ["FullName"] = "Rahul Sharma",
            ["Email"] = "customer@autocare.com",
            ["PhoneNumber"] = "9876543210",
            ["__RequestVerificationToken"] = revertToken
        };
        await client.PostAsync("/Customer/Profile", new FormUrlEncodedContent(revertData));

        // 5. Verify Customer cannot access Admin Dashboard (Access Denied)
        var adminAccessResp = await client.GetAsync("/Admin/Dashboard");
        var adminAccessHtml = await adminAccessResp.Content.ReadAsStringAsync();
        Assert.Contains("Access Restricted", adminAccessHtml);
    }

    [Fact]
    public async Task Admin_CanLogin_ViewAdminProfile_AndNavbarIsSeparated()
    {
        var (client, _) = CreateClient();

        // 1. Get Login Page & Token
        var loginPageResp = await client.GetAsync("/Account/Login");
        var loginHtml = await loginPageResp.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(loginHtml);

        // 2. Login as Admin
        var loginPostData = new Dictionary<string, string>
        {
            ["Email"] = "admin@autocare.com",
            ["Password"] = "Admin@12345",
            ["__RequestVerificationToken"] = token
        };

        var loginPostResp = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(loginPostData));
        var postLoginHtml = await loginPostResp.Content.ReadAsStringAsync();
        Assert.Contains("Administrative Dashboard", postLoginHtml);

        // Verify Admin navbar contains only admin items + View Website
        Assert.Contains("Manage Services", postLoginHtml);
        Assert.Contains("Service Requests", postLoginHtml);
        Assert.Contains("Customers", postLoginHtml);
        Assert.Contains("Admin Profile", postLoginHtml);
        Assert.Contains("View Website", postLoginHtml);

        // Verify Dashboard is active on Admin Dashboard page
        Assert.Matches(@"nav-link nav-link-autocare active[^""]*"" href=""/Admin/Dashboard""", postLoginHtml);

        // 3. Visit Admin Profile
        var adminProfileResp = await client.GetAsync("/Admin/Profile");
        Assert.Equal(HttpStatusCode.OK, adminProfileResp.StatusCode);
        var adminProfileHtml = await adminProfileResp.Content.ReadAsStringAsync();
        Assert.Contains("Administrator Profile", adminProfileHtml);
        Assert.Contains("admin@autocare.com", adminProfileHtml);
        Assert.Matches(@"nav-link nav-link-autocare active[^""]*"" href=""/Admin/Profile""", adminProfileHtml);
    }
}
