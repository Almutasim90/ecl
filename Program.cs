using System.Threading.RateLimiting;
using System.Security.Claims;
using ECL.Data;
using ECL.Filters;
using ECL.Models;
using ECL.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load optional local-only settings (ignored by git) for dev convenience.
// Priority: env vars > appsettings.*.local.json > appsettings.json
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllersWithViews(o => o.Filters.Add<DatabaseExceptionFilter>());
builder.Services.AddRazorPages();

// ── AuthN/AuthZ ────────────────────────────────────────────────────────────
builder.Services.Configure<AdminCredentialsOptions>(
    builder.Configuration.GetSection(AdminCredentialsOptions.SectionName));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.LogoutPath = "/Account/Logout";
        o.AccessDeniedPath = "/Account/AccessDenied";
        o.SlidingExpiration = true;

        // For APIs, don't redirect to HTML pages; return proper status codes.
        o.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

// Register Test auth scheme always (disabled by default) so policies can reference it safely.
builder.Services
    .AddAuthentication()
    .AddScheme<TestAuthOptions, TestAuthHandler>(TestAuthDefaults.SchemeName, o =>
    {
        o.Enabled =
            builder.Environment.IsEnvironment("Testing") ||
            builder.Configuration.GetValue("TestAuth:Enabled", false);
        o.HeaderName = builder.Configuration["TestAuth:HeaderName"] ?? "X-Test-Auth";
        o.HeaderValue = builder.Configuration["TestAuth:HeaderValue"] ?? "admin";
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Questions.Read", p => p
        .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme, TestAuthDefaults.SchemeName)
        .RequireAuthenticatedUser()
        .RequireRole("Admin"));

    o.AddPolicy("Questions.Write", p => p
        .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme, TestAuthDefaults.SchemeName)
        .RequireAuthenticatedUser()
        .RequireRole("Admin"));
});

// ── CORS ──────────────────────────────────────────────────────────────────
// Web origin comes from env (production) or falls back to localhost for dev.
var webOrigin = Environment.GetEnvironmentVariable("WEB_ORIGIN")?.Trim();
if (string.IsNullOrWhiteSpace(webOrigin)) webOrigin = "http://localhost:5000";
builder.Services.AddCors(options =>
    options.AddPolicy("AppPolicy", p => p
        .WithOrigins(webOrigin, "http://localhost:5000", "http://localhost:5173")
        .SetIsOriginAllowed(origin => origin.StartsWith("http://localhost:"))
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()));

// ── Database ──────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")?.Trim();

// Support common hosting convention: DATABASE_URL (postgres://user:pass@host:port/db)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")?.Trim();
if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    // If it's already an Npgsql-style connection string, accept it.
    if (databaseUrl.Contains("Host=", StringComparison.OrdinalIgnoreCase))
    {
        connectionString = databaseUrl;
    }
    else if (databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
             databaseUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? "");
        var pass = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? "");
        var db = uri.AbsolutePath.Trim('/'); // /dbname

        var port = uri.IsDefaultPort ? 5432 : uri.Port;
        connectionString = $"Host={uri.Host};Port={port};Database={db};Username={user};Password={pass}";
    }
}

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "Database connection is not configured. Set ConnectionStrings:DefaultConnection in appsettings.json.");


builder.Services.AddDbContext<ApplicationDbContext>(o => o
    .UseNpgsql(connectionString)
    .ConfigureWarnings(w => w.Ignore(
        Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// ── Rate Limiting ─────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(o =>
{
    o.AddSlidingWindowLimiter("api", opts =>
    {
        opts.PermitLimit = 60;
        opts.Window = TimeSpan.FromMinutes(1);
        opts.SegmentsPerWindow = 6;
        opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opts.QueueLimit = 5;
    });
    o.RejectionStatusCode = 429;
});

var app = builder.Build();

// Try to connect and run migrations. If DB is unreachable, log and continue — app always starts so the site can display a "no connection" message.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        if (await db.Database.CanConnectAsync())
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            logger.LogWarning("Database unreachable at startup — app will run; pages will show a connection message.");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database startup failed — app will run; pages will show a connection message when data is needed.");
    }
}

// ── HTTP pipeline ─────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    // Dev only — HTTPS handled by reverse proxy (Coolify/Caddy) in production
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();
app.UseCors("AppPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
