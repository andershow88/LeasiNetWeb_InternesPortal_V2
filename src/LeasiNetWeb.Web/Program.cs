using Hangfire;
using Hangfire.InMemory;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Web;
using LeasiNetWeb.Application.Services;
using LeasiNetWeb.Infrastructure.Data;
using AnhangServiceImpl = LeasiNetWeb.Infrastructure.Data.AnhangService;
using LeasiNetWeb.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Datenbank ─────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=leasinetweb.db";

if (connectionString.Contains(".db") || connectionString.StartsWith("Data Source="))
{
    builder.Services.AddDbContext<ApplicationDbContext>(opt =>
        opt.UseSqlite(connectionString));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(opt =>
        opt.UseSqlServer(connectionString));
}

builder.Services.AddScoped<IApplicationDbContext>(sp =>
    sp.GetRequiredService<ApplicationDbContext>());

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEreignisService, EreignisService>();
builder.Services.AddScoped<INachrichtService, NachrichtService>();
builder.Services.AddScoped<IKommentarService, KommentarService>();
builder.Services.AddScoped<IAnhangService, AnhangServiceImpl>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ILeasingantragService, LeasingantragService>();
builder.Services.AddScoped<BereinigungsJob>();

// ── Hangfire (background jobs — replaces Intrexx timer workflows) ──────────────
// Production: switch to .UseSqlServerStorage(connectionString)
builder.Services.AddHangfire(config => config.UseInMemoryStorage());
builder.Services.AddHangfireServer();

// ── Cookie-Authentifizierung ──────────────────────────────────────────────────
builder.Services.AddAuthentication("LeasiNetWeb.Auth")
    .AddCookie("LeasiNetWeb.Auth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/ZugriffVerweigert";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "LeasiNetWeb.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Genehmiger", p => p.RequireClaim("Rolle", "Genehmiger", "Administrator"));
    options.AddPolicy("Administrator", p => p.RequireClaim("Rolle", "Administrator"));
    options.AddPolicy("InternerPruefer", p => p.RequireClaim("Rolle", "InternerPruefer", "Administrator"));
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ── Datenbank initialisieren & seeden ────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DataSeeder.SeedAsync(db);
}

// ── Hangfire recurring jobs ───────────────────────────────────────────────────
RecurringJob.AddOrUpdate<BereinigungsJob>(
    "antraege-archivieren",
    job => job.AntraegeArchivieren(24),
    Cron.Monthly());

RecurringJob.AddOrUpdate<BereinigungsJob>(
    "sync-anfragen-bereinigen",
    job => job.SynchronisierungsAnfragenBereinigen(),
    Cron.Daily(3, 0));

// ── Middleware ────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAdminAuthorization()]
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
