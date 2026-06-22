using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Helpers;

namespace Ecommerce.Web.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class PermissionFilter : Attribute, IActionFilter
    {
        #region Properties
        private readonly string _moduleCode;
        private readonly string _permissionType;
        #endregion

        #region Constructor
        public PermissionFilter(string moduleCode, string permissionType = PermissionConstants.CanView)
        {
            _moduleCode = moduleCode;
            _permissionType = permissionType;
        }
        #endregion

        #region OnActionExecuting
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var hasPermission = SessionHelper.HasPermission(session, _moduleCode, _permissionType);

            if (!hasPermission)
            {
                context.Result = new RedirectResult(RouteConstants.AccessDenied);
            }
        }
        #endregion

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}