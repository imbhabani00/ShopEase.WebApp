namespace ShopEase.WebApp.Helpers
{
    public class CookieHelper
    {
        public static void SetRefreshTokenCookie(HttpResponse response, string refreshToken, int expiryDays)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(expiryDays)
            };
            response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
        }

        public static string? GetRefreshTokenCookie(HttpRequest request)
            => request.Cookies["RefreshToken"];

        public static void DeleteRefreshTokenCookie(HttpResponse response)
        {
            response.Cookies.Delete("RefreshToken");
        }
    }
}