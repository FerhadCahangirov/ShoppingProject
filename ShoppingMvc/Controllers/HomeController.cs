using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.ViewModels.HomeVm;
using ShoppingMvc.ViewModels.ProductVm;
using ShoppingMvc.ViewModels.SliderVm;

namespace ShoppingMvc.Controllers
{
    public class HomeController : Controller
    {
        EvaraDbContext _db {  get; set; }

        public HomeController(EvaraDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            HomeVm vm = new HomeVm
            {
                SliderListItems = await _db.Sliders.Select(c => new SliderListItemVm
                {
                    Id = c.Id,
                    CreatedTime = c.CreatedTime,
                    UpdatedTime = c.UpdatedTime,
                    ImageUrl = c.ImageUrl,
                    IsDeleted = c.IsDeleted,
                    Title = c.Title,
                    Description = c.Description,
                    Discount = c.Discount,
                    Button = c.Button,
                }).ToListAsync(),
                ProductListItems = await _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .Where(p => !p.IsDeleted)
                    .Select(p => p.FromProduct_ToProductListItemVm())
                    .ToListAsync()
            };

            return View(vm);
        }


        public IActionResult AccessDenied()
        {
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
