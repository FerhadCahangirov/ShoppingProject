using Microsoft.AspNetCore.Mvc;

namespace ShoppingMvc.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
