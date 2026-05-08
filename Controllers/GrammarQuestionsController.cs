using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECL.Data;
using ECL.Models;

namespace ECL.Controllers
{
    [Authorize(Policy = "Questions.Read")]
    public class GrammarQuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GrammarQuestionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GrammarQuestions
        public IActionResult Index() => View();

        // GET: GrammarQuestions/LoadData
        [HttpGet]
        public async Task<IActionResult> LoadData(
            int draw = 1,
            int start = 0,
            int length = 10,
            string? search = null,
            string? typeFilter = null,
            string? levelFilter = null,
            string? orderColumn = "Qno",
            string? orderDir = "asc")
        {
            var query = _context.GrammarQuestions.AsNoTracking().AsQueryable();

            var recordsTotal = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(typeFilter))
            {
                query = query.Where(x => x.GrammarType != null && x.GrammarType == typeFilter);
            }

            if (!string.IsNullOrWhiteSpace(levelFilter))
            {
                query = query.Where(x => x.Level != null && x.Level == levelFilter);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(x =>
                    (x.GrammarType != null && x.GrammarType.ToLower().Contains(term)) ||
                    (x.Level != null && x.Level.ToLower().Contains(term)) ||
                    (x.QuestionText != null && x.QuestionText.ToLower().Contains(term)) ||
                    (x.OptionA != null && x.OptionA.ToLower().Contains(term)) ||
                    (x.OptionB != null && x.OptionB.ToLower().Contains(term)) ||
                    (x.OptionC != null && x.OptionC.ToLower().Contains(term)) ||
                    (x.OptionD != null && x.OptionD.ToLower().Contains(term)) ||
                    (x.Explanation != null && x.Explanation.ToLower().Contains(term)));
            }

            var recordsFiltered = await query.CountAsync();

            bool desc = orderDir == "desc";
            query = orderColumn switch
            {
                "GrammarType"  => desc ? query.OrderByDescending(x => x.GrammarType)  : query.OrderBy(x => x.GrammarType),
                "Level"        => desc ? query.OrderByDescending(x => x.Level)        : query.OrderBy(x => x.Level),
                "QuestionText" => desc ? query.OrderByDescending(x => x.QuestionText) : query.OrderBy(x => x.QuestionText),
                _              => desc ? query.OrderByDescending(x => x.Qno)          : query.OrderBy(x => x.Qno)
            };

            int take = length == -1 ? recordsFiltered : length;
            var data = await query
                .Skip(start)
                .Take(take)
                .Select(x => new
                {
                    x.Qno,
                    GrammarType = x.GrammarType ?? string.Empty,
                    Level = x.Level ?? string.Empty,
                    QuestionText = x.QuestionText ?? string.Empty,
                    OptionA = x.OptionA ?? string.Empty,
                    OptionB = x.OptionB ?? string.Empty,
                    OptionC = x.OptionC ?? string.Empty,
                    OptionD = x.OptionD ?? string.Empty,
                    Explanation = x.Explanation ?? string.Empty
                })
                .ToListAsync();

