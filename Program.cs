var builder = WebApplication.CreateBuilder(args);

// 1. REGISTER SERVICES
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// CRITICAL FIX: Must be Singleton so all players share the same game state
builder.Services.AddSingleton<NeonGrid.Services.GameManager>();

// CRITICAL FIX: Allow remote clients (CORS)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Allow any IP
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // SignalR requires this
    });
});

var app = builder.Build();

// 2. CONFIGURE PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// CRITICAL FIX: Use CORS before mapping hubs
app.UseCors();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NeonGrid.Hubs.GameHub>("/gameHub");

// 3. BIND TO 0.0.0.0 (Allows remote connections)
app.Run("http://0.0.0.0:5000");