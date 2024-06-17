using MCBA.Controllers;
using Microsoft.EntityFrameworkCore;
using MCBA.Data;
using MCBA.Debugbar;
using MCBA.Debugbar.Logging;

Debugbar.Boot();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    builder.Services.AddSingleton<ILoggerProvider, DebugbarLoggerProvider>();
});

builder.Services.AddDbContext<MCBAContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(MCBAContext)));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    
    // Enable lazy loading.
    options.UseLazyLoadingProxies();
});

// Setup Sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
});

builder.Services.AddHostedService<BillPayBackgroundService>();


builder.Services.AddAuthentication("CookieAuthentication")
                .AddCookie("CookieAuthentication", options =>
                {
                    options.LoginPath = "/Login";
                    options.AccessDeniedPath = "/AccessDenied"; 
                });

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

Debugbar.SetApp(app);

using(var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        SeedData.Initialize(services);
    }
    catch(Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();


app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PortalLogin}/{action=Login}/{id?}");

app.Run();