using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
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
            model.GrammarQuestionsCount = await _context.GrammarQuestions.CountAsync();

            model.ListeningFormsCount = await _context.ListeningQuestions.Select(q => q.FormNumber).Distinct().CountAsync();
            model.ReadingFormsCount = await _context.ReadingQuestions.Select(q => q.FormNumber).Distinct().CountAsync();
            model.GrammarTopicsCount = await _context.GrammarQuestions
                .Where(q => q.GrammarType != null && q.GrammarType != "")
                .Select(q => q.GrammarType)
                .Distinct()
                .CountAsync();

            model.GrammarLevelBeginnerCount =
                await _context.GrammarQuestions.CountAsync(q => q.Level == GrammarQuestion.LevelBeginner);
            model.GrammarLevelIntermediateCount =
                await _context.GrammarQuestions.CountAsync(q => q.Level == GrammarQuestion.LevelIntermediate);
            model.GrammarLevelAdvancedCount =
                await _context.GrammarQuestions.CountAsync(q => q.Level == GrammarQuestion.LevelAdvanced);

            ViewBag.IsAdmin = User?.IsInRole("Admin") == true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Dashboard could not load stats from database; showing zeros.");
            ViewData["DashboardWarning"] = "Statistics temporarily unavailable. Check database connection.";
            ViewBag.IsAdmin = User?.IsInRole("Admin") == true;
        }
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Dashboard()
    {
        var model = new DashboardPageViewModel();
        try
        {
            // Base totals
            model.ListeningQuestionsCount = await _context.ListeningQuestions.CountAsync();
            model.ReadingQuestionsCount = await _context.ReadingQuestions.CountAsync();
            model.GrammarQuestionsCount = await _context.GrammarQuestions.CountAsync();

            model.ListeningFormsCount = await _context.ListeningQuestions.Select(q => q.FormNumber).Distinct().CountAsync();
            model.ReadingFormsCount = await _context.ReadingQuestions.Select(q => q.FormNumber).Distinct().CountAsync();
            model.GrammarTopicsCount = await _context.GrammarQuestions
                .Where(q => q.GrammarType != null && q.GrammarType != "")
                .Select(q => q.GrammarType)
                .Distinct()
                .CountAsync();

            model.GrammarLevelBeginnerCount =
                await _context.GrammarQuestions.CountAsync(q => q.Level == GrammarQuestion.LevelBeginner);
            model.GrammarLevelIntermediateCount =
                await _context.GrammarQuestions.CountAsync(q => q.Level == GrammarQuestion.LevelIntermediate);
            model.GrammarLevelAdvancedCount =
                await _context.GrammarQuestions.CountAsync(q => q.Level == GrammarQuestion.LevelAdvanced);

            // Charts data
            var listeningByForm = await _context.ListeningQuestions
                .GroupBy(q => q.FormNumber)
                .Select(g => new { Form = g.Key, Count = g.Count() })
                .OrderBy(x => x.Form)
                .ToListAsync();
            model.ListeningFormNumbers = listeningByForm.Select(x => x.Form).ToList();
            model.ListeningFormCounts = listeningByForm.Select(x => x.Count).ToList();

            var readingByForm = await _context.ReadingQuestions
                .GroupBy(q => q.FormNumber)
                .Select(g => new { Form = g.Key, Count = g.Count() })
                .OrderBy(x => x.Form)
                .ToListAsync();
            model.ReadingFormNumbers = readingByForm.Select(x => x.Form).ToList();
            model.ReadingFormCounts = readingByForm.Select(x => x.Count).ToList();

            var levels = new[]
            {
                new { Label = GrammarQuestion.LevelBeginner,     Count = model.GrammarLevelBeginnerCount },
                new { Label = GrammarQuestion.LevelIntermediate, Count = model.GrammarLevelIntermediateCount },
                new { Label = GrammarQuestion.LevelAdvanced,     Count = model.GrammarLevelAdvancedCount },
            };
            model.GrammarLevelLabels = levels.Select(x => x.Label).ToList();
            model.GrammarLevelCounts = levels.Select(x => x.Count).ToList();

            var topTopics = await _context.GrammarQuestions
                .Where(q => q.GrammarType != null && q.GrammarType != "")
                .GroupBy(q => q.GrammarType!)
                .Select(g => new { Topic = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Topic)
                .Take(8)
                .ToListAsync();
            model.TopGrammarTopicLabels = topTopics.Select(x => x.Topic).ToList();
            model.TopGrammarTopicCounts = topTopics.Select(x => x.Count).ToList();

            ViewBag.IsAdmin = User?.IsInRole("Admin") == true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Dashboard could not load stats from database; showing zeros.");
            ViewData["DashboardWarning"] = "Statistics temporarily unavailable. Check database connection.";
            ViewBag.IsAdmin = User?.IsInRole("Admin") == true;
        }

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>Shown when the database connection is unavailable (e.g. after a DB exception or linked from dashboard warning).</summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult DatabaseUnavailable()
    {
        return View("DatabaseUnavailable");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
