using System.Diagnostics;
using AutoCare.Data;
using AutoCare.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var services = await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.ServiceId)
            .ToListAsync();

        return View(services);
    }

    public async Task<IActionResult> Services()
    {
        var services = await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.ServiceId)
            .ToListAsync();

        return View(services);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(string name, string email, string phone, string subject, string message)
    {
        TempData["SuccessMessage"] = "Thank you for reaching out to AutoCare! Our service advisory team will contact you shortly.";
        return RedirectToAction(nameof(Contact));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
