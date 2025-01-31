using Aikido.Zen.DotNetCore;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddZenFireWall();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
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
