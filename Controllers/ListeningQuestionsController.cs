using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECL.Data;
using ECL.Models;

namespace ECL.Controllers
{
    [Authorize(Policy = "Questions.Read")]
    public class ListeningQuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ListeningQuestionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ListeningQuestions
        public IActionResult Index() => View();

        // GET: ListeningQuestions/LoadData
        // DataTables server-side endpoint with GET for simplicity and cache-friendliness
        [HttpGet]
        public async Task<IActionResult> LoadData(
            int draw = 1,
            int start = 0,
            int length = 10,
            string? search = null,
            string? orderColumn = "Qno",
            string? orderDir = "asc")
        {
            var query = _context.ListeningQuestions.AsNoTracking().AsQueryable();

            var recordsTotal = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(x =>
                    (x.AudioFile != null && x.AudioFile.ToLower().Contains(term)) ||
                    (x.OptionA   != null && x.OptionA.ToLower().Contains(term))   ||
                    (x.OptionB   != null && x.OptionB.ToLower().Contains(term))   ||
                    (x.OptionC   != null && x.OptionC.ToLower().Contains(term))   ||
                    (x.OptionD   != null && x.OptionD.ToLower().Contains(term)));
            }

            var recordsFiltered = await query.CountAsync();

            bool desc = orderDir == "desc";
            query = orderColumn switch
            {
                "FormNumber" => desc ? query.OrderByDescending(x => x.FormNumber) : query.OrderBy(x => x.FormNumber),
                "AudioFile"  => desc ? query.OrderByDescending(x => x.AudioFile)  : query.OrderBy(x => x.AudioFile),
                "OptionA"    => desc ? query.OrderByDescending(x => x.OptionA)    : query.OrderBy(x => x.OptionA),
                _            => desc ? query.OrderByDescending(x => x.Qno)        : query.OrderBy(x => x.Qno)
            };

            int take = length == -1 ? recordsFiltered : length;
            var data = await query
                .Skip(start)
                .Take(take)
                .Select(x => new
                {
                    x.Qno,
                    x.FormNumber,
                    AudioFile     = x.AudioFile     ?? string.Empty,
                    OptionA       = x.OptionA       ?? string.Empty,
                    OptionB       = x.OptionB       ?? string.Empty,
                    OptionC       = x.OptionC       ?? string.Empty,
                    OptionD       = x.OptionD       ?? string.Empty,
                    x.CorrectOption
                })
                .ToListAsync();

            return Json(new { draw, recordsTotal, recordsFiltered, data });
        }
      

        // GET: ListeningQuestions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listeningQuestion = await _context.ListeningQuestions
                .FirstOrDefaultAsync(m => m.Qno == id);
            if (listeningQuestion == null)
            {
                return NotFound();
            }

            return View(listeningQuestion);
        }

        // GET: ListeningQuestions/Create
        [Authorize(Policy = "Questions.Write")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ListeningQuestions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Policy = "Questions.Write")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Qno,FormNumber,AudioFile,OptionA,OptionB,OptionC,OptionD,CorrectOption")] ListeningQuestion listeningQuestion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(listeningQuestion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(listeningQuestion);
        }

        // GET: ListeningQuestions/Edit/5
        [Authorize(Policy = "Questions.Write")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listeningQuestion = await _context.ListeningQuestions.FindAsync(id);
            if (listeningQuestion == null)
            {
                return NotFound();
            }
            return View(listeningQuestion);
        }

        // POST: ListeningQuestions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Policy = "Questions.Write")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Qno,FormNumber,AudioFile,OptionA,OptionB,OptionC,OptionD,CorrectOption")] ListeningQuestion listeningQuestion)
        {
            if (id != listeningQuestion.Qno)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listeningQuestion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListeningQuestionExists(listeningQuestion.Qno))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(listeningQuestion);
        }

        // GET: ListeningQuestions/Delete/5
        [Authorize(Policy = "Questions.Write")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listeningQuestion = await _context.ListeningQuestions
                .FirstOrDefaultAsync(m => m.Qno == id);
            if (listeningQuestion == null)
            {
                return NotFound();
            }

            return View(listeningQuestion);
        }

        // POST: ListeningQuestions/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Policy = "Questions.Write")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var listeningQuestion = await _context.ListeningQuestions.FindAsync(id);
            if (listeningQuestion != null)
            {
                _context.ListeningQuestions.Remove(listeningQuestion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ListeningQuestions/StartQuiz
        [AllowAnonymous]
        public async Task<IActionResult> StartQuiz()
        {
            try
            {
                var forms = await _context.ListeningQuestions
                    .Select(q => q.FormNumber)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToListAsync();
                return View(forms);
            }
            catch (Exception ex)
            {
                TempData["DbError"] = $"Could not load quiz forms: {ex.Message}";
                return View(new List<int>());
            }
        }

        // GET: ListeningQuestions/Quiz/5
        [AllowAnonymous]
        public async Task<IActionResult> Quiz(int formNumber)
        {
            try
            {
                var questions = await _context.ListeningQuestions
                    .Where(q => q.FormNumber == formNumber)
                    .OrderBy(q => q.Qno)
                    .ToListAsync();

                if (!questions.Any())
                {
                    TempData["DbError"] = $"No questions found for Form {formNumber}. The database may be empty or unreachable.";
                    return RedirectToAction(nameof(StartQuiz));
                }

                ViewBag.FormNumber = formNumber;
                return View(questions);
            }
            catch (Exception ex)
            {
                TempData["DbError"] = $"Could not load quiz for Form {formNumber}: {ex.Message}";
                return RedirectToAction(nameof(StartQuiz));
            }
        }

        private bool ListeningQuestionExists(int id)
        {
            return _context.ListeningQuestions.Any(e => e.Qno == id);
        }
    }
}
