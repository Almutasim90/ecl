using System.Threading.RateLimiting;
using ECL.Data;
using ECL.Filters;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(o => o.Filters.Add<DatabaseExceptionFilter>());
builder.Services.AddRazorPages();

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
            await db.Database.MigrateAsync();
        else
            logger.LogWarning("Database unreachable at startup — app will run; pages will show a connection message.");
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

app.UseRateLimiter();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
