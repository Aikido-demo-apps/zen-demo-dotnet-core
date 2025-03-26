using Microsoft.Extensions.FileProviders;

namespace zen_demo_dotnet;

public class PublicFallbackMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PublicFallbackMiddleware> _logger;

    public PublicFallbackMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment,
        ILogger<PublicFallbackMiddleware> logger)
    {
        _next = next;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Call the next middleware in the pipeline
        await _next(context);

        // If the response is 404 (Not Found), try to serve from public folder
        if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
        {
            var path = context.Request.Path.Value;
            if (!string.IsNullOrEmpty(path))
            {
                // Remove leading slash if present
                if (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }

                // Check if file exists in wwwroot/public
                var publicFilePath = Path.Combine(_environment.WebRootPath, "public", path);
                if (File.Exists(publicFilePath))
                {
                    // Reset the response
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = GetContentType(publicFilePath);

                    // Serve the file
                    await context.Response.SendFileAsync(publicFilePath);
                }
            }
        }
    }

    private string GetContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            _ => "application/octet-stream",
        };
    }
}

public static class PublicFallbackMiddlewareExtensions
{
    public static IApplicationBuilder UsePublicFallback(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PublicFallbackMiddleware>();
    }
}
