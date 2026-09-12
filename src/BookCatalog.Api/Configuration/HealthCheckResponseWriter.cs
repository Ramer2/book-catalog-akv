using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BookCatalog.Api.Configuration;

public static class HealthCheckResponseWriter
{
    public static Task WriteResponseAsync(HttpContext context, HealthReport result)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = result.Status.ToString(),
            totalDuration = result.TotalDuration.ToString(),
            entries = result.Entries.Select(pair => new
            {
                name = pair.Key,
                status = pair.Value.Status.ToString(),
                description = pair.Value.Description,
                duration = pair.Value.Duration.ToString(),
                data = pair.Value.Data
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}
