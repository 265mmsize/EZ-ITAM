using System.Globalization;
using EzItam.Web.Api;
using EzItam.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// 본사 화면은 DB를 쓰지 않습니다. 로그인 계정 확인과 모든 데이터는 API 가 담당합니다.
builder.Services.AddControllersWithViews(options => options.Filters.Add<ApiExceptionFilter>());

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanEdit", p => p.RequireRole("Admin", "Manager"));
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

builder.Services.AddItamApi(builder.Configuration);

var app = builder.Build();

var culture = new CultureInfo("ko-KR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture }
});

if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
