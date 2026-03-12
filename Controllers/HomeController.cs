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
        var listeningCount = await _context.ListeningQuestions.CountAsync();
        var readingCount = await _context.ReadingQuestions.CountAsync();
        var listeningForms = await _context.ListeningQuestions.Select(q => q.FormNumber).Distinct().CountAsync();
        var readingForms = await _context.ReadingQuestions.Select(q => q.FormNumber).Distinct().CountAsync();

        var model = new DashboardViewModel
        {
            ListeningQuestionsCount = listeningCount,
            ReadingQuestionsCount = readingCount,
            ListeningFormsCount = listeningForms,
            ReadingFormsCount = readingForms
        };
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
