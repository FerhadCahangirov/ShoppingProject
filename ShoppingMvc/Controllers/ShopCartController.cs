using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.ViewModels.HomeVm;
using ShoppingMvc.ViewModels.ProductVm;

namespace ShoppingMvc.Controllers
{
    public class ShopCartController : Controller
    {
        EvaraDbContext _db { get; set; }

        public ShopCartController(EvaraDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            HomeVm vm = new HomeVm
            {
                ProductListItems = await _db.Products.Select(p => p.FromProduct_ToProductListItemVm()).ToListAsync()
            };
            return View();
        }
        public string GetCookie(string key)
        {
            return HttpContext.Request.Cookies[key] ?? "";
        }
        public IActionResult GetBasket()
        {
            return ViewComponent("Basket");
        }
    }
}
