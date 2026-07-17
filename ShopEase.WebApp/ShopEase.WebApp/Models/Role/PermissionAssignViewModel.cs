using ShopEase.WebApp.Models.Common;

namespace ShopEase.WebApp.Models.Role
{
    public class PermissionAssignViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<PermissionModel> Permissions { get; set; } = new();
    }
    public class SavePermissionsRequest
    {
        public int RoleId { get; set; }
        public List<PermissionModel> Permissions { get; set; }
    }
}