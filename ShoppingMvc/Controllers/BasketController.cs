using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingMvc.Models.Identity;
using ShoppingMvc.Models;
using ShoppingMvc.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ShoppingMvc.Controllers
{
    public class BasketController : Controller
    {
        EvaraDbContext _db { get; set; }

        public BasketController(EvaraDbContext db)
        {
            _db = db;
        }

        private async Task<AppUser> GetAuthenticatedUserAsync()
         => await _db.Users
            .Include(u => u.Baskets)
            .ThenInclude(b => b.BasketItems)
            .ThenInclude(b => b.Product)
            .ThenInclude(p => p.ProductImages)
            .FirstOrDefaultAsync(u => u.UserName == HttpContext.User.Identity.Name);


        public IActionResult Index()
        {
            return ViewComponent("Basket");
        }

        [HttpPost]
        [Authorize(Policy = "AuthRequiredPolicy")]
        public async Task<IActionResult> AddProductToBasket(int id)
        {
            AppUser? user = await GetAuthenticatedUserAsync();

            Product? product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null) return NotFound();

            if (user.Baskets != null && user.Baskets.Any(b => !b.IsOrdered))
            {
                var basket = user.Baskets.Single(b => !b.IsOrdered);

                var basketItems = basket.BasketItems;

                if (basketItems != null && basketItems.Any(bi => bi.Product.Id == product.Id))
                {
                    var basketItem = basketItems.First(bi => bi.Product.Id == product.Id);
                    basketItem.Count += 1;

                    _db.BasketItems.Update(basketItem);

                    await _db.SaveChangesAsync();

                    return new JsonResult(new { Success = true });
                }

                BasketItem item = new BasketItem()
                {
                    Product = product,
                    Count = 1,
                    Basket = basket
                };

                await _db.BasketItems.AddAsync(item);
                await _db.SaveChangesAsync();
                return new JsonResult(new { Success = true });
            }

            Basket newBasket = new Basket()
            {
                IsOrdered = false,
                User = user,
            };

            await _db.Baskets.AddAsync(newBasket);

            await _db.SaveChangesAsync();

            newBasket = user.Baskets.Single(b => !b.IsOrdered);

            BasketItem newBasketItem = new BasketItem()
            {
                Product = product,
                Count = 1,
                Basket = newBasket
            };

            await _db.BasketItems.AddAsync(newBasketItem);

            await _db.SaveChangesAsync();

            return new JsonResult(new { Success = true });
        }


        [HttpDelete]
        [Authorize(Policy = "AuthRequiredPolicy")]
        public async Task<IActionResult> RemoveProductFromBasket(int id)
        {
            AppUser? user = await GetAuthenticatedUserAsync();
            var basketItem = user.Baskets?
                .Single(b => !b.IsOrdered)?
                .BasketItems
                .FirstOrDefault(bi => bi.Id == id);

            if (basketItem == null) return NotFound();

            _db.BasketItems.Remove(basketItem);    
            await _db.SaveChangesAsync();

            return new JsonResult(new { Success = true });
        }
    }
}
