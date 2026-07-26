using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Helpers;

namespace Ecommerce.Web.Controllers
{
    public class BaseController : Controller
    {
        #region Session Properties
        protected string? AccessToken => HttpContext.Session.GetString(SessionConstants.AccessToken);
        protected string? RefreshToken => HttpContext.Session.GetString(SessionConstants.RefreshToken);
        protected int? CurrentUserId => HttpContext.Session.GetInt32(SessionConstants.UserId);
        protected int? CurrentTenantId => HttpContext.Session.GetInt32(SessionConstants.TenantId);
        protected string? CurrentRoleCode => HttpContext.Session.GetString(SessionConstants.RoleCode);
        protected string? CurrentRoleName => HttpContext.Session.GetString(SessionConstants.RoleName);
        protected string? CurrentUserFullName => HttpContext.Session.GetString(SessionConstants.UserFullName);
        #endregion

        #region Permission Helpers
        protected bool HasPermission(string flatPermissionKey)
            => SessionHelper.HasPermission(HttpContext.Session, flatPermissionKey);
        #endregion

        #region TempData Helpers
        protected void SetSuccessMessage(string message)
            => TempData["SuccessMessage"] = message;

        protected void SetErrorMessage(string message)
            => TempData["ErrorMessage"] = message;
        #endregion

        #region OnActionExecuting
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!string.IsNullOrEmpty(AccessToken))
            {
                ViewBag.Permissions = SessionHelper.BuildPermissionViewModel(HttpContext.Session);
            }

            base.OnActionExecuting(context);
        }
        #endregion
    }
}