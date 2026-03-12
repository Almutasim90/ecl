using System.Text;
using System.Threading.RateLimiting;
using ECL.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ── CORS ──────────────────────────────────────────────────────────────────
// Web origin comes from env (production) or falls back to localhost for dev.
var webOrigin = Environment.GetEnvironmentVariable("WEB_ORIGIN") ?? "http://localhost:5000";
builder.Services.AddCors(options =>
    options.AddPolicy("AppPolicy", p => p
        .WithOrigins(webOrigin, "http://localhost:5000", "http://localhost:5173")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()));

// ── Database ──────────────────────────────────────────────────────────────
// In production: DATABASE_URL env var (PostgreSQL Npgsql connection string).
// In development: fall back to SQLite for local work.
var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=App_Data/ecl.db";

var appDataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(appDataDirectory);

if (connectionString.StartsWith("Host=", StringComparison.OrdinalIgnoreCase)
    || connectionString.StartsWith("Server=", StringComparison.OrdinalIgnoreCase)
    || connectionString.Contains("postgresql", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<ApplicationDbContext>(o => o
        .UseNpgsql(connectionString)
        .ConfigureWarnings(w => w.Ignore(
            Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(o => o
        .UseSqlite(connectionString)
        .ConfigureWarnings(w => w.Ignore(
            Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
}

// ── JWT Auth (Supabase GoTrue) ────────────────────────────────────────────
var jwtSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET");
if (!string.IsNullOrWhiteSpace(jwtSecret))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

    builder.Services.AddAuthorization(options =>
        options.AddPolicy("AdminOnly", p => p.RequireClaim("role", "admin")));
}
else
{
    // Dev mode: no JWT required — allow everything through
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization(options =>
        options.AddPolicy("AdminOnly", p => p.RequireAssertion(_ => true)));
}

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // SQLite (local dev): run migrations to create tables automatically.
    // PostgreSQL (Supabase): tables already exist — skip migrations.
    if (db.Database.IsSqlite())
        db.Database.Migrate();
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
