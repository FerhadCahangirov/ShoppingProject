using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.Enums;
using ShoppingMvc.Models;
using ShoppingMvc.Models.Identity;
using ShoppingMvc.ViewModels.BasketVm;
using ShoppingMvc.ViewModels.CheckoutVm;
using ShoppingMvc.ViewModels.ProductVm;

namespace ShoppingMvc.Controllers
{
    [Authorize(Policy = "AuthRequiredPolicy")]
    public class CheckoutController : Controller
    {
        EvaraDbContext _db { get; set; }

        public CheckoutController(EvaraDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var paymentMethods = Enum.GetValues(typeof(PaymentMethods))
                .Cast<PaymentMethods>()
                .Select(p => new SelectListItem
                {
                    Value = p.ToString(),
                    Text = p.ToString()
                });

            ViewBag.PaymentMethods = new SelectList(paymentMethods, "Value", "Text");

            var username = HttpContext.User.Identity?.Name;

            var basketItemsQuery = _db.BasketItems
                .Include(bi => bi.Basket)
                .ThenInclude(b => b.User)
                .Include(bi => bi.Product)
                .ThenInclude(bi => bi.ProductImages)
                .Where(bi => bi.Basket.User.UserName == username && !bi.Basket.IsOrdered);

            decimal subTotalPrice = basketItemsQuery.ToList().Sum(bi =>
                (bi.Product.Price - (bi.Product.Price * bi.Product.DiscountRate / 100)) * bi.Count);

            decimal totalPrice = basketItemsQuery.ToList().Sum(bi =>
                ((bi.Product.Price - (bi.Product.Price * bi.Product.DiscountRate / 100)) + bi.Product.ShippingFee) * bi.Count);

            decimal totalShippingFee = basketItemsQuery.ToList().Sum(bi => bi.Product.ShippingFee);

            var basketItems = await basketItemsQuery
            .Select(bi => new BasketProductItemVm()
            {
                Id = bi.Id,
                Count = bi.Count,
                Product = bi.Product.FromProduct_ToProductListItemVm(),
                Basket = bi.Basket,
                TotalItemPrice = (((bi.Product.Price - (bi.Product.Price * bi.Product.DiscountRate / 100)) + bi.Product.ShippingFee) * bi.Count).ToString("0.00")
            }).ToListAsync();

            var basketViewModel = new BasketTotalVm()
            {
                SubTotalPrice = subTotalPrice.ToString("0.00"),
                TotalPrice = totalPrice.ToString("0.00"),
                TotalShippingFee = totalShippingFee.ToString("0,00"),
                Items = basketItems
            };

            CheckoutCompleteVm vm = new()
            {
                Basket = basketViewModel
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CheckoutComplete(CheckoutCompleteVm vm)
        {
            if (ModelState.IsValid)
                return RedirectToAction("Index", vm);

            var pm = vm.PaymentMethod;

            var username = HttpContext.User.Identity?.Name;

            AppUser? user = _db.Users
                .Include(u => u.Baskets)
                .ThenInclude(b => b.BasketItems)
                .ThenInclude(bi => bi.Product)
                .SingleOrDefault(u => u.UserName == username);

            if (user == null) return NotFound();

            Basket? basket = user.Baskets?.SingleOrDefault(b => !b.IsOrdered);

            if (basket == null) return NotFound();

            Order order = new Order
            {
                AdditionalAddressInfo = vm.AdditionalAddressInfo,
                Street = vm.Street,
                City = vm.City,
                State = vm.State,
                PostalCode = vm.PostalCode,
                CardholderName = vm.CardholderName,
                CardNumber = vm.CardNumber,
                ExpiryMonth = vm.ExpiryMonth,
                ExpiryYear = vm.ExpiryYear,
                CVV = vm.CVV,
                Basket = basket,
                PaymentMethod = vm.PaymentMethod,
                PhoneNumber = vm.PhoneNumber,
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            basket.IsOrdered = true;
            _db.Baskets.Update(basket);

            await _db.SaveChangesAsync();

            TempData["RedirectedToOrder"] = true;
            return RedirectToAction("Account", "Auth");
        }

    }
}
