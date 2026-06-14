using Microsoft.EntityFrameworkCore;

namespace Eims.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Optimistic concurrency conflict.");
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Data was modified by another user. Reload and retry.");
        }
        catch (InvalidOperationException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled API error.");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Unexpected server error.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://httpstatuses.com/" + statusCode,
            title = ReasonPhrases.GetReasonPhrase(statusCode),
            status = statusCode,
            detail
        });
    }
}
