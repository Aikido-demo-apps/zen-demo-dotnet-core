using System.Net;
using Aikido.Zen.DotNetCore;
using Microsoft.AspNetCore.HttpOverrides;
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
builder.Services.AddZenFireWall();

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

app.UseZenFireWall();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
