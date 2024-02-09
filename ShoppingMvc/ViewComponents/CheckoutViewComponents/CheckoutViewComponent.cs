using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShoppingMvc.Contexts;
using ShoppingMvc.ViewModels.BasketVm;

namespace ShoppingMvc.ViewComponents.CheckoutViewComponents
{
    public class CheckoutViewComponent : ViewComponent
    {
        EvaraDbContext _db { get; set; }

        public CheckoutViewComponent(EvaraDbContext db)
        {
            _db = db;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = JsonConvert.DeserializeObject<List<BasketProductandCountVm>>(HttpContext.Request.Cookies["basket"] ?? "[]");
            var products = _db.Products.Where(p => items.Select(b => b.Id).Contains(p.Id));
            List<BasketProductItemVm> basketItems = new();
            foreach (var item in products)
            {
                basketItems.Add(new BasketProductItemVm
                {
                    Id = item.Id,
                    ImageUrl = item.ImageUrl,
                    Title = item.Title,
                    SellPrice = (float)item.SellPrice,
                    Count = items.FirstOrDefault(x => x.Id == item.Id).Count
                });
            }
            return View(basketItems);
        }
    }
}
