using Ecommerce.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using ShopEase.WebApp.Constants;
using ShopEase.WebApp.Helpers;

namespace ShopEase.WebApp.Controllers
{
    public class DashboardController : BaseController
    {
        public IActionResult Index()
        {
            var forcePasswordChange = SessionHelper.GetForcePasswordChange(HttpContext.Session);
            if (forcePasswordChange)
            {
                return Redirect(RouteConstants.AccessDenied);
            }

            return View();
        }
    }
}