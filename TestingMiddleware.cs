
namespace zen_demo_dotnet
{
    public class TestingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PublicFallbackMiddleware> _logger;

        public TestingMiddleware(
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
            if (context.Request.Path.StartsWithSegments("/test_unregistered_route"))
            {
                var aikidoContext = (context.Items["Aikido.Zen.Context"] as Aikido.Zen.Core.Context)?.Route;
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync($"Hello from unknown routing test! Route: {aikidoContext}");
                return;
            }
            await _next(context);
        }
    }

    public static class TestingMiddlewareExtensions
    {
        public static IApplicationBuilder UseTestingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TestingMiddleware>();
        }
    }

}
