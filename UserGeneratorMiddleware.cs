namespace zen_demo_dotnet;

public class UserGeneratorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly UserGenerator _userGenerator;
    private readonly Random _random;

    public UserGeneratorMiddleware(RequestDelegate next)
    {
        _next = next;
        _userGenerator = new UserGenerator();
        _random = new Random();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ip = forwardedFor ?? context.Connection.RemoteIpAddress?.ToString();

        if (ip != null && _random.NextDouble() < 0.75)
        {
            var (id, name) = _userGenerator.GenerateUserFromIP(ip);
            context.Items["UserId"] = id;
            context.Items["UserName"] = name;
        }

        await _next(context);
    }
}