using Microsoft.AspNetCore.Mvc;

namespace NeonGrid.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}