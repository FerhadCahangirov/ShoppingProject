using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.Helpers;
using ShoppingMvc.Models;
using ShoppingMvc.Models.Cards;
using ShoppingMvc.Models.Categories;
using ShoppingMvc.ViewModels.CommonVm;
using ShoppingMvc.ViewModels.ProductVm;
using ShoppingMvc.ViewModels.SliderVm;

namespace ShoppingMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
       EvaraDbContext _db {  get; set; }
        IWebHostEnvironment _env { get; set; }

        public ProductController(EvaraDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> ProductPagination(int page = 1, int count = 8)
        {
            var items = await _db.Products.Skip((page - 1) * count).Take(count).Select(c => new ProductListItemVm
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
            }).ToListAsync();
            int totalCount = await _db.Products.CountAsync();
            PaginationVm<IEnumerable<ProductListItemVm>> pag = new(totalCount, page, (int)Math.Ceiling((decimal)totalCount / count), items);
            return PartialView("ProductPagination2", pag);
        }
        public async Task<IActionResult> Index(string categoryFilter, DateTime? dateFilter, string statusFilter, int page = 1)
        {
            int take = 4;
            int skip = (page - 1) * take;

            var query = _db.Products.Select(c => new ProductListItemVm
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

            // Apply category filter
            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All categories")
            {
                query = query.Where(c => c.Category.Name == categoryFilter);
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

            PaginationVm<IEnumerable<ProductListItemVm>> pag = new PaginationVm<IEnumerable<ProductListItemVm>>(total, page, (int)Math.Ceiling((decimal)total / take), paginatedData);

            if (paginatedData.Count == 0)
            {
                ViewBag.Message = "Nothing Found";
            }

            return View(pag);
        }



        public async Task<IActionResult> Create()
        {
            ViewBag.Tags = new SelectList(_db.Tags, "Id", "Title");
            ViewBag.Category = new SelectList(_db.Categorys, "Id", "Name");
            if (!ModelState.IsValid)
            {
                ViewBag.Category = new SelectList(_db.Categorys, "Id", "Name");
                return View();
            }
            return View();
          
        }
        public async Task<IActionResult> Cancel()
        {
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tags = new SelectList(_db.Tags, "Id", "Title");
                ViewBag.Category = new SelectList(_db.Categorys, "Id", "Name");
                return View(vm);

            }
            string filename = null;
            if (vm.MainImage != null)
            {
                if (!vm.MainImage.IsCorrectType())
                {
                    ModelState.AddModelError("MainImage", "Wrong fie type");
                }
                if (!vm.MainImage.IsValidSize(200))
                {
                    ModelState.AddModelError("MainImage", "Image must less than given kb");
                }
                filename = Guid.NewGuid() + Path.GetExtension(vm.MainImage.FileName);
                using (Stream fs = new FileStream(Path.Combine(_env.WebRootPath, "Assets", "assets", "products", filename), FileMode.Create))
                {
                    await vm.MainImage.CopyToAsync(fs);
                }
            }
            if (vm.CostPrice > vm.SellPrice)
            {
                ModelState.AddModelError("CostPrice", "Sell price must be bigger than cost price");
            }
            if (!await _db.Categorys.AnyAsync(c => c.Id == vm.CategoryId))
            {
                ViewBag.Tags = new SelectList(_db.Tags, "Id", "Title");
                ModelState.AddModelError("CategoryId", "Category doesnt exist");
                ViewBag.Category = new SelectList(_db.Categorys, "Id", "Name");
                return View(vm);
            }
            if (await _db.Tags.Where(c => vm.TagsId.Contains(c.Id)).Select(c => c.Id).CountAsync() != vm.TagsId.Count())
            {
                ViewBag.Category = new SelectList(_db.Categorys, "Id", "Name");
                ModelState.AddModelError("TagsId", "TagsId doesnt exist");
                ViewBag.Tags = new SelectList(_db.Tags, "Id", "Title");
                return View(vm);
            }
            Product product = new Product()
            {
                Title = vm.Title,
                Description = vm.Description,
                RateRange = vm.RateRange,
                CategoryId = (int)vm.CategoryId,
                TagProduct = vm.TagsId.Select(id => new ProductTag
                {
                    TagId = id,
                }).ToList(),
                CostPrice = vm.CostPrice,
                SellPrice = vm.SellPrice,
                ImageUrl = filename,
            };
            _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
            TempData["Create"] = true;
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();
            ViewBag.Category = new SelectList(_db.Categorys, "Id", "Name");
            ViewBag.Tags = new SelectList(_db.Tags, "Id", "Title");
            var data = await _db.Products
                .Include(p => p.TagProduct)
                .SingleOrDefaultAsync(p => p.Id == id);
            if (data == null) return NotFound();
            return View(new ProductUpdateVm
            {
                Title = data.Title,
                Description = data.Description,
                RateRange = data.RateRange,
                CategoryId = data.CategoryId,
                CostPrice = data.CostPrice,
                SellPrice = data.SellPrice,
                ImageUrl = data.ImageUrl,
                TagsId = data.TagProduct.Select(i => i.TagId).ToList(),

            });
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, ProductUpdateVm vm)
        {
            TempData["Update"] = false;
            if (id == null || id < 0) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Category = new SelectList(_db.Categorys, "Id", "Name");
                return View(vm);
            }
            if (vm.MainImage != null)
            {
                if (!vm.MainImage.IsCorrectType())
                {
                    ModelState.AddModelError("ImageFile", "Wrong file type");
                }
                if (!vm.MainImage.IsValidSize())
                {
                    ModelState.AddModelError("ImageFile", "Files length must be less than kb");
                }
            }

            if (vm.CostPrice > vm.SellPrice)
            {
                ModelState.AddModelError("CostPrice", "Sell price must be bigger than cost price");
            }
            
            if (!await _db.Categorys.AnyAsync(c => c.Id == vm.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Category doesnt exist");
                ViewBag.Category = new SelectList(_db.Categorys, "Id", "Name");
                return View(vm);
            }
            if (!vm.TagsId.Any())
            {
                ModelState.AddModelError("TagsId", "You must select at least 1 Tag");
            }

            var data = await _db.Products
                .Include(P => P.TagProduct)
                .SingleOrDefaultAsync(p => p.Id == id);
             data.Title = vm.Title;
            data.Description = vm.Description;
            data.RateRange = vm.RateRange;
            data.CategoryId = (int)vm.CategoryId;
            data.CostPrice = vm.CostPrice;
            data.SellPrice = vm.SellPrice;
            if (data == null) return NotFound();
            if (!string.IsNullOrEmpty(data.ImageUrl))
            {
                string filepath = Path.Combine(_env.WebRootPath, "Assets", "assets", "products", data.ImageUrl);
                if (System.IO.File.Exists(filepath))
                {
                    System.IO.File.Delete(filepath);
                }
            }
            string filename = Guid.NewGuid() + Path.GetExtension(vm.MainImage.FileName);
            using (Stream fs = new FileStream(Path.Combine(_env.WebRootPath, "Assets", "assets", "products", filename), FileMode.Create))
            {
                await vm.MainImage.CopyToAsync(fs);
            }
            data.ImageUrl = filename;
             if (!Enumerable.SequenceEqual(data.TagProduct.Select(p => p.TagId), vm.TagsId))
            {
                data.TagProduct = vm.TagsId.Select(c => new ProductTag { TagId = c, ProductId = data.Id }).ToList();
            }
            await _db.SaveChangesAsync();
            TempData["Update"] = true;
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteProduct(int? id)
        {

            if (id == null) return BadRequest();
            var data = await _db.Products.FindAsync(id);
            if (data == null) return NotFound();

            data.IsDeleted = true;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> RestoreProduct(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Products.FindAsync(id);
            if (data == null) return NotFound();
            data.IsDeleted = false;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> RestoreArchiveProduct(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Products.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = false;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteFromData(int? id)
        {
            TempData["Delete"] = false;
            if (id == null) return BadRequest();
            var data = await _db.Products.FindAsync(id);
            if (data == null) return NotFound();
            TempData["Delete"] = true;
            _db.Products.Remove(data);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Archived(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Products.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = true;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public JsonResult GetDistinctProductNames()
        {
            var distinctProductNames = _db.Products
                .Select(p => p.Title)
                .Distinct()
                .ToList();

            return Json(distinctProductNames);
        }
    }
}
