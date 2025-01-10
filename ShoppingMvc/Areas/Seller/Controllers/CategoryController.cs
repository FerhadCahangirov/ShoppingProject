using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using ShoppingMvc.Contexts;
using ShoppingMvc.Models;
using ShoppingMvc.ViewModels.CategoryVm;
using ShoppingMvc.ViewModels.CommonVm;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ShoppingMvc.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Policy = "SellerPolicy")]
    public class CategoryController : Controller
    {
        EvaraDbContext _db { get; set; }

        public CategoryController(EvaraDbContext db)
        {
            _db = db;
        }

        private async Task<SellerData> GetActiveSellerAsync() =>
            await _db.SellerDatas
                .Include(x => x.Seller)
                .FirstAsync(x => x.Seller.UserName == HttpContext.User.Identity.Name);


        public async Task<IActionResult> CategoriesPartial(string? searchFilter, DateTime? dateFilter, string? statusFilter, int page = 1, int size = 4)
        {
            SellerData seller = await GetActiveSellerAsync();

            var query = _db.Categories
                .Include(c => c.Seller)
                .Where(c => c.Seller == seller)
                .Select(c => new CategoryListItemVm
                {
                    Id = c.Id,
                    CreatedTime = c.CreatedTime,
                    UpdatedTime = c.UpdatedTime,
                    IsDeleted = c.IsDeleted,
                    IsArchived = c.IsArchived,
                    Name = c.Name,
                });

            if (!string.IsNullOrEmpty(searchFilter))
            {
                searchFilter = searchFilter.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(searchFilter));
            }

            if (dateFilter.HasValue)
            {
                DateTime filterDate = dateFilter.Value.Date;
                query = query.Where(c => c.CreatedTime.Value.Date == filterDate);
            }

            query = query.OrderBy(c => c.CreatedTime);

            var results = query.ToList();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All statuses" && statusFilter != "Show all")
            {
                query = query.Where(c => c.IsDeleted && statusFilter == "Disabled" || !c.IsDeleted && !c.IsArchived && statusFilter == "Active" || c.IsArchived && statusFilter == "Archived");
            }

            var filteredData = await query.ToListAsync();
            var totalCount = filteredData.Count;

            var paginatedData = filteredData.Skip((page - 1) * size).Take(size).ToList();

            PaginationVm<IEnumerable<CategoryListItemVm>> pagination = new(totalCount, page, (int)Math.Ceiling((decimal)totalCount / size), paginatedData, size);

            if (paginatedData.Count == 0)
            {
                ViewBag.Message = "Nothing Found";
            }

            return PartialView("CategoriesPartial", pagination);
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            SellerData seller = await GetActiveSellerAsync();

            int size = 4;
            int skip = (page - 1) * size;

            var query = _db.Categories
                .Include(c => c.Seller)
                .Where(c => c.Seller == seller)
                .Select(c => new CategoryListItemVm
            {
                Id = c.Id,
                CreatedTime = c.CreatedTime,
                UpdatedTime = c.UpdatedTime,
                IsDeleted = c.IsDeleted,
                IsArchived = c.IsArchived,
                Name = c.Name,
            });

            var data = await query.ToListAsync();

            int total = data.Count();

            var paginatedData = data.Skip(skip).Take(size).ToList();

            PaginationVm<IEnumerable<CategoryListItemVm>> pag = new(total, page, (int)Math.Ceiling((decimal)total / size), paginatedData, size);

            if (paginatedData.Count == 0)
            {
                ViewBag.Message = "Nothing Found";
            }

            return View(pag);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        public async Task<IActionResult> Cancel()
        {
            return Redirect("/Seller/Category/Index");
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateVm vm)
        {
            SellerData seller = await GetActiveSellerAsync();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            if (await _db.Categories.AnyAsync(x => x.Name == vm.Name))
            {
                ModelState.AddModelError("Name", vm.Name + " already exist");
                return View(vm);
            }

            Category categroy = new Category()
            {
                Name = vm.Name,
                Seller = seller,
            };

            _db.Categories.AddAsync(categroy);
            await _db.SaveChangesAsync();
            TempData["Response"] = true;
            return Redirect("/Seller/Category/Index");

        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Categories.FindAsync(id);
            if (data == null) return NotFound();
            return View(new CategoryUpdateVm
            {
                Id = data.Id,
                Name = data.Name,
            });
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, CategoryUpdateVm vm)
        {
            TempData["Update"] = false;
            if (id == null || id < 0) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _db.Categories;
                return View(vm);
            }
            var data = await _db.Categories.FindAsync(id);
            if (data == null) return NotFound();
            data.Name = vm.Name;
            await _db.SaveChangesAsync();
            TempData["Update"] = true;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteProduct(int? id)
        {

            if (id == null) return BadRequest();
            var data = await _db.Categories.FindAsync(id);
            if (data == null) return NotFound();

            data.IsDeleted = true;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> RestoreProduct(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Categories.FindAsync(id);
            if (data == null) return NotFound();
            data.IsDeleted = false;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> RestoreArchiveProduct(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Categories.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = false;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteFromData(int? id)
        {
            TempData["Delete"] = false;
            if (id == null) return BadRequest();
            var data = await _db.Categories.FindAsync(id);
            if (data == null) return NotFound();
            TempData["Delete"] = true;
            _db.Categories.Remove(data);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Archived(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Categories.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = true;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
