using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShoppingMvc.Contexts;
using ShoppingMvc.Helpers;
using ShoppingMvc.Models;
using ShoppingMvc.ViewModels.CommonVm;
using ShoppingMvc.ViewModels.ProductVm;

namespace ShoppingMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        EvaraDbContext _db { get; set; }
        IWebHostEnvironment _env { get; set; }

        public ProductController(EvaraDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> ProductPagination(int page = 1, int count = 8)
        {
            var items = await _db.Products
                .Skip((page - 1) * count).Take(count)
                .Include(p => p.AdditionalInfos)
                .Include(p => p.ProductImages)
                .Include(p => p.TagProducts)
                .Include(p => p.Category)
                .Select(p => p.FromProduct_ToProductListItemVm()).ToListAsync();

            int totalCount = await _db.Products.CountAsync();
            PaginationVm<IEnumerable<ProductListItemVm>> pag = new(totalCount, page, (int)Math.Ceiling((decimal)totalCount / count), items);
            return PartialView("ProductPagination2", pag);
        }

        public async Task<IActionResult> Index(string categoryFilter, DateTime? dateFilter, string statusFilter, int page = 1)
        {
            @ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name");

            int take = 4;
            int skip = (page - 1) * take;

            IQueryable<Product> baseQuery = _db.Products
                .Include(p => p.AdditionalInfos)
                .Include(p => p.ProductImages)
                .Include(p => p.TagProducts)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All categories")
            {
                baseQuery = baseQuery.Where(p => p.Category.Name == categoryFilter);
            }

            if (dateFilter.HasValue)
            {
                DateTime filterDate = dateFilter.Value.Date;
                baseQuery = baseQuery.Where(p => p.CreatedTime.Date == filterDate);
            }

            baseQuery = baseQuery.OrderBy(p => p.CreatedTime);

            var products = await baseQuery.ToListAsync();

            var productViewModels = products.Select(p => p.FromProduct_ToProductListItemVm()).ToList();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All statuses" && statusFilter != "Show all")
            {
                productViewModels = productViewModels.Where(p =>
                    (p.IsDeleted && statusFilter == "Disabled") ||
                    (!p.IsDeleted && !p.IsArchived && statusFilter == "Active") ||
                    (p.IsArchived && statusFilter == "Archived")
                ).ToList();
            }

            int total = productViewModels.Count;

            var paginatedData = productViewModels.Skip(skip).Take(take).ToList();

            PaginationVm<IEnumerable<ProductListItemVm>> pagination = new PaginationVm<IEnumerable<ProductListItemVm>>(total, page, (int)Math.Ceiling((decimal)total / take), paginatedData);

            if (paginatedData.Count == 0)
            {
                ViewBag.Message = "Nothing Found";
            }

            return View(pagination);
        }


        public async Task<IActionResult> Create()
        {
            ViewBag.Tags = new SelectList(_db.Tags, "Id", "Title");
            ViewBag.Category = new SelectList(_db.Categories, "Id", "Name");
            ViewBag.AdditionalInfos = new List<AdditionalInfo>();

            if (!ModelState.IsValid)
            {
                ViewBag.Category = new SelectList(_db.Categories, "Id", "Name");
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
                ViewBag.Category = new SelectList(_db.Categories, "Id", "Name");
                return View(vm);

            }
            string filename = null;

            List<ProductImage> productImages = new List<ProductImage>();

            if (vm.ProductImages != null)
            {
                foreach (var image in vm.ProductImages)
                {
                    if (!image.IsCorrectType())
                    {
                        ModelState.AddModelError("MainImage", "Wrong fie type");
                    }
                    if (!image.IsValidSize(200))
                    {
                        ModelState.AddModelError("MainImage", "Image must less than given kb");
                    }
                    filename = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    using (Stream fs = new FileStream(Path.Combine(_env.WebRootPath, "Assets", "assets", "products", filename), FileMode.Create))
                    {
                        await image.CopyToAsync(fs);
                        ProductImage pi = new ProductImage()
                        {
                            ImageName = filename,
                            ImageUrl = Path.Combine("Assets", "assets", "products", filename),
                            IsPrimary = false
                        };

                        productImages.Add(pi);
                    }
                }

            }
            if (!await _db.Categories.AnyAsync(c => c.Id == vm.CategoryId))
            {
                ViewBag.Tags = new SelectList(_db.Tags, "Id", "Title");
                ModelState.AddModelError("CategoryId", "Category doesnt exist");
                ViewBag.Category = new SelectList(_db.Categories, "Id", "Name");
                return View(vm);
            }

            if (await _db.Tags.Where(c => vm.TagsId.Contains(c.Id)).Select(c => c.Id).CountAsync() != vm.TagsId.Count())
            {
                ViewBag.Category = new SelectList(_db.Categories, "Id", "Name");
                ModelState.AddModelError("TagsId", "TagsId doesnt exist");
                ViewBag.Tags = new SelectList(_db.Tags, "Id", "Title");
                return View(vm);
            }

            Product product = new Product()
            {
                Title = vm.Title,
                Description = vm.Description,
                Price = vm.Price,
                CategoryId = (int)vm.CategoryId,
                TagProducts = vm.TagsId.Select(id => new ProductTag
                {
                    TagId = id,
                }).ToList(),
                DiscountRate = vm.DiscountRate,
                StockNumber = vm.StockNumber,
                Color = vm.Color,
                Size = vm.Size,
            };

            EntityEntry<Product> result = await _db.Products.AddAsync(product);

            if (result.State != EntityState.Added)
                return BadRequest(result);

            Product addedProduct = result.Entity;

            await _db.SaveChangesAsync();

            List<AdditionalInfo>? additionalInfos = vm?.AdditionalInfos;

            additionalInfos?.ForEach(ai => ai.ProductId = addedProduct.Id);
            additionalInfos?.ForEach(ai => ai.Product = addedProduct);

            if (additionalInfos != null)
                await _db.AdditionalInfos.AddRangeAsync(additionalInfos);


            productImages.ForEach(pi => pi.ProductId = addedProduct.Id);
            productImages.ForEach(pi => pi.Product = addedProduct);


            if (addedProduct.ProductImages == null || !addedProduct.ProductImages.Any(pi => pi.IsPrimary))
                productImages.First().IsPrimary = true;

            await _db.ProductImages.AddRangeAsync(productImages);

            await _db.SaveChangesAsync();

            TempData["Create"] = true;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();
            ViewBag.Category = new SelectList(_db.Categories, "Id", "Name");
            ViewBag.Tags = new SelectList(_db.Tags, "Id", "Title");
            var data = await _db.Products
                .Include(p => p.TagProducts)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.AdditionalInfos)
                .SingleOrDefaultAsync(p => p.Id == id);


            if (data == null) return NotFound();
            return View(data.FromProduct_ToProductUpdateVm());

        }


        [HttpPost]
        public async Task<IActionResult> SetPrimaryImage(int ProductId, int ImageId)
        {
            Product? product = await _db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == ProductId);

            if (product == null) return NotFound();

            List<ProductImage>? productImages = product.ProductImages;

            ProductImage? productImage = productImages?.FirstOrDefault(pi => pi.Id == ImageId);

            if (productImage == null) return NotFound();

            productImages?.ForEach(pi =>
            {
                if (pi.Id == productImage.Id)
                    pi.IsPrimary = true;
                else pi.IsPrimary = false;
            });

            _db.ProductImages.UpdateRange(productImages);

            int result = await _db.SaveChangesAsync();

            return result > 0 ? Ok() : BadRequest();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteImage(int ProductId, int ImageId)
        {
            Product? product = await _db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == ProductId);

            if (product == null) return NotFound();

            List<ProductImage>? productImages = product.ProductImages;

            ProductImage? productImage = productImages?.FirstOrDefault(pi => pi.Id == ImageId);

            if (productImage == null) return NotFound();

            string filename = productImage.ImageName;

            string filepath = Path.Combine(_env.WebRootPath, "Assets", "assets", "products", filename);

            if (System.IO.File.Exists(filepath))
            {
                System.IO.File.Delete(filepath);
            }

            _db.ProductImages.Remove(productImage);

            if (productImage.IsPrimary && _db.ProductImages.ToList().Count > 0)
            {
                _db.ProductImages.ToList().First().IsPrimary = true;
            }

            int result = await _db.SaveChangesAsync();

            return result > 0 ? Ok() : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, ProductUpdateVm vm)
        {
            TempData["Update"] = false;
            Product? product = await _db.Products.FirstOrDefaultAsync(p => p.Id == vm.Id);

            if (product == null) return NotFound();

            if (id == null || id < 0) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Category = new SelectList(_db.Categories, "Id", "Name");
                return View(vm);
            }

            List<ProductImage> productImages = new List<ProductImage>();

            if (vm.ProductImages != null)
            {
                foreach (var image in vm.ProductImages)
                {
                    if (image != null)
                    {
                        if (!image.IsCorrectType())
                        {
                            ModelState.AddModelError("ImageFile", "Wrong file type");
                        }
                        else if (!image.IsValidSize())
                        {
                            ModelState.AddModelError("ImageFile", "Files length must be less than kb");
                        }

                        string filename = Guid.NewGuid() + Path.GetExtension(image.FileName);

                        using (Stream fs = new FileStream(Path.Combine(_env.WebRootPath, "Assets", "assets", "products", filename), FileMode.Create))
                        {
                            await image.CopyToAsync(fs);
                            ProductImage pi = new ProductImage()
                            {
                                ImageName = filename,
                                ImageUrl = Path.Combine("Assets", "assets", "products", filename),
                                IsPrimary = false
                            };

                            productImages.Add(pi);
                        }

                        if (productImages.Count > 0)
                        {
                            productImages.ForEach(pi => pi.ProductId = product.Id);
                            productImages.ForEach(pi => pi.Product = product);


                            if (vm.ProductImagesData == null || !vm.ProductImagesData.Any(pi => pi.IsPrimary))
                                productImages.First().IsPrimary = true;

                            await _db.ProductImages.AddRangeAsync(productImages);
                        }
                    }
                }
            }

            if (!await _db.Categories.AnyAsync(c => c.Id == vm.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Category doesnt exist");
                ViewBag.Category = new SelectList(_db.Categories, "Id", "Name");
                return View(vm);
            }
            if (!vm.TagsId.Any())
            {
                ModelState.AddModelError("TagsId", "You must select at least 1 Tag");
            }

            var data = await _db.Products
                .Include(P => P.TagProducts)
                .SingleOrDefaultAsync(p => p.Id == id);

            data.Title = vm.Title;
            data.Description = vm.Description;
            data.DiscountRate = vm.DiscountRate;
            data.CategoryId = (int)vm.CategoryId;
            data.Price = vm.Price;
            data.Color = vm.Color;
            data.Size = vm.Size;
            data.AdditionalInfos = vm.AdditionalInfos;


            if (data == null) return NotFound();
            if (!Enumerable.SequenceEqual(data.TagProducts.Select(p => p.TagId), vm.TagsId))
            {
                data.TagProducts = vm.TagsId.Select(c => new ProductTag { TagId = c, ProductId = data.Id }).ToList();
            }

            List<AdditionalInfo>? additionalInfos = vm?.AdditionalInfos;

            additionalInfos?.ForEach(ai => ai.ProductId = product.Id);
            additionalInfos?.ForEach(ai => ai.Product = product);

            if (additionalInfos != null)
                await _db.AdditionalInfos.AddRangeAsync(additionalInfos);


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
            var data = await _db.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);
            if (data == null) return NotFound();
            TempData["Delete"] = true;

            List<ProductImage>? productImages = data.ProductImages;

            if(productImages?.Count > 0)
            {
                foreach (var productImage in productImages)
                {
                    if (productImage == null) return NotFound();

                    string filename = productImage.ImageName;

                    string filepath = Path.Combine(_env.WebRootPath, "Assets", "assets", "products", filename);

                    if (System.IO.File.Exists(filepath))
                    {
                        System.IO.File.Delete(filepath);
                    }

                    _db.ProductImages.Remove(productImage);
                }
            }

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
