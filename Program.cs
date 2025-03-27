using Aikido.Zen.DotNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using zen_demo_dotnet;
using zen_demo_dotnet.Data;
using zen_demo_dotnet.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

// Add zen services
builder.Services.AddZenFirewall();

// Create app
var app = builder.Build();

// Allow XForwardedFor
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
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
app.UsePublicFallback();
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
