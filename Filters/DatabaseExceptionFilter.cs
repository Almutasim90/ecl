using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ECL.Filters;

/// <summary>
/// When a database-related exception is thrown, shows a friendly "Database connection unavailable" page instead of a generic error.
/// </summary>
public class DatabaseExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.ExceptionHandled) return;

        if (!IsDatabaseException(context.Exception))
            return;

        context.ExceptionHandled = true;
        context.HttpContext.Response.StatusCode = 503;
        context.Result = new ViewResult
        {
            ViewName = "DatabaseUnavailable",
            ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                context.ModelState)
            {
                ["Title"] = "Database Unavailable"
            }
        };
    }

    private static bool IsDatabaseException(Exception ex)
    {
        for (var e = ex; e != null; e = e.InnerException)
        {
            if (e is NpgsqlException or DbUpdateException)
                return true;
            if (e is System.Net.Sockets.SocketException && (e.Message?.Contains("temporarily unavailable", StringComparison.OrdinalIgnoreCase) == true || e.Message?.Contains("Connection refused", StringComparison.OrdinalIgnoreCase) == true))
                return true;
        }
        return false;
    }
}
