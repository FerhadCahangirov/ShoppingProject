using Microsoft.AspNetCore.Mvc;

namespace ShoppingMvc.Controllers
{
    public class AnouncementController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}
