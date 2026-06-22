using System.Security.Claims;

namespace ShopEase.WebApp.Extensions
{
    public static class ClaimExtension
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst("UserId")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        public static string? GetRoleCode(this ClaimsPrincipal user)
            => user.FindFirst("RoleCode")?.Value;

        public static string? GetRoleName(this ClaimsPrincipal user)
            => user.FindFirst("RoleName")?.Value;

        public static int GetTenantId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst("TenantId")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        public static int GetRoleId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst("RoleId")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}