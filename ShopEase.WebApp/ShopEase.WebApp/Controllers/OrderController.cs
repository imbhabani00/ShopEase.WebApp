using Microsoft.AspNetCore.Mvc;

namespace ShopEase.WebApp.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
