using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.ViewModels.HomeVm;
using ShoppingMvc.ViewModels.ProductVm;

namespace ShoppingMvc.Controllers
{
    [Authorize(Policy = "AuthRequiredPolicy")]
    public class ShopCartController : Controller
    {
        EvaraDbContext _db { get; set; }

        public ShopCartController(EvaraDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View(); 
        }
        public IActionResult GetShopCart()
        {
            return ViewComponent("ShopCart");
        }
    }
}
