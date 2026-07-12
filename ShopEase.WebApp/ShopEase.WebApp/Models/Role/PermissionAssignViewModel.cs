namespace ShopEase.WebApp.Models.Role
{
    public class PermissionAssignViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<ModulePermissionViewModel> Permissions { get; set; }
    }
    public class SavePermissionsRequest
    {
        public int RoleId { get; set; }
        public List<ModulePermissionViewModel> Permissions { get; set; }
    }
}