using Microsoft.Extensions.Options;
using ShopEase.WebApp.Configuration;
public static class ServiceExtension
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // AppSettings
        services.Configure<AppSettings>(configuration);
        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<AppSettings>>().Value);

        // HttpClient
        services.AddHttpClient("EcommerceApi", client =>
        {
            client.BaseAddress = new Uri(
                configuration["EcommerceApi:BaseUrl"]!);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Auto-register all Web Services
        var webAssembly = typeof(Program).Assembly;

        foreach (var type in webAssembly.GetTypes()
            .Where(x => x.IsClass
                     && !x.IsAbstract
                     && x.Name.EndsWith("Service")
                     && x.Name != "BaseService"))
        {
            var interfaceType = type.GetInterfaces()
                .FirstOrDefault(i =>
                    i.Name != "IDisposable" &&
                    i.Name != "IBaseService");

            if (interfaceType != null)
                services.AddScoped(interfaceType, type);
        }

        return services;
    }
}