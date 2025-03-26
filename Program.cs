using System.Net;
using Aikido.Zen.DotNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using zen_demo_dotnet;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Clear all known networks
    options.KnownProxies.Clear();
    options.KnownNetworks.Clear();

    // Trust all proxies (not recommended for production ;))
    options.KnownNetworks.Add(new IPNetwork(IPAddress.Any, 0));
});
try
{
    builder.Services.AddZenFireWall();
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Unable to add Zen FireWall service: {ex.Message}");
    Console.WriteLine("Application will continue without Zen protection.");
}

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

try
{
    app.UseZenFireWall();
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Unable to initialize Zen FireWall: {ex.Message}");
    Console.WriteLine("Application will continue without Zen protection.");
}

app.UseAuthorization();

// Add the public fallback middleware before mapping routes
app.UsePublicFallback();

app.MapRazorPages();

// Create the public directory if it doesn't exist
var publicDir = Path.Combine(app.Environment.WebRootPath, "public");
if (!Directory.Exists(publicDir))
{
    Directory.CreateDirectory(publicDir);
    Console.WriteLine($"Created directory: {publicDir}");
}

app.Run();
