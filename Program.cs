using ECL.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ECL.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings__DefaultConnection"];

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("❌ FATAL: Connection string 'DefaultConnection' was not found.");
    Console.WriteLine("Available env vars:");
    foreach (var e in Environment.GetEnvironmentVariables().Keys)
        Console.WriteLine($"  {e}");
    throw new InvalidOperationException("Connection string not found - check Coolify environment variables.");
}

Console.WriteLine($"✅ Connection string found: Host={new NpgsqlConnectionStringBuilder(connectionString).Host}");

try
{
    connectionString = new NpgsqlConnectionStringBuilder(connectionString).ConnectionString;
}
catch (Exception ex)
{
    throw new InvalidOperationException("Invalid PostgreSQL connection string.", ex);
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.EnableRetryOnFailure()));

var app = builder.Build();

// Run diagnostics and migrations before middleware
var runDiagnostics = async () =>
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        Console.WriteLine("🔍 DATABASE DIAGNOSTICS:");
        Console.WriteLine($"   Connection: {connectionString.Replace(new NpgsqlConnectionStringBuilder(connectionString).Password ?? "", "***")}");
        
        try
        {
            Console.WriteLine("   Testing connection...");
            await db.Database.CanConnectAsync();
            Console.WriteLine("   ✅ Connection successful!");
            
            Console.WriteLine("   Running migrations...");
            await db.Database.MigrateAsync();
            Console.WriteLine("   ✅ Migrations applied successfully.");
            
          
         
        }
        catch (Npgsql.NpgsqlException npgEx)
        {
            Console.WriteLine($"   ❌ PostgreSQL Error: {npgEx.Message}");
            Console.WriteLine($"   Error Code: {npgEx.SqlState}");
            Console.WriteLine($"   Severity: {npgEx.Severity}");
            if (npgEx.InnerException != null)
                Console.WriteLine($"   Inner: {npgEx.InnerException.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ General Error: {ex.GetType().Name}");
            Console.WriteLine($"   Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            Console.WriteLine($"   Stack: {ex.StackTrace?.Split('\n').FirstOrDefault()}");
        }
    }
};

// Execute diagnostics
await runDiagnostics();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();