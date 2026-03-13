using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECL.Data;
using ECL.Models;

namespace ECL.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel();
        try
        {
            model.ListeningQuestionsCount = await _context.ListeningQuestions.CountAsync();
            model.ReadingQuestionsCount = await _context.ReadingQuestions.CountAsync();
            model.ListeningFormsCount = await _context.ListeningQuestions.Select(q => q.FormNumber).Distinct().CountAsync();
            model.ReadingFormsCount = await _context.ReadingQuestions.Select(q => q.FormNumber).Distinct().CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Dashboard could not load stats from database; showing zeros.");
            ViewData["DashboardWarning"] = "Statistics temporarily unavailable. Check database connection.";
        }
        return View(model);
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
