using Microsoft.AspNetCore.Mvc;

namespace ShopEase.WebApp.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
