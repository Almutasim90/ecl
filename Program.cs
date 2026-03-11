using ECL.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;


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
            Console.WriteLine("   Testing connection (direct Npgsql)...");
            const int maxAttempts = 3;
            var connected = false;
            Exception? lastError = null;
            for (int attempt = 1; attempt <= maxAttempts && !connected; attempt++)
            {
                try
                {
                    await using (var testConn = new NpgsqlConnection(connectionString))
                    {
                        await testConn.OpenAsync();
                    }
                    connected = true;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    lastError = ex;
                    Console.WriteLine($"   Attempt {attempt}/{maxAttempts} failed, retrying in 2s...");
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }
            }
            if (!connected)
            {
                if (lastError != null)
                {
                    Console.WriteLine($"   ❌ Last error: {lastError.Message}");
                    if (lastError.InnerException != null)
                        Console.WriteLine($"   Inner: {lastError.InnerException.Message}");
                }
                throw new InvalidOperationException("Database connection failed after " + maxAttempts + " attempts. Fix server pg_hba.conf and listen_addresses. See CONNECTION_TROUBLESHOOTING.md.");
            }
            Console.WriteLine("   ✅ Connection successful!");

            Console.WriteLine("   Testing EF Core CanConnectAsync...");
            var canConnect = await db.Database.CanConnectAsync();
            if (!canConnect)
            {
                Console.WriteLine("   ❌ EF Core connection test returned FALSE.");
                throw new InvalidOperationException("Database is not reachable with current connection string.");
            }
            
            Console.WriteLine("   Running migrations...");
            await db.Database.MigrateAsync();
            Console.WriteLine("   ✅ Migrations applied successfully.");

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    current_database()::text,
                    current_schema()::text,
                    (SELECT COUNT(*)::int FROM public.""ListeningQuestions""),
                    (SELECT COUNT(*)::int FROM public.""ReadingQuestions"")";

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var dbName = reader.GetString(0);
                var schema = reader.GetString(1);
                var listeningCount = reader.GetInt32(2);
                var readingCount = reader.GetInt32(3);

                Console.WriteLine($"   📌 Database: {dbName}");
                Console.WriteLine($"   📌 Schema: {schema}");
                Console.WriteLine($"   📊 Rows: Listening={listeningCount}, Reading={readingCount}");
            }
            
          
         
        }
        catch (Npgsql.NpgsqlException npgEx)
        {
            Console.WriteLine($"   ❌ PostgreSQL Error: {npgEx.Message}");
            Console.WriteLine($"   Error Code: {npgEx.SqlState}");
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