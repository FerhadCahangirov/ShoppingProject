using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.Helpers;
using ShoppingMvc.Models.Cards;
using ShoppingMvc.Models.Categories;
using ShoppingMvc.ViewModels.CategoryVm;
using ShoppingMvc.ViewModels.CommonVm;

namespace ShoppingMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        EvaraDbContext _db {  get; set; }

        public CategoryController(EvaraDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> ProductPagination(int page = 1, int count = 8)
        {
            var items = await _db.Categorys.Skip((page - 1) * count).Take(count).Select(c => new CategoryListItemVm
            {
                Id = c.Id,
                CreatedTime = c.CreatedTime,
                UpdatedTime = c.UpdatedTime,
                IsDeleted = c.IsDeleted,
                IsArchived = c.IsArchived,
                Name = c.Name,
            }).ToListAsync();
            int totalCount = await _db.Categorys.CountAsync();
            PaginationVm<IEnumerable<CategoryListItemVm>> pag = new(totalCount, page, (int)Math.Ceiling((decimal)totalCount / count), items);
            return PartialView("_CategoryPaginationPartial", pag);
        }
        public async Task<IActionResult> Index(string categoryFilter, DateTime? dateFilter, string statusFilter, int page = 1)
        {
            int take = 4;
            int skip = (page - 1) * take;

            var query = _db.Categorys.Select(c => new CategoryListItemVm
            {
                Id = c.Id,
                CreatedTime = c.CreatedTime,
                UpdatedTime = c.UpdatedTime,
                IsDeleted = c.IsDeleted,
                IsArchived = c.IsArchived,
                Name = c.Name,
            });

            
            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All categories")
            {
                query = query.Where(c => c.Name == categoryFilter);
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

            int total = filteredData.Count();

            var paginatedData = filteredData.Skip(skip).Take(take).ToList();

            PaginationVm<IEnumerable<CategoryListItemVm>> pag = new PaginationVm<IEnumerable<CategoryListItemVm>>(total, page, (int)Math.Ceiling((decimal)total / take), paginatedData);

            if (paginatedData.Count == 0)
            {
                ViewBag.Message = "Nothing Found";
            }

            return View(pag);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }
        public async Task<IActionResult> Cancel()
        {
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            if (await _db.Categorys.AnyAsync(x => x.Name == vm.Name))
            {
                ModelState.AddModelError("Name", vm.Name + " already exist");
                return View(vm);
            }
            Category product = new Category()
            {
                Name = vm.Name,
            };
            _db.Categorys.AddAsync(product);
            await _db.SaveChangesAsync();
            TempData["Response"] = true;
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Categorys.FindAsync(id);
            if (data == null) return NotFound();
            return View(new CategoryUpdateVm
            {
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
                ViewBag.Categories = _db.Categorys;
                return View(vm);
            }
            var data = await _db.Categorys.FindAsync(id);
            if (data == null) return NotFound();
             data.Name = vm.Name;
            await _db.SaveChangesAsync();
            TempData["Update"] = true;
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteProduct(int? id)
        {

            if (id == null) return BadRequest();
            var data = await _db.Categorys.FindAsync(id);
            if (data == null) return NotFound();

            data.IsDeleted = true;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> RestoreProduct(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Categorys.FindAsync(id);
            if (data == null) return NotFound();
            data.IsDeleted = false;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> RestoreArchiveProduct(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Categorys.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = false;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteFromData(int? id)
        {
            TempData["Delete"] = false;
            if (id == null) return BadRequest();
            var data = await _db.Categorys.FindAsync(id);
            if (data == null) return NotFound();
            TempData["Delete"] = true;
            _db.Categorys.Remove(data);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Archived(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Categorys.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = true;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
