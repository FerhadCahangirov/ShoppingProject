using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShoppingMvc.Contexts;
using ShoppingMvc.ViewModels.BasketVm;
using ShoppingMvc.ViewModels.ProductVm;

namespace ShoppingMvc.ViewComponents.ShoppingViewComponents
{
    [Authorize(Policy = "AuthRequiredPolicy")]
    public class ShoppingViewComponent : ViewComponent
    {
        EvaraDbContext _db { get; set; }

        public ShoppingViewComponent(EvaraDbContext db)
        {
            _db = db;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var username = HttpContext.User.Identity?.Name;

            var basketItemsQuery = _db.BasketItems
                .Include(bi => bi.Basket)
                .ThenInclude(b => b.User)
                .Include(bi => bi.Product)
                .ThenInclude(bi => bi.ProductImages)
                .Where(bi => bi.Basket.User.UserName == username && !bi.Basket.IsOrdered);

            decimal totalPrice = basketItemsQuery.ToList().Sum(bi =>
                ((bi.Product.Price - (bi.Product.Price * bi.Product.DiscountRate / 100)) + bi.Product.ShippingFee) * bi.Count);


            var basketItems = await basketItemsQuery
            .Select(bi => new BasketProductItemVm()
            {
                Id = bi.Id,
                Count = bi.Count,
                Product = bi.Product.FromProduct_ToProductListItemVm(),
                Basket = bi.Basket,
                TotalItemPrice = (((bi.Product.Price - (bi.Product.Price * bi.Product.DiscountRate / 100)) + bi.Product.ShippingFee) * bi.Count).ToString("")
            }).ToListAsync();

            var viewModel = new BasketTotalVm()
            {
                TotalPrice = totalPrice.ToString("0.00"),
                Items = basketItems
            };

            return View(viewModel);
        }
    }
}