            return Json(new { draw, recordsTotal, recordsFiltered, data });
        }

        // GET: GrammarQuestions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var grammarQuestion = await _context.GrammarQuestions
                .FirstOrDefaultAsync(m => m.Qno == id);
            if (grammarQuestion == null) return NotFound();

            return View(grammarQuestion);
        }

        // GET: GrammarQuestions/Create
        [Authorize(Policy = "Questions.Write")]
        public IActionResult Create() => View();

        // POST: GrammarQuestions/Create
        [HttpPost]
        [Authorize(Policy = "Questions.Write")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Qno,GrammarType,Level,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption,Explanation")]
            GrammarQuestion grammarQuestion)
        {
            NormalizeLevel(grammarQuestion);

            if (ModelState.IsValid)
            {
                _context.Add(grammarQuestion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(grammarQuestion);
        }

        // GET: GrammarQuestions/Edit/5
        [Authorize(Policy = "Questions.Write")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var grammarQuestion = await _context.GrammarQuestions.FindAsync(id);
            if (grammarQuestion == null) return NotFound();

            return View(grammarQuestion);
        }

        // POST: GrammarQuestions/Edit/5
        [HttpPost]
        [Authorize(Policy = "Questions.Write")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Qno,GrammarType,Level,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption,Explanation")]
            GrammarQuestion grammarQuestion)
        {
            if (id != grammarQuestion.Qno) return NotFound();

            NormalizeLevel(grammarQuestion);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grammarQuestion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GrammarQuestionExists(grammarQuestion.Qno))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(grammarQuestion);
        }

        // GET: GrammarQuestions/Delete/5
        [Authorize(Policy = "Questions.Write")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var grammarQuestion = await _context.GrammarQuestions
                .FirstOrDefaultAsync(m => m.Qno == id);
            if (grammarQuestion == null) return NotFound();

            return View(grammarQuestion);
        }

        // POST: GrammarQuestions/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Policy = "Questions.Write")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grammarQuestion = await _context.GrammarQuestions.FindAsync(id);
            if (grammarQuestion != null)
            {
                _context.GrammarQuestions.Remove(grammarQuestion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: GrammarQuestions/StartQuiz
        [AllowAnonymous]
        public async Task<IActionResult> StartQuiz()
        {
            var vm = new GrammarStartQuizViewModel();

            try
            {
                vm.Topics = await _context.GrammarQuestions
                    .Select(q => q.GrammarType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();

                // Ordered list of levels actually present in the data, mapped to canonical order.
                var presentLevels = await _context.GrammarQuestions
                    .Where(q => q.Level != null && q.Level != "")
                    .Select(q => q.Level!)
                    .Distinct()
                    .ToListAsync();

                vm.Levels = GrammarQuestion.AllowedLevels
                    .Where(l => presentLevels.Contains(l, StringComparer.OrdinalIgnoreCase))
                    .ToList();
            }
            catch (Exception ex)
            {
                TempData["DbError"] = $"Could not load grammar topics: {ex.Message}";
            }

            return View(vm);
        }

        // GET: GrammarQuestions/Quiz?grammarType=Tenses&level=Beginner
        [AllowAnonymous]
        public async Task<IActionResult> Quiz(string grammarType, string? level = null)
        {
            if (string.IsNullOrWhiteSpace(grammarType))
                return RedirectToAction(nameof(StartQuiz));

            var normalizedLevel = NormalizeLevelString(level);

            try
            {
                var query = _context.GrammarQuestions
                    .Where(q => q.GrammarType == grammarType);

                if (!string.IsNullOrWhiteSpace(normalizedLevel))
                    query = query.Where(q => q.Level == normalizedLevel);

                var questions = await query
                    .OrderBy(q => q.Qno)
                    .ToListAsync();

                if (!questions.Any())
                {
                    TempData["DbError"] = string.IsNullOrWhiteSpace(normalizedLevel)
                        ? $"No questions found for {grammarType}. The database may be empty or unreachable."
                        : $"No {normalizedLevel} questions found for {grammarType}.";
                    return RedirectToAction(nameof(StartQuiz));
                }

                ViewBag.GrammarType = grammarType;
                ViewBag.Level       = normalizedLevel;
                return View(questions);
            }
            catch (Exception ex)
            {
                TempData["DbError"] = $"Could not load quiz for {grammarType}: {ex.Message}";
                return RedirectToAction(nameof(StartQuiz));
            }
        }

        // GET: GrammarQuestions/RandomQuiz?count=20&grammarType=Tenses&level=Advanced
        [AllowAnonymous]
        public async Task<IActionResult> RandomQuiz(int count = 20, string? grammarType = null, string? level = null)
        {
            count = Math.Clamp(count, 5, 50);
            var normalizedLevel = NormalizeLevelString(level);

            try
            {
                var query = _context.GrammarQuestions.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(grammarType))
                    query = query.Where(q => q.GrammarType == grammarType);

                if (!string.IsNullOrWhiteSpace(normalizedLevel))
                    query = query.Where(q => q.Level == normalizedLevel);

                var questions = await query
                    .OrderBy(_ => EF.Functions.Random())
                    .Take(count)
                    .ToListAsync();

                if (!questions.Any())
                {
                    TempData["DbError"] = BuildEmptyQuizMessage(grammarType, normalizedLevel);
                    return RedirectToAction(nameof(StartQuiz));
                }

                ViewBag.GrammarType = BuildRandomTitle(grammarType, normalizedLevel, questions.Count);
                ViewBag.Level       = normalizedLevel;
                return View("Quiz", questions);
            }
            catch (Exception ex)
            {
                TempData["DbError"] = $"Could not load random grammar quiz: {ex.Message}";
                return RedirectToAction(nameof(StartQuiz));
            }
        }

        private bool GrammarQuestionExists(int id)
        {
            return _context.GrammarQuestions.Any(e => e.Qno == id);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void NormalizeLevel(GrammarQuestion q)
        {
            q.Level = NormalizeLevelString(q.Level);
        }

        /// <summary>
        /// Returns the canonical capitalization (Beginner / Intermediate / Advanced)
        /// for the supplied level, or null when the value is empty / unrecognized.
        /// </summary>
        private static string? NormalizeLevelString(string? level)
        {
            if (string.IsNullOrWhiteSpace(level)) return null;
            var match = GrammarQuestion.AllowedLevels
                .FirstOrDefault(l => string.Equals(l, level.Trim(), StringComparison.OrdinalIgnoreCase));
            return match;
        }

        private static string BuildEmptyQuizMessage(string? grammarType, string? level)
        {
            var topicPart = string.IsNullOrWhiteSpace(grammarType) ? "grammar" : grammarType!;
            var levelPart = string.IsNullOrWhiteSpace(level) ? string.Empty : $" {level}";
            return $"No{levelPart} questions found for {topicPart}. The database may be empty or unreachable.";
        }

        private static string BuildRandomTitle(string? grammarType, string? level, int count)
        {
            var prefix = "Random Mix";
            if (!string.IsNullOrWhiteSpace(grammarType)) prefix = $"Random · {grammarType}";
            if (!string.IsNullOrWhiteSpace(level))       prefix = $"{prefix} ({level})";
            return $"{prefix} ({count})";
        }
    }
}
