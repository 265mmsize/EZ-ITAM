using System.Globalization;
using EzItam.Web.Data;
using EzItam.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EZITAM_PROFILE=Sqlite 처럼 지정하면 appsettings.{Profile}.json 을 추가로 읽습니다 (로컬 개발/데모용).
var profile = Environment.GetEnvironmentVariable("EZITAM_PROFILE");
if (!string.IsNullOrWhiteSpace(profile))
    builder.Configuration.AddJsonFile($"appsettings.{profile}.json", optional: true, reloadOnChange: true);

// ---------- Database ----------
var provider = builder.Configuration["Database:Provider"] ?? "SqlServer";
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default 설정이 없습니다.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        options.UseSqlite(connectionString);
    else
        options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(3));
});

// ---------- Identity ----------
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanEdit", p => p.RequireRole(AppRoles.Admin, AppRoles.Manager));
    options.AddPolicy("AdminOnly", p => p.RequireRole(AppRoles.Admin));
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

// ---------- Application services ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpCurrentUserService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

var app = builder.Build();

// ---------- Culture (ko-KR) ----------
var culture = new CultureInfo("ko-KR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture }
});

// ---------- Database init ----------
if (app.Configuration.GetValue("Database:AutoMigrate", true))
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsSqlite())
        await db.Database.EnsureCreatedAsync();
    else
        await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(scope.ServiceProvider, app.Configuration, logger);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
