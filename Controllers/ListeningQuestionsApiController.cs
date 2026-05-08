using ECL.Data;
using ECL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECL.Controllers
{
    [ApiController]
    [Route("api/listening")]
    [Authorize(Policy = "Questions.Read")]
    public class ListeningQuestionsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ListeningQuestionsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------
        // GET api/listening?page=1&pageSize=20&search=&orderBy=qno&desc=false
        // ---------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int pageSize = 20,
            string? search = null,
            string orderBy = "qno",
            bool desc = false)
        {
            var query = _context.ListeningQuestions.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(q =>
                    (q.AudioFile != null && q.AudioFile.Contains(search)) ||
                    (q.OptionA != null && q.OptionA.Contains(search)) ||
                    (q.OptionB != null && q.OptionB.Contains(search)) ||
                    (q.OptionC != null && q.OptionC.Contains(search)) ||
                    (q.OptionD != null && q.OptionD.Contains(search)));
            }

            var total = await query.CountAsync();

            query = orderBy.ToLower() switch
            {
                "formnumber" => desc ? query.OrderByDescending(q => q.FormNumber) : query.OrderBy(q => q.FormNumber),
                "audiofile" => desc ? query.OrderByDescending(q => q.AudioFile) : query.OrderBy(q => q.AudioFile),
                _ => desc ? query.OrderByDescending(q => q.Qno) : query.OrderBy(q => q.Qno)
            };

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new
                {
                    q.Qno,
                    q.FormNumber,
                    q.AudioFile,
                    AudioPath = "/AUDIO/" + q.AudioFile,
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
        // GET api/listening/forms  – distinct form numbers
        // ---------------------------------------------------------------
        [HttpGet("forms")]
        [AllowAnonymous]
        public async Task<IActionResult> GetForms()
        {
            var forms = await _context.ListeningQuestions
                .AsNoTracking()
                .Select(q => q.FormNumber)
                .Distinct()
                .OrderBy(f => f)
                .ToListAsync();

            return Ok(forms);
        }

        // ---------------------------------------------------------------
        // GET api/listening/quiz/{formNumber}
        // ---------------------------------------------------------------
        [HttpGet("quiz/{formNumber:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuiz(int formNumber)
        {
            var questions = await _context.ListeningQuestions
                .AsNoTracking()
                .Where(q => q.FormNumber == formNumber)
                .OrderBy(q => q.Qno)
                .Select(q => new
                {
                    q.Qno,
                    q.FormNumber,
                    q.AudioFile,
                    AudioPath = "/AUDIO/" + q.AudioFile,
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
        // GET api/listening/{id}
        // ---------------------------------------------------------------
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var q = await _context.ListeningQuestions
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Qno == id);

            return q is null ? NotFound() : Ok(q);
        }

        // ---------------------------------------------------------------
        // POST api/listening
        // ---------------------------------------------------------------
        [HttpPost]
        [Authorize(Policy = "Questions.Write")]
        public async Task<IActionResult> Create([FromBody] ListeningQuestion question)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.ListeningQuestions.Add(question);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = question.Qno }, question);
        }

        // ---------------------------------------------------------------
        // PUT api/listening/{id}
        // ---------------------------------------------------------------
        [HttpPut("{id:int}")]
        [Authorize(Policy = "Questions.Write")]
        public async Task<IActionResult> Update(int id, [FromBody] ListeningQuestion question)
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
                if (!await _context.ListeningQuestions.AnyAsync(q => q.Qno == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // ---------------------------------------------------------------
        // DELETE api/listening/{id}
        // ---------------------------------------------------------------
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "Questions.Write")]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _context.ListeningQuestions.FindAsync(id);
            if (question is null)
                return NotFound();

            _context.ListeningQuestions.Remove(question);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
