namespace ShopEase.WebApp.Models.Permission
{
    public class PermissionViewModel
    {
        public Dictionary<string, bool> Permissions { get; set; } = new();

        public bool Has(string flatPermissionKey)
            => Permissions.TryGetValue(flatPermissionKey, out var value) && value;
    }
}