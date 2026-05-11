using ECL.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECL.Controllers;

public class GrammarGuideController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Grammar Guide";
        var model = GrammarGuideCatalog.CreatePageModel();
        return View(model);
    }
}
