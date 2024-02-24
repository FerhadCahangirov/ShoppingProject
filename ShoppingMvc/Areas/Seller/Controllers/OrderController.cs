using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.Enums;
using ShoppingMvc.Models;
using ShoppingMvc.Models.Identity;
using ShoppingMvc.ViewModels.CommonVm;
using ShoppingMvc.ViewModels.OrderVm;
using ShoppingMvc.ViewModels.ProductVm;

namespace ShoppingMvc.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Policy = "SellerPolicy")]
    public class OrderController : Controller
    {
        private readonly EvaraDbContext _db;

        public OrderController(EvaraDbContext db)
        {
            _db = db;
        }

        private async Task<AppUser> GetAuthenticatedUserAsync()
         => await _db.Users
                .FirstOrDefaultAsync(u => u.UserName == HttpContext.User.Identity.Name);


        public async Task<IActionResult> Index(int page = 1, int size = 12)
        {
            AppUser seller = await GetAuthenticatedUserAsync();

            int take = size;
            int skip = (page - 1) * take;

            var query = _db.Orders
                .Include(o => o.Basket)
                .ThenInclude(b => b.BasketItems)
                .ThenInclude(bi => bi.Product)
                .ThenInclude(p => p.SellerData)
                .ThenInclude(sd => sd.Seller)
                .Include(o => o.Basket)
                .ThenInclude(b => b.User)
                .Where(o => o.Basket.BasketItems.Any(bi => bi.Product.SellerData.Seller.UserName == seller.UserName))
                .Select(o => o.FromOrder_ToSellerOrderListItemVm(seller));

            int totalCount = query.Count();

            List<SellerOrderListItemVm> paginatedOrders = await query.Skip(skip).Take(take).ToListAsync();

            PaginationVm<IEnumerable<SellerOrderListItemVm>> pagination = new PaginationVm<IEnumerable<SellerOrderListItemVm>>(totalCount, page, (int)Math.Ceiling((decimal)totalCount / take), paginatedOrders, take);

            return View(pagination);
        }


        public async Task<IActionResult> OrderDetails(int id)
        {
            var shippingStatus = Enum.GetValues(typeof(ShippingStatus))
                .Cast<ShippingStatus>()
                .Select(p => new SelectListItem
                {
                    Value = p.ToString(),
                    Text = p.ToString()
                });

            ViewBag.ShippingStatus = new SelectList(shippingStatus, "Value", "Text");

            AppUser seller = await GetAuthenticatedUserAsync();

            Order? order = _db.Orders
                .Include(o => o.Basket)
                .ThenInclude(b => b.BasketItems)
                .ThenInclude(bi => bi.Product)
                .ThenInclude(p => p.SellerData)
                .ThenInclude(sd => sd.Seller)
                .Include(o => o.Basket)
                .ThenInclude(b => b.BasketItems)
                .ThenInclude(bi => bi.Product)
                .ThenInclude(b => b.ProductImages)
                .Include(o => o.Basket)
                .ThenInclude(b => b.User)
                .SingleOrDefault(o => o.Id == id);

            if (order == null) return NotFound();

            SellerOrderListItemVm sellerOrderListItemVm = order.FromOrder_ToSellerOrderListItemVm(seller);

            return View(sellerOrderListItemVm);
        }

        public async Task<IActionResult> OrderTracking(int id)
        {
            var shippingStatus = Enum.GetValues(typeof(ShippingStatus))
                .Cast<ShippingStatus>()
                .Select(p => new SelectListItem
                {
                    Value = p.ToString(),
                    Text = p.ToString()
                });

            ViewBag.ShippingStatus = new SelectList(shippingStatus, "Value", "Text");

            AppUser seller = await GetAuthenticatedUserAsync();

            Order? order = _db.Orders
                .Include(o => o.Basket)
                .ThenInclude(b => b.BasketItems)
                .ThenInclude(bi => bi.Product)
                .ThenInclude(p => p.SellerData)
                .ThenInclude(sd => sd.Seller)
                .Include(o => o.Basket)
                .ThenInclude(b => b.BasketItems)
                .ThenInclude(bi => bi.Product)
                .ThenInclude(b => b.ProductImages)
                .Include(o => o.Basket)
                .ThenInclude(b => b.User)
                .SingleOrDefault(o => o.Id == id);

            if (order == null) return NotFound();

            SellerOrderListItemVm sellerOrderListItemVm = order.FromOrder_ToSellerOrderListItemVm(seller);

            return View(sellerOrderListItemVm);
        }
    }
}
