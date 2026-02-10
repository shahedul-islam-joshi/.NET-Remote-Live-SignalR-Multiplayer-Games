using NeonGrid.Hubs;
using NeonGrid.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// CRITICAL: Register GameManager as Singleton so state persists across requests
builder.Services.AddSingleton<GameManager>();

var app = builder.Build();

// 2. Configure Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 3. Map Endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<GameHub>("/gameHub"); // The WebSocket endpoint

app.Run();