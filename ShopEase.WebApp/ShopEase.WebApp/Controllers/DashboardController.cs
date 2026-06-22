using Microsoft.AspNetCore.Mvc;

namespace ShopEase.WebApp.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
