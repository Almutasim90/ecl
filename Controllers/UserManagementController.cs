using ECL.Data;
using ECL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECL.Controllers;

[Authorize(Roles = "Admin")]
public sealed class UserManagementController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly PasswordHasher<StudentUser> _hasher = new();

    public UserManagementController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var rows = await _db.StudentUsers.AsNoTracking()
            .OrderByDescending(u => u.CreatedAtUtc)
            .Select(u => new AdminStudentUserRowViewModel
            {
                Id = u.Id,
                Username = u.Username,
                CreatedAtUtc = u.CreatedAtUtc,
                QuizAttemptsCount = _db.QuizAttempts.Count(a => a.StudentUserId == u.Id),
            })
            .ToListAsync();

        return View(rows);
    }

    [HttpGet]
    public IActionResult Create() => View(new AdminCreateStudentViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminCreateStudentViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var username = vm.Username.Trim();
        if (await _db.StudentUsers.AnyAsync(u => u.Username == username))
        {
            ModelState.AddModelError(nameof(vm.Username), "Username is already taken.");
            return View(vm);
        }

        var user = new StudentUser
        {
            Username = username,
            CreatedAtUtc = DateTime.UtcNow,
        };
        user.PasswordHash = _hasher.HashPassword(user, vm.Password);

        _db.StudentUsers.Add(user);
        await _db.SaveChangesAsync();

        TempData["UserMgmtMessage"] = $"Student account “{user.Username}” was created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var user = await _db.StudentUsers.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return NotFound();

        return View(new AdminResetPasswordViewModel { Id = user.Id, Username = user.Username });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(AdminResetPasswordViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var user = await _db.StudentUsers.SingleOrDefaultAsync(u => u.Id == vm.Id);
        if (user is null)
            return NotFound();

        user.PasswordHash = _hasher.HashPassword(user, vm.NewPassword);
        await _db.SaveChangesAsync();

        TempData["UserMgmtMessage"] = $"Password reset for “{user.Username}”.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.StudentUsers.SingleOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return NotFound();

        var name = user.Username;
        _db.StudentUsers.Remove(user);
        await _db.SaveChangesAsync();

        TempData["UserMgmtMessage"] = $"Deleted student account “{name}” and related quiz history.";
        return RedirectToAction(nameof(Index));
    }
}
