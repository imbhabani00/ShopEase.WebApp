namespace ShopEase.WebApp.Models.Role
{
    public class ModulePermissionViewModel
    {
        public int PermissionId { get; set; }
        public int RoleId { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
