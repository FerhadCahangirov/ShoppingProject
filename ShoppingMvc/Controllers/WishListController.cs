using Microsoft.AspNetCore.Mvc;
using ShoppingMvc.Contexts;

namespace ShoppingMvc.Controllers
{
    public class WishListController:Controller
    {
        EvaraDbContext _db { get; set; }

        public WishListController(EvaraDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
