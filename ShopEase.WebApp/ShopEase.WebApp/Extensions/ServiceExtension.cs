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

        var webAssembly = typeof(Program).Assembly;

        // Auto-register all Web Services
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

        // Auto-register all Repositories
        foreach (var type in webAssembly.GetTypes()
            .Where(x => x.IsClass
                     && !x.IsAbstract
                     && x.Name.EndsWith("Repository")))
        {
            var interfaceType = type.GetInterfaces()
                .FirstOrDefault(i => i.Name != "IDisposable");

            if (interfaceType != null)
                services.AddScoped(interfaceType, type);
        }

        return services;
    }
}