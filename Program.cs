using System.Net;
using Aikido.Zen.Core;
using Aikido.Zen.DotNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using zen_demo_dotnet;
using zen_demo_dotnet.Data;
using zen_demo_dotnet.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add zen services
builder.Services.AddZenFirewall();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("Zen firewall reporting status", () => Zen.Status().Heartbeat switch
    {
        ReportingStatusResult.Ok => HealthCheckResult.Healthy("Zen firewall is reporting heartbeats successfully."),
        ReportingStatusResult.NotReported => HealthCheckResult.Degraded("Zen firewall has not reported heartbeats yet."),
        ReportingStatusResult.Expired => HealthCheckResult.Degraded("Zen firewall heartbeat reports have expired."),
        ReportingStatusResult.Failure => HealthCheckResult.Unhealthy("Zen firewall is not reporting heartbeats successfully."),
        _ => HealthCheckResult.Unhealthy("Unknown Zen firewall heartbeat status.")
    });

//add other services
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get database configuration
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ??
    builder.Configuration.GetValue<string>("DATABASE_URL") ??
    builder.Configuration.GetConnectionString("DefaultConnection") ??
throw new InvalidOperationException("DATABASE_URL environment variable is not set");

// Convert from URL format to connection string if needed
if (connectionString.StartsWith("postgres://"))
{
    // Parse the URL format
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
    var host = uri.Host;
    var port = uri.Port;
    var database = uri.AbsolutePath.TrimStart('/');
    var sslMode = uri.Query.Contains("sslmode=disable") ? "Disable" : "Prefer";

    // Build the connection string
    connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode={sslMode}";
}

// Add database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register helpers
builder.Services.AddScoped<DatabaseHelper>();
builder.Services.AddSingleton<AppHelpers>(); 

// Create app
var app = builder.Build();

// Allow XForwardedFor
var forwardedHeadersoptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    ForwardLimit = 5,
};

// only for demo purposes
forwardedHeadersoptions.KnownNetworks.Clear();
forwardedHeadersoptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersoptions);

// Fly.io specific middleware to set the client IP address
// https://fly.io/docs/networking/request-headers/
//app.Use(async (context, next) =>
//{
//    if (context.Request.Headers.ContainsKey("Fly-Client-IP") && IPAddress.TryParse(context.Request.Headers["Fly-Client-IP"].ToString(), out var clientIp))
//    {
//        context.Connection.RemoteIpAddress = clientIp;
//    }
//    await next(context);
//});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseAuthorization();

// Track user
app.Use((context, next) =>
{
    // First check for 'user' header (primary method)
    if (context.Request.Headers.TryGetValue("user", out var userValue) && !string.IsNullOrEmpty(userValue))
    {
        if (int.TryParse(userValue, out int userId))
        {
            // Use the GetName function to get a consistent name based on the user ID
            string userName = UserHelper.GetName(userId);
            Zen.SetUser(userId.ToString(), userName, context);
        }
    }
    // Fallback to X-User-ID and X-User-Name headers
    else if (context.Request.Headers.TryGetValue("X-User-ID", out var userIdValue) &&
             context.Request.Headers.TryGetValue("X-User-Name", out var userNameValue) &&
             !string.IsNullOrEmpty(userIdValue))
    {
        if (int.TryParse(userIdValue, out int userId))
        {
            Zen.SetUser(userId.ToString(), userNameValue, context);
        }
    }

    return next();
});

// Enable Zen
try
{
    app.UseZenFirewall();
}
catch (Exception e)
{
    Console.WriteLine("Aikido does not run on ARM chips", e);
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseTestingMiddleware(); // used to test unregistered routes
app.UsePublicFallback();
app.MapHealthChecks("/healthz");
app.MapRazorPages();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply database migrations at startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("Database migration completed successfully");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
}

app.Run();
