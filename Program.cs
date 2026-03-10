using ECL.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Allow Flutter (and any other cross-platform client) to call the API from any origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings__DefaultConnection"]
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

try
{
    connectionString = new NpgsqlConnectionStringBuilder(connectionString).ConnectionString;
}
catch (Exception ex)
{
    throw new InvalidOperationException(
        "Invalid PostgreSQL connection string in 'ConnectionStrings:DefaultConnection'. " +
        "Remove SQL Server keys like Trusted_Connection or MultipleActiveResultSets.", ex);
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.EnableRetryOnFailure()));



var app = builder.Build();

// Auto-apply EF Core migrations on startup (runs inside the Coolify container
// which shares the Docker network with supabase-db-*)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        Console.WriteLine("✅ Migration successful");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Migration failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// Enable HTTPS redirection only when not in Development so local LAN devices can use HTTP
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
