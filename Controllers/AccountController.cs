using System.Security.Claims;
using ECL.Data;
using ECL.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECL.Controllers;

public sealed class AccountController : Controller
{
    private readonly AdminCredentialsOptions _admin;
    private readonly ApplicationDbContext _db;
    private readonly PasswordHasher<StudentUser> _hasher = new();

    public AccountController(IOptions<AdminCredentialsOptions> adminOptions, ApplicationDbContext db)
    {
        _admin = adminOptions.Value;
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var username = vm.Username?.Trim() ?? string.Empty;

        // 1) Try admin login if configured and matches exactly.
        if (!string.IsNullOrWhiteSpace(_admin.Username) && !string.IsNullOrWhiteSpace(_admin.Password))
        {
            bool adminOk =
                string.Equals(username, _admin.Username, StringComparison.Ordinal) &&
                string.Equals(vm.Password ?? string.Empty, _admin.Password, StringComparison.Ordinal);

            if (adminOk)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, "admin"),
                    new(ClaimTypes.Name, _admin.Username),
                    new(ClaimTypes.Role, "Admin"),
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }
        }

        // 2) Student login.
        var student = await _db.StudentUsers.SingleOrDefaultAsync(u => u.Username == username);
        if (student is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(vm);
        }

        var verify = _hasher.VerifyHashedPassword(student, student.PasswordHash, vm.Password ?? string.Empty);
        if (verify == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(vm);
        }

        var studentClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, student.Id.ToString()),
            new(ClaimTypes.Name, student.Username),
            new(ClaimTypes.Role, "Student"),
        };

        var studentIdentity = new ClaimsIdentity(studentClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        var studentPrincipal = new ClaimsPrincipal(studentIdentity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, studentPrincipal);

        if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
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

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, "Student"),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}

