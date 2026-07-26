namespace ShopEase.WebApp.Models.Common
{
    public class PermissionModel
    {
        public int RoleId { get; set; }
        public int ModuleId { get; set; }
        public string ModuleCode { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanInactive { get; set; }
    }

    public class PermissionAssignViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<PermissionModel> Permissions { get; set; } = new();
    }
    public class SavePermissionsRequest
    {
        public int RoleId { get; set; }
        public int ModuleId { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanInactive { get; set; }
    }
}
