using System.Globalization;
using EzItam.Web.Api;
using EzItam.Web.Infrastructure;
using EzItam.Web.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// 본사 화면은 DB를 쓰지 않습니다. 로그인 계정 확인과 모든 데이터는 API 가 담당합니다.
builder.Services.AddLocalization();
builder.Services.AddControllersWithViews(options => options.Filters.Add<ApiExceptionFilter>())
    .AddDataAnnotationsLocalization(o => o.DataAnnotationLocalizerProvider = (_, _) => new JsonStringLocalizer<SharedResource>(Strings.Store!));

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

// 다국어: ko(기본), en, vi. 문자열은 Resources/Strings.{culture}.json 에서 읽습니다.
builder.Services.AddSingleton<JsonLocalizerStore>();
builder.Services.AddSingleton(typeof(IStringLocalizer<>), typeof(JsonStringLocalizer<>));

var app = builder.Build();
Strings.Store = app.Services.GetRequiredService<JsonLocalizerStore>();

var supported = new[] { new CultureInfo("ko-KR"), new CultureInfo("en-US"), new CultureInfo("vi-VN") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(supported[0]),
    SupportedCultures = supported,
    SupportedUICultures = supported
});

if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
