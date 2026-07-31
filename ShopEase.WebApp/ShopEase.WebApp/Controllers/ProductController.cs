using Ecommerce.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ShopEase.WebApp.Controllers
{
    public class ProductController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
