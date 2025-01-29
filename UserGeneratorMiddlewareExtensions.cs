public static class UserGeneratorMiddlewareExtensions
{
    public static IApplicationBuilder UseUserGenerator(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserGeneratorMiddleware>();
    }
}
