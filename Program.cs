using NeonGrid.Hubs;
using NeonGrid.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// [FIX] Enable Detailed Errors for production debugging
builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = true;
});

// [FIX] Add permissive CORS for Render deployment
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(_ => true)
              .AllowCredentials());
});

builder.Services.AddSingleton<GameManager>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// [FIX] Use the CORS policy
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<GameHub>("/gameHub");

app.Run();