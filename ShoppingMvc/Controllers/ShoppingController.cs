using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShoppingMvc.Contexts;
using ShoppingMvc.ViewModels.BasketVm;
using ShoppingMvc.ViewModels.CommonVm;
using ShoppingMvc.ViewModels.HomeVm;
using ShoppingMvc.ViewModels.ProductVm;
using ShoppingMvc.ViewModels.SliderVm;

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
            var items = _db.Products.Where(p => !p.IsDeleted).Take(take).Select(c => new ProductListItemVm
            {
                Id = c.Id,
                CreatedTime = c.CreatedTime,
                UpdatedTime = c.UpdatedTime,
                ImageUrl = c.ImageUrl,
                IsDeleted = c.IsDeleted,
                IsArchived = c.IsArchived,
                Title = c.Title,
                Description = c.Description,
                CostPrice = c.CostPrice,
                SellPrice = c.SellPrice,
                Category = c.Category,
                RateRange = c.RateRange,
                Tags = c.TagProduct.Select(p => p.Tag)
            });
            int count = await _db.Products.CountAsync(x => !x.IsDeleted);
            PaginationVm<IEnumerable<ProductListItemVm>> pag = new(count, 1, (int)Math.Ceiling((decimal)count / take), items);
            HomeVm vm = new HomeVm
            {
                ProductListItems = await _db.Products.Select(c => new ProductListItemVm
                {
                    Id = c.Id,
                    CreatedTime = c.CreatedTime,
                    UpdatedTime = c.UpdatedTime,
                    ImageUrl = c.ImageUrl,
                    IsDeleted = c.IsDeleted,
                    IsArchived = c.IsArchived,
                    Title = c.Title,
                    Description = c.Description,
                    CostPrice = c.CostPrice,
                    SellPrice = c.SellPrice,
                    Category = c.Category,
                    RateRange = c.RateRange,
                    Tags = c.TagProduct.Select(p => p.Tag)
                }).ToListAsync(),
            };
            return View(vm);
            
        }
        public async Task<IActionResult> ProductPagination(int page = 1, int count = 8)
        {
            var items = _db.Products.Where(p => !p.IsDeleted).Skip((page - 1) * count).Take(count).Select(c => new ProductListItemVm
            {
                Id = c.Id,
                CreatedTime = c.CreatedTime,
                UpdatedTime = c.UpdatedTime,
                ImageUrl = c.ImageUrl,
                IsDeleted = c.IsDeleted,
                IsArchived = c.IsArchived,
                Title = c.Title,
                Description = c.Description,
                CostPrice = c.CostPrice,
                SellPrice = c.SellPrice,
                Category = c.Category,
                RateRange = c.RateRange,
                Tags = c.TagProduct.Select(p => p.Tag)
            });
            int totalCount = await _db.Products.CountAsync(x => !x.IsDeleted);
            PaginationVm<IEnumerable<ProductListItemVm>> pag = new(totalCount, page, (int)Math.Ceiling((decimal)totalCount / count), items);

            return PartialView("_ShopProductPartial", pag);
        }
        
        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id == null || id <= 0) return BadRequest();
            if (!await _db.Products.AnyAsync(p => p.Id == id)) return NotFound();
            var basket = JsonConvert.DeserializeObject<List<BasketProductandCountVm>>(HttpContext.Request.Cookies["basket"] ?? "[]");
            var existItem = basket.Find(p => p.Id == id);
            if (existItem == null)
            {
                basket.Add(new BasketProductandCountVm
                {
                    Id = (int)id,
                    Count = 1

                });
            }
            else
            {
                existItem.Count++;
            }
            HttpContext.Response.Cookies.Append("basket", JsonConvert.SerializeObject(basket));
            return Ok();
        }
        public async Task<IActionResult> RemoveBasket(int? id)
        {
            if (id == null || id <= 0) return BadRequest();
            if (!await _db.Products.AnyAsync(p => p.Id == id)) return NotFound();
            var dasket = JsonConvert.DeserializeObject<List<BasketProductandCountVm>>(HttpContext.Request.Cookies["basket"] ?? "[]");
            var existItem = dasket.Find(b => b.Id == id);
            if (existItem.Count == 1)
            {
                dasket.Remove(new BasketProductandCountVm()
                {
                    Id = (int)id,
                    Count = 1
                });
            }
            else
            {
                existItem.Count--;
            }
            HttpContext.Response.Cookies.Append("basket", JsonConvert.SerializeObject(dasket));
            return Ok();
        }

    }
}
