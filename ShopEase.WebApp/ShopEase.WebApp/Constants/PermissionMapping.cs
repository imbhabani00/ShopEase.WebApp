using ShopEase.WebApp.Models.Common;

namespace ShopEase.WebApp.Constants
{
    public static class PermissionMap
    {
        public static readonly Dictionary<string, (string ModuleCode, Func<PermissionModel, bool> Selector)> Map = new()
        {
            // User
            { PermissionConstants.User_View_List, ("USER", m => m.CanView) },
            { PermissionConstants.User_Add,       ("USER", m => m.CanAdd) },
            { PermissionConstants.User_Edit,      ("USER", m => m.CanEdit) },
            { PermissionConstants.User_Delete,    ("USER", m => m.CanDelete) },
            { PermissionConstants.User_Inactive,  ("USER", m => m.CanInactive) },

            // Customer
            { PermissionConstants.Customer_View_List, ("CUSTOMER", m => m.CanView) },
            { PermissionConstants.Customer_Add,       ("CUSTOMER", m => m.CanAdd) },
            { PermissionConstants.Customer_Edit,      ("CUSTOMER", m => m.CanEdit) },
            { PermissionConstants.Customer_Delete,    ("CUSTOMER", m => m.CanDelete) },
            { PermissionConstants.Customer_Inactive,  ("CUSTOMER", m => m.CanInactive) },

            // Role
            { PermissionConstants.Role_View_List, ("ROLE", m => m.CanView) },
            { PermissionConstants.Role_Add,       ("ROLE", m => m.CanAdd) },
            { PermissionConstants.Role_Edit,      ("ROLE", m => m.CanEdit) },
            { PermissionConstants.Role_Delete,    ("ROLE", m => m.CanDelete) },
            { PermissionConstants.Role_Inactive,  ("ROLE", m => m.CanInactive) },

            // Dashboard
            { PermissionConstants.Dashboard_View, ("DASHBOARD", m => m.CanView) },
        };
    }
}