namespace ShopEase.WebApp.Extensions
{
    public static class SessionExtension
    {
        public static IServiceCollection AddSessionConfig(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var timeoutMinutes = configuration.GetValue<int>("Session:TimeoutMinutes");
            var cookieName = configuration.GetValue<string>("Session:CookieName");

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(timeoutMinutes > 0 ? timeoutMinutes : 60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Name = cookieName ?? "Ecommerce.Web.Session";
            });

            return services;
        }
    }
}