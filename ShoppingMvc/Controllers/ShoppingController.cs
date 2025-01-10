using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShoppingMvc.Contexts;
using ShoppingMvc.ViewModels.BasketVm;
using ShoppingMvc.ViewModels.CommonVm;
using ShoppingMvc.ViewModels.HomeVm;
using ShoppingMvc.ViewModels.ProductVm;

namespace ShoppingMvc.Controllers
{
    public class ShoppingController : Controller
    {
        EvaraDbContext _db { get; set; }

        public ShoppingController(EvaraDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            int take = 4;
            var items = _db.Products.Where(p => !p.IsDeleted && !p.IsArchived).Take(take).Select(p => p.FromProduct_ToProductListItemVm());
            int count = await _db.Products.CountAsync(x => !x.IsDeleted && !x.IsArchived);
            PaginationVm<IEnumerable<ProductListItemVm>> pag = new(count, 1, (int)Math.Ceiling((decimal)count / take), items);
            HomeVm vm = new HomeVm
            {
                ProductListItems = await _db.Products.Select(p => p.FromProduct_ToProductListItemVm()).ToListAsync(),
            };
            return View(vm);
        }

        public async Task<IActionResult> ProductPagination(int page = 1, int count = 8)
        {
            var items = _db.Products.Where(p => !p.IsDeleted && !p.IsArchived).Skip((page - 1) * count).Take(count).Select(p => p.FromProduct_ToProductListItemVm());
            int totalCount = await _db.Products.CountAsync(x => !x.IsDeleted && !x.IsArchived);
            PaginationVm<IEnumerable<ProductListItemVm>> pag = new(totalCount, page, (int)Math.Ceiling((decimal)totalCount / count), items);

            return PartialView("_ShopProductPartial", pag);
        }
    }
}
