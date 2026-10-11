namespace EzItam.Web.Api;

public static class ApiClientRegistration
{
    public static IServiceCollection AddItamApi(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<ApiTokenHandler>();
        services.AddHttpClient<ItamApiClient>(c =>
        {
            var url = (config["Api:BaseUrl"] ?? "http://localhost:5100/").TrimEnd('/') + "/";
            c.BaseAddress = new Uri(url);
            c.Timeout = TimeSpan.FromSeconds(100);
        }).AddHttpMessageHandler<ApiTokenHandler>();
        return services;
    }
}
