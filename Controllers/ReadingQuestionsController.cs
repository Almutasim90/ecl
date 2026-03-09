using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECL.Data;
using ECL.Models;

namespace ECL.Controllers
{
    public class ReadingQuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReadingQuestionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ReadingQuestions
        public IActionResult Index() => View();

        // GET: ReadingQuestions/LoadData
        // DataTables server-side endpoint. GET is preferred:
        // - no CSRF token needed
        // - responses are cache-friendly
        // - cleaner logging/debugging
        [HttpGet]
        public async Task<IActionResult> LoadData(
            int draw = 1,
            int start = 0,
            int length = 10,
            string? search = null,
            string? orderColumn = "Qno",
            string? orderDir = "asc")
        {
            var query = _context.ReadingQuestions.AsNoTracking().AsQueryable();

            var recordsTotal = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(x =>
                    (x.QuestionText != null && x.QuestionText.ToLower().Contains(term)) ||
                    (x.OptionA      != null && x.OptionA.ToLower().Contains(term))      ||
                    (x.OptionB      != null && x.OptionB.ToLower().Contains(term))      ||
                    (x.OptionC      != null && x.OptionC.ToLower().Contains(term))      ||
                    (x.OptionD      != null && x.OptionD.ToLower().Contains(term)));
            }

            var recordsFiltered = await query.CountAsync();

            bool desc = orderDir == "desc";
            query = orderColumn switch
            {
                "FormNumber"   => desc ? query.OrderByDescending(x => x.FormNumber)   : query.OrderBy(x => x.FormNumber),
                "QuestionText" => desc ? query.OrderByDescending(x => x.QuestionText) : query.OrderBy(x => x.QuestionText),
                "OptionA"      => desc ? query.OrderByDescending(x => x.OptionA)      : query.OrderBy(x => x.OptionA),
                _              => desc ? query.OrderByDescending(x => x.Qno)          : query.OrderBy(x => x.Qno)
            };

            int take = length == -1 ? recordsFiltered : length;
            var data = await query
                .Skip(start)
                .Take(take)
                .Select(x => new
                {
                    x.Qno,
                    x.FormNumber,
                    QuestionText = x.QuestionText ?? string.Empty,
                    OptionA      = x.OptionA      ?? string.Empty,
                    OptionB      = x.OptionB      ?? string.Empty,
                    OptionC      = x.OptionC      ?? string.Empty,
                    OptionD      = x.OptionD      ?? string.Empty
                })
                .ToListAsync();

            return Json(new { draw, recordsTotal, recordsFiltered, data });
        }

        // GET: ReadingQuestions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readingQuestion = await _context.ReadingQuestions
                .FirstOrDefaultAsync(m => m.Qno == id);
            if (readingQuestion == null)
            {
                return NotFound();
            }

            return View(readingQuestion);
        }

        // GET: ReadingQuestions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ReadingQuestions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Qno,FormNumber,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption")] ReadingQuestion readingQuestion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(readingQuestion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(readingQuestion);
        }

        // GET: ReadingQuestions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readingQuestion = await _context.ReadingQuestions.FindAsync(id);
            if (readingQuestion == null)
            {
                return NotFound();
            }
            return View(readingQuestion);
        }

        // POST: ReadingQuestions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Qno,FormNumber,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption")] ReadingQuestion readingQuestion)
        {
            if (id != readingQuestion.Qno)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(readingQuestion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReadingQuestionExists(readingQuestion.Qno))
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
            return View(readingQuestion);
        }

        // GET: ReadingQuestions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readingQuestion = await _context.ReadingQuestions
                .FirstOrDefaultAsync(m => m.Qno == id);
            if (readingQuestion == null)
            {
                return NotFound();
            }

            return View(readingQuestion);
        }

        // POST: ReadingQuestions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var readingQuestion = await _context.ReadingQuestions.FindAsync(id);
            if (readingQuestion != null)
            {
                _context.ReadingQuestions.Remove(readingQuestion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ReadingQuestions/StartQuiz
        public async Task<IActionResult> StartQuiz()
        {
            var forms = await _context.ReadingQuestions
                .Select(q => q.FormNumber)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();
            return View(forms);
        }

        // GET: ReadingQuestions/Quiz/5
        public async Task<IActionResult> Quiz(int formNumber)
        {
            var questions = await _context.ReadingQuestions
                .Where(q => q.FormNumber == formNumber)
                .OrderBy(q => q.Qno)
                .ToListAsync();
            ViewBag.FormNumber = formNumber;
            return View(questions);
        }

        private bool ReadingQuestionExists(int id)
        {
            return _context.ReadingQuestions.Any(e => e.Qno == id);
        }
    }
}
