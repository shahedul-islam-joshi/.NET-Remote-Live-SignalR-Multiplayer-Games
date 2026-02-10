using NeonGrid.Hubs;
using NeonGrid.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// [FIX] Add CORS Policy to allow the Render frontend to talk to the backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b
        .AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(origin => true) // Allow any domain (Render, localhost, etc.)
        .AllowCredentials());               // Required for SignalR
});

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

// [FIX] Enable CORS before Authorization and Endpoints
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<GameHub>("/gameHub");

app.Run();