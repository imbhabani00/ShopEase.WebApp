using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShopEase.WebApp.Constants;

namespace Ecommerce.Web.Filters
{
    public class AuthFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Session.GetString(SessionConstants.AccessToken);
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectResult(RouteConstants.Login);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}