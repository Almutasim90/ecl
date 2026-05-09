using System.Linq;

namespace ECL.Configuration;

/// <summary>
/// Builds an Npgsql connection string from appsettings, ASP.NET env keys, and common Docker/cloud conventions.
/// </summary>
internal static class DatabaseConnectionResolver
{
    public static string? Resolve(IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("DefaultConnection")?.Trim();
        if (!string.IsNullOrWhiteSpace(cs))
            return cs;

        var databaseUrl = configuration["DATABASE_URL"]?.Trim()
            ?? Environment.GetEnvironmentVariable("DATABASE_URL")?.Trim();
        if (!string.IsNullOrWhiteSpace(databaseUrl))
            return ParseDatabaseUrl(databaseUrl);

        return BuildFromDiscretePostgresVars(configuration);
    }

    private static string? BuildFromDiscretePostgresVars(IConfiguration configuration)
    {
        static string? E(IConfiguration c, params string[] keys)
        {
            foreach (var k in keys)
            {
                var v = c[k]?.Trim();
                if (!string.IsNullOrWhiteSpace(v))
                    return v;

                var e = Environment.GetEnvironmentVariable(k)?.Trim();
                if (!string.IsNullOrWhiteSpace(e))
                    return e;
            }
            return null;
        }

        var host = E(configuration, "PGHOST", "POSTGRES_HOST");
        var database = E(configuration, "PGDATABASE", "POSTGRES_DB", "POSTGRES_DATABASE");
        var user = E(configuration, "PGUSER", "POSTGRES_USER");
        var password = E(configuration, "PGPASSWORD", "POSTGRES_PASSWORD") ?? "";
        var port = E(configuration, "PGPORT", "POSTGRES_PORT") ?? "5432";

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(database) ||
            string.IsNullOrWhiteSpace(user))
            return null;

        var sslMode = E(configuration, "PGSSLMODE", "POSTGRES_SSLMODE");
        var conn =
            $"Host={host};Port={port};Database={database};Username={user};Password={password}";
        if (sslMode?.Equals("require", StringComparison.OrdinalIgnoreCase) == true ||
            sslMode?.Equals("verify-full", StringComparison.OrdinalIgnoreCase) == true ||
            sslMode?.Equals("verify-ca", StringComparison.OrdinalIgnoreCase) == true)
        {
            conn += ";SSL Mode=Require;Trust Server Certificate=true";
        }

        return conn;
    }

    private static string? ParseDatabaseUrl(string databaseUrl)
    {
        if (databaseUrl.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            return databaseUrl;

        if (!databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !databaseUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return null;

        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? "");
        var pass = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? "");
        var db = uri.AbsolutePath.Trim('/');

        var port = uri.IsDefaultPort ? 5432 : uri.Port;
        var connectionString = $"Host={uri.Host};Port={port};Database={db};Username={user};Password={pass}";

        var query = uri.Query.TrimStart('?');
        if (query.Contains("sslmode=require", StringComparison.OrdinalIgnoreCase) ||
            query.Contains("sslmode=verify-full", StringComparison.OrdinalIgnoreCase) ||
            query.Contains("sslmode=verify-ca", StringComparison.OrdinalIgnoreCase))
        {
            connectionString += ";SSL Mode=Require;Trust Server Certificate=true";
        }

        return connectionString;
    }
}
