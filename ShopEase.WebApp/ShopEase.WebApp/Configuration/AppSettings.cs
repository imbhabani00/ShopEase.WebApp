using ShopEase.WebApp.Models.Email;

namespace ShopEase.WebApp.Configuration
{
    public class AppSettings
    {
        public EcommerceApiSettings EcommerceApi { get; set; } = new();
        public EmailSettings EmailSettings { get; set; } = new();
        public ConnectionStringsSettings ConnectionStrings { get; set; } = new();
        public SessionSettings Session { get; set; } = new();
    }

    public class EcommerceApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Version { get; set; } = "1";
    }

    public class ConnectionStringsSettings
    {
        public string ShopEaseConnection { get; set; } = string.Empty;
    }

    public class SessionSettings
    {
        public int TimeoutMinutes { get; set; } = 60;
        public string CookieName { get; set; } = "Ecommerce.Web.Session";
    }
}
