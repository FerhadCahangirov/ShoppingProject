using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShoppingMvc.Contexts;
using ShoppingMvc.ViewModels.BasketVm;
using System.Text.Json.Serialization;

namespace ShoppingMvc.ViewComponents.BasketViewComponents
{
    public class BasketViewComponent : ViewComponent
    {
        EvaraDbContext _db { get; set; }

        public BasketViewComponent(EvaraDbContext db)
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
                BasketProductItemVm itemVm = item.FromProduct_ToBasketProductItemVm();
                itemVm.Count = items.FirstOrDefault(x => x.Id == item.Id).Count;
                basketItems.Add(itemVm);
            }
            return View(basketItems);
        }
    }
}
