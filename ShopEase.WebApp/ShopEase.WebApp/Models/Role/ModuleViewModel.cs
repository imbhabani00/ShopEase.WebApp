namespace ShopEase.WebApp.Models.Role
{
    public class ModuleViewModel
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public int? ParentModuleId { get; set; }
        public string? ParentModuleName { get; set; }
        public bool IsActive { get; set; }
    }
    public class ModuleViewModelList
    {
        public List<ModuleViewModel> Modules { get; set; }
        public int TotalCount { get; set; }
    }
}
