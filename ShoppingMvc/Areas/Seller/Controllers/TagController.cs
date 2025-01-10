using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.Models;
using ShoppingMvc.ViewModels.CommonVm;
using ShoppingMvc.ViewModels.ProductVm;
using ShoppingMvc.ViewModels.TagVm;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ShoppingMvc.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Policy = "SellerPolicy")]
    public class TagController : Controller
    {
        EvaraDbContext _db { get; set; }
        IWebHostEnvironment _env { get; set; }

        public TagController(EvaraDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private async Task<SellerData> GetActiveSellerAsync() =>
            await _db.SellerDatas
                .Include(x => x.Seller)
                .FirstAsync(x => x.Seller.UserName == HttpContext.User.Identity.Name);

        public async Task<IActionResult> TagsPartial(string? searchFilter, DateTime? dateFilter, string? statusFilter, int page = 1, int size = 4)
        {
            SellerData seller = await GetActiveSellerAsync();

            var query = _db.Tags
                .Include(t => t.Seller)
                .Where(t => t.Seller == seller)
                .Select(t => new TagListItemVm
                {
                    Id = t.Id,
                    CreatedTime = t.CreatedTime,
                    UpdatedTime = t.UpdatedTime,
                    IsDeleted = t.IsDeleted,
                    IsArchived = t.IsArchived,
                    Title = t.Title,

                });

            if (!string.IsNullOrEmpty(searchFilter))
            {
                searchFilter = searchFilter.ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(searchFilter));
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

            var paginatedData = filteredData.Skip((page - 1) * size).Take(size).ToList();

            PaginationVm<IEnumerable<TagListItemVm>> pag = new(total, page, (int)Math.Ceiling((decimal)total / size), paginatedData, size);
            return PartialView("TagsPartial", pag);
        }

        public async Task<IActionResult> Index()
        {
            SellerData seller = await GetActiveSellerAsync();

            int page = 1;
            int take = 4;
            int skip = (page - 1) * take;

            var query = _db.Tags
                .Include(t => t.Seller)
                .Where(t => t.Seller == seller)
                .Select(t => new TagListItemVm
                {
                    Id = t.Id,
                    CreatedTime = t.CreatedTime,
                    UpdatedTime = t.UpdatedTime,
                    IsDeleted = t.IsDeleted,
                    IsArchived = t.IsArchived,
                    Title = t.Title,
                });

            var data = await query.ToListAsync();

            int total = data.Count();

            var paginatedData = data.Skip(skip).Take(take).ToList();

            PaginationVm<IEnumerable<TagListItemVm>> pag = new PaginationVm<IEnumerable<TagListItemVm>>(total, page, (int)Math.Ceiling((decimal)total / take), paginatedData, take);

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
            return Redirect("/Seller/Tag/Index");
        }
        [HttpPost]
        public async Task<IActionResult> Create(TagCreateVm vm)
        {
            SellerData seller = await GetActiveSellerAsync();

            Tag tag = new Tag()
            {
                Title = vm.Title,
                Seller = seller
            };

            await _db.Tags.AddAsync(tag);
            await _db.SaveChangesAsync();
            TempData["Create"] = true;
            return Redirect("/Seller/Tag/Index");
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Tags.FindAsync(id);
            if (data == null) return NotFound();
            return View(new TagUpdatedVm
            {
                Title = data.Title,

            });
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, TagUpdatedVm vm)
        {
            TempData["Update"] = false;
            if (id == null || id < 0) return BadRequest();

            var data = await _db.Tags.FindAsync(id);
            if (data == null) return NotFound();
            data.Title = vm.Title;
            await _db.SaveChangesAsync();
            TempData["Update"] = true;
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteProduct(int? id)
        {

            if (id == null) return BadRequest();
            var data = await _db.Tags.FindAsync(id);
            if (data == null) return NotFound();

            data.IsDeleted = true;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> RestoreProduct(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Tags.FindAsync(id);
            if (data == null) return NotFound();
            data.IsDeleted = false;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> RestoreArchiveProduct(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Tags.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = false;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteFromData(int? id)
        {
            TempData["Delete"] = false;
            if (id == null) return BadRequest();
            var data = await _db.Tags.FindAsync(id);
            if (data == null) return NotFound();
            TempData["Delete"] = true;
            _db.Tags.Remove(data);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Archived(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Tags.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = true;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public JsonResult GetDistinctProductNames()
        {
            var distinctProductNames = _db.Tags
                .Select(p => p.Title)
                .Distinct()
                .ToList();

            return Json(distinctProductNames);
        }
    }
}
