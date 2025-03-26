using System.Net;
using Aikido.Zen.Core.Exceptions;
using Aikido.Zen.DotNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using zen_demo_dotnet;
using zen_demo_dotnet.Data;
using zen_demo_dotnet.Helpers;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddZenFirewall();
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register helpers
builder.Services.AddScoped<DatabaseHelper>();
builder.Services.AddSingleton<AppHelpers>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Clear all known networks
    options.KnownProxies.Clear();
    options.KnownNetworks.Clear();

    // Trust all proxies (not recommended for production ;))
    options.KnownNetworks.Add(new IPNetwork(IPAddress.Any, 0));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
// Configure static files to serve from root
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseUserGenerator();

app.Use((context, next) =>
{
    var id = context.Items["UserId"] as string ?? null;
    var name = context.Items["UserName"] as string ?? null;
    if (id != null)
        Zen.SetUser(id, name, context);
    return next();
});

app.UseAuthorization();

// Add the public fallback middleware before mapping routes
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

// Create the public directory if it doesn't exist
var publicDir = Path.Combine(app.Environment.WebRootPath, "public");
if (!Directory.Exists(publicDir))
{
    Directory.CreateDirectory(publicDir);
    Console.WriteLine($"Created directory: {publicDir}");
}

try
{
    app.UseZenFirewall();
}
catch(Exception e)
{
    Console.WriteLine("Aikido does not run on ARM chips", e);
}


app.Run();
