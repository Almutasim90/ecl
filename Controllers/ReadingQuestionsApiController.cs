using ECL.Data;
using ECL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECL.Controllers
{
    [ApiController]
    [Route("api/reading")]
    [Authorize]
    public class ReadingQuestionsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReadingQuestionsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------
        // GET api/reading?page=1&pageSize=20&search=&orderBy=qno&desc=false
        // ---------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int pageSize = 20,
            string? search = null,
            string orderBy = "qno",
            bool desc = false)
        {
            var query = _context.ReadingQuestions.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(q =>
                    q.QuestionText.Contains(search) ||
                    q.OptionA.Contains(search) ||
                    q.OptionB.Contains(search) ||
                    q.OptionC.Contains(search) ||
                    q.OptionD.Contains(search));
            }

            var total = await query.CountAsync();

            query = orderBy.ToLower() switch
            {
                "formnumber" => desc ? query.OrderByDescending(q => q.FormNumber) : query.OrderBy(q => q.FormNumber),
                "questiontext" => desc ? query.OrderByDescending(q => q.QuestionText) : query.OrderBy(q => q.QuestionText),
                _ => desc ? query.OrderByDescending(q => q.Qno) : query.OrderBy(q => q.Qno)
            };

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new
                {
                    q.Qno,
                    q.FormNumber,
                    q.QuestionText,
                    q.OptionA,
                    q.OptionB,
                    q.OptionC,
                    q.OptionD,
                    q.CorrectOption
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize),
                data
            });
        }

        // ---------------------------------------------------------------
        // GET api/reading/forms  – distinct form numbers
        // ---------------------------------------------------------------
        [HttpGet("forms")]
        public async Task<IActionResult> GetForms()
        {
            var forms = await _context.ReadingQuestions
                .AsNoTracking()
                .Select(q => q.FormNumber)
                .Distinct()
                .OrderBy(f => f)
                .ToListAsync();

            return Ok(forms);
        }

        // ---------------------------------------------------------------
        // GET api/reading/quiz/{formNumber}
        // ---------------------------------------------------------------
        [HttpGet("quiz/{formNumber:int}")]
        public async Task<IActionResult> GetQuiz(int formNumber)
        {
            var questions = await _context.ReadingQuestions
                .AsNoTracking()
                .Where(q => q.FormNumber == formNumber)
                .OrderBy(q => q.Qno)
                .Select(q => new
                {
                    q.Qno,
                    q.FormNumber,
                    q.QuestionText,
                    q.OptionA,
                    q.OptionB,
                    q.OptionC,
                    q.OptionD,
                    q.CorrectOption
                })
                .ToListAsync();

            if (!questions.Any())
                return NotFound(new { message = $"No questions found for form {formNumber}" });

            return Ok(questions);
        }

        // ---------------------------------------------------------------
        // GET api/reading/{id}
        // ---------------------------------------------------------------
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var q = await _context.ReadingQuestions
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Qno == id);

            return q is null ? NotFound() : Ok(q);
        }

        // ---------------------------------------------------------------
        // POST api/reading
        // ---------------------------------------------------------------
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] ReadingQuestion question)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.ReadingQuestions.Add(question);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = question.Qno }, question);
        }

        // ---------------------------------------------------------------
        // PUT api/reading/{id}
        // ---------------------------------------------------------------
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] ReadingQuestion question)
        {
            if (id != question.Qno)
                return BadRequest(new { message = "ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(question).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.ReadingQuestions.AnyAsync(q => q.Qno == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // ---------------------------------------------------------------
        // DELETE api/reading/{id}
        // ---------------------------------------------------------------
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _context.ReadingQuestions.FindAsync(id);
            if (question is null)
                return NotFound();

            _context.ReadingQuestions.Remove(question);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
