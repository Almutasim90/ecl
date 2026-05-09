using System.Security.Claims;
using ECL.Data;
using ECL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECL.Controllers;

[ApiController]
[Route("api/attempts")]
[Authorize(Roles = "Student")]
public sealed class AttemptsApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AttemptsApiController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<SaveAttemptResponse>> SaveAttempt([FromBody] SaveAttemptRequest req)
    {
        if (req is null)
            return BadRequest();

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        if (req.TotalQuestions <= 0 || req.CorrectCount < 0 || req.CorrectCount > req.TotalQuestions)
            return BadRequest("Invalid score payload.");

        var attempt = new QuizAttempt
        {
            StudentUserId = userId,
            Mode = req.Mode,
            FormNumber = req.FormNumber,
            GrammarType = req.GrammarType,
            Level = req.Level,
            TotalQuestions = req.TotalQuestions,
            CorrectCount = req.CorrectCount,
            StartedAtUtc = req.StartedAtUtc ?? DateTime.UtcNow,
            FinishedAtUtc = req.FinishedAtUtc ?? DateTime.UtcNow,
        };

        if (req.Answers is { Count: > 0 })
        {
            foreach (var a in req.Answers)
            {
                attempt.Answers.Add(new QuizAttemptAnswer
                {
                    Mode = req.Mode,
                    QuestionQno = a.QuestionQno,
                    SelectedOption = a.SelectedOption,
                    CorrectOption = a.CorrectOption,
                    IsCorrect = a.SelectedOption.HasValue && a.SelectedOption.Value == a.CorrectOption,
                });
            }
        }

        _db.QuizAttempts.Add(attempt);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest("Could not save attempt.");
        }

        return Ok(new SaveAttemptResponse { AttemptId = attempt.Id });
    }
}

