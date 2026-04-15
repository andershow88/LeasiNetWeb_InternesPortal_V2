using Hangfire;
using Hangfire.InMemory;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Application.Services;
using LeasiNetWeb.Web;
using IAdminService = LeasiNetWeb.Application.Interfaces.IAdminService;
using AdminService = LeasiNetWeb.Application.Services.AdminService;
using LeasiNetWeb.Infrastructure.Data;
using AnhangServiceImpl = LeasiNetWeb.Infrastructure.Data.AnhangService;
using LeasiNetWeb.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;

// ── Global unhandled-exception logger ────────────────────────────────────────
// Prints exception type + message even when ToString() would normally fail,
// giving Railway logs something to show before the process dies.
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    var ex = e.ExceptionObject;
    try
    {
        Console.Error.WriteLine($"[FATAL] {ex?.GetType().FullName}: {(ex as Exception)?.Message}");
        Console.Error.WriteLine((ex as Exception)?.StackTrace);
    }
    catch
    {
        Console.Error.WriteLine("[FATAL] Unhandled exception — ToString() failed (likely StackOverflow).");
    }
    Console.Error.Flush();
};

Checkpoint("1 – process start");

var builder = WebApplication.CreateBuilder(args);
Checkpoint("2 – builder created");

// ── PORT (Railway injects PORT env variable) ──────────────────────────────────
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
Checkpoint("3 – MVC registered");

// ── Datenbank ─────────────────────────────────────────────────────────────────
// Priority: DATABASE_URL (Railway PostgreSQL) → DefaultConnection → SQLite fallback
var databaseUrl      = Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(databaseUrl))
{
    Checkpoint("4 – using PostgreSQL (DATABASE_URL)");
    var npgsqlConn = ConvertPostgresUrlToNpgsql(databaseUrl);
    builder.Services.AddDbContext<ApplicationDbContext>(opt =>
        opt.UseNpgsql(npgsqlConn));
}
else if (!string.IsNullOrEmpty(connectionString) &&
         !connectionString.Contains(".db") &&
         !connectionString.StartsWith("Data Source="))
{
    Checkpoint("4 – using SQL Server");
    builder.Services.AddDbContext<ApplicationDbContext>(opt =>
        opt.UseSqlServer(connectionString));
}
else
{
    Checkpoint("4 – using SQLite (fallback)");
    var sqliteConn = connectionString ?? "Data Source=leasinetweb.db";
    builder.Services.AddDbContext<ApplicationDbContext>(opt =>
        opt.UseSqlite(sqliteConn));
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
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IInternePruefungService, InternePruefungService>();
builder.Services.AddScoped<BereinigungsJob>();
Checkpoint("5 – application services registered");

// ── Hangfire ──────────────────────────────────────────────────────────────────
builder.Services.AddHangfire(config => config.UseInMemoryStorage());
builder.Services.AddHangfireServer();
Checkpoint("6 – Hangfire registered");

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
Checkpoint("7 – auth registered");

var app = builder.Build();
Checkpoint("8 – app built");

// ── Datenbank initialisieren & seeden ────────────────────────────────────────
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    Checkpoint("9 – seeding database");
    await DataSeeder.SeedAsync(db);
    Checkpoint("10 – seed complete");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[ERROR] Seed failed: {ex.GetType().Name}: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    Console.Error.Flush();
    // Do not rethrow — let the app start even if seed fails on subsequent deploys
}

// ── Hangfire recurring jobs ───────────────────────────────────────────────────
// Register AFTER the host starts so JobStorage is fully initialized.
app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        Checkpoint("11 – registering Hangfire recurring jobs");
        var jobManager = app.Services.GetRequiredService<IRecurringJobManager>();
        jobManager.AddOrUpdate<BereinigungsJob>(
            "antraege-archivieren",
            job => job.AntraegeArchivieren(24),
            Cron.Monthly());
        jobManager.AddOrUpdate<BereinigungsJob>(
            "sync-anfragen-bereinigen",
            job => job.SynchronisierungsAnfragenBereinigen(),
            Cron.Daily(3, 0));
        Checkpoint("12 – recurring jobs registered");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[WARN] Recurring job registration failed: {ex.GetType().Name}: {ex.Message}");
        Console.Error.Flush();
        // Non-fatal: app works without recurring jobs
    }
});

// ── Middleware ────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Railway terminates TLS at the edge — don't redirect to HTTPS internally
}

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAdminAuthorization() }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

Checkpoint("13 – middleware configured, calling app.Run()");
app.Run();

// ── Helpers ───────────────────────────────────────────────────────────────────

static void Checkpoint(string label)
{
    Console.Error.WriteLine($"[STARTUP] {label}");
    Console.Error.Flush();
}

/// <summary>
/// Converts a Railway DATABASE_URL (postgres://user:pass@host:port/db)
/// to an Npgsql connection string.
/// </summary>
static string ConvertPostgresUrlToNpgsql(string databaseUrl)
{
    var uri      = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);   // limit to 2 parts — safe if password contains ':'
    return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
