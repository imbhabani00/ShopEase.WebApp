namespace ShopEase.WebApp.Configuration
{
    public static class ApiSettings
    {
        public static string AccessTokenEndpoint(string baseUrl, string version)
            => $"{baseUrl}/api/v{version}/token/access-token";

        public static string RefreshTokenEndpoint(string baseUrl, string version)
            => $"{baseUrl}/api/v{version}/token/refresh-token";
    }
}
