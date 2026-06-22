using Newtonsoft.Json;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Models.Common;
using System.Text.Json.Serialization;

namespace ShopEase.WebApp.Helpers
{
    public class SessionHelper
    {
        public static void SetAccessToken(ISession session, string token)
            => session.SetString(SessionConstants.AccessToken, token);

        public static string? GetAccessToken(ISession session)
            => session.GetString(SessionConstants.AccessToken);

        public static void SetRefreshToken(ISession session, string token)
            => session.SetString(SessionConstants.RefreshToken, token);

        public static string? GetRefreshToken(ISession session)
            => session.GetString(SessionConstants.RefreshToken);

        public static void SetUserId(ISession session, int userId)
            => session.SetInt32(SessionConstants.UserId, userId);

        public static int? GetUserId(ISession session)
            => session.GetInt32(SessionConstants.UserId);

        public static void SetRoleCode(ISession session, string roleCode)
            => session.SetString(SessionConstants.RoleCode, roleCode);

        public static string? GetRoleCode(ISession session)
            => session.GetString(SessionConstants.RoleCode);

        public static void SetRoleName(ISession session, string roleName)
            => session.SetString(SessionConstants.RoleName, roleName);

        public static string? GetRoleName(ISession session)
            => session.GetString(SessionConstants.RoleName);

        public static void SetTenantId(ISession session, int tenantId)
            => session.SetInt32(SessionConstants.TenantId, tenantId);

        public static int? GetTenantId(ISession session)
            => session.GetInt32(SessionConstants.TenantId);

        public static void SetUserFullName(ISession session, string fullName)
            => session.SetString(SessionConstants.UserFullName, fullName);

        public static string? GetUserFullName(ISession session)
            => session.GetString(SessionConstants.UserFullName);

        public static void SetPermissions(ISession session, List<PermissionModel> permissions)
            => session.SetString(SessionConstants.Permissions, JsonConvert.SerializeObject(permissions));

        public static List<PermissionModel> GetPermissions(ISession session)
        {
            var json = session.GetString(SessionConstants.Permissions);
            if (string.IsNullOrEmpty(json)) return new List<PermissionModel>();
            return JsonConvert.DeserializeObject<List<PermissionModel>>(json) ?? new List<PermissionModel>();
        }

        public static bool HasPermission(ISession session, string moduleCode, string permissionType)
        {
            var permissions = GetPermissions(session);
            var module = permissions.FirstOrDefault(p =>
                p.ModuleCode.Equals(moduleCode, StringComparison.OrdinalIgnoreCase));

            if (module == null) return false;

            return permissionType switch
            {
                PermissionConstants.CanView => module.CanView,
                PermissionConstants.CanAdd => module.CanAdd,
                PermissionConstants.CanEdit => module.CanEdit,
                PermissionConstants.CanDelete => module.CanDelete,
                _ => false
            };
        }

        public static void ClearSession(ISession session)
            => session.Clear();
    }
}