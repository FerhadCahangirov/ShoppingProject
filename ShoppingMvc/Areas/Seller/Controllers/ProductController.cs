using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShoppingMvc.Contexts;
using ShoppingMvc.Enums;
using ShoppingMvc.Helpers;
using ShoppingMvc.Models;
using ShoppingMvc.ViewModels.CommentVm;
using ShoppingMvc.ViewModels.CommonVm;
using ShoppingMvc.ViewModels.ProductVm;

namespace ShoppingMvc.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Policy = "SellerPolicy")]
    public class ProductController : Controller
    {
        EvaraDbContext _db { get; set; }
        IWebHostEnvironment _env { get; set; }

        public ProductController(EvaraDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private async Task<SellerData> GetActiveSellerDataAsync() => await _db.SellerDatas.Include(s => s.Seller).FirstOrDefaultAsync(x => x.Seller.UserName == HttpContext.User.Identity.Name);

        public async Task<IActionResult> ProductsPartial(string? searchFilter, string? categoryFilter, DateTime? dateFilter, string? statusFilter, int page = 1, int size = 4)
        {
            int take = size;
            int skip = (page - 1) * take;

            SellerData? sellerData = await GetActiveSellerDataAsync();

            var baseQuery = _db.Products
                .Include(p => p.AdditionalInfos)
                .Include(p => p.ProductImages)
                .Include(p => p.Tags)
                .Include(p => p.Category)
                .Include(p => p.SellerData)
                .Include(p => p.Comments)
                .Where(p => p.SellerData == sellerData);

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                categoryFilter = categoryFilter.ToLower();
                if (categoryFilter != "show all" && _db.Categories.Any(c => c.Name.ToLower() == categoryFilter))
                    baseQuery = baseQuery.Where(p => p.Category.Name.ToLower() == categoryFilter);
            }

            if (dateFilter.HasValue)
            {
                DateTime filterDate = dateFilter.Value.Date;
                baseQuery = baseQuery.Where(p => p.CreatedTime.Date == filterDate);
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                statusFilter = statusFilter.ToLower();
                if (statusFilter != "all statuses" && statusFilter != "show all")
                {
                    baseQuery = baseQuery.Where(p =>
                        (p.IsDeleted && statusFilter == "disabled") ||
                        (!p.IsDeleted && !p.IsArchived && statusFilter == "active") ||
                        (p.IsArchived && statusFilter == "archived")
                    );
                }
            }

            if (!string.IsNullOrEmpty(searchFilter))
            {
                searchFilter = searchFilter.ToLower();
                baseQuery = baseQuery.Where(p => p.Title.ToLower().Contains(searchFilter) == true || p.Description.ToLower().Contains(searchFilter) == true);
            }

            baseQuery = baseQuery.OrderBy(p => p.CreatedTime);

            var products = await baseQuery.ToListAsync();

            var productViewModels = products.Select(p => p.FromProduct_ToProductListItemVm()).ToList();

            int total = await _db.Products
                .Include(p => p.SellerData)
                .Where(p => p.SellerData == sellerData)
                .CountAsync();

            var paginatedData = productViewModels.Skip(skip).Take(take).ToList();

            PaginationVm<IEnumerable<ProductListItemVm>> pagination = new PaginationVm<IEnumerable<ProductListItemVm>>(total, page, (int)Math.Ceiling((decimal)total / take), paginatedData, size);

            if (paginatedData.Count == 0)
            {
                ViewBag.Message = "Nothing Found";
            }

            return PartialView("ProductsPartial", pagination);
        }


        public async Task<IActionResult> Index()
        {
            SellerData? sellerData = await GetActiveSellerDataAsync();

            @ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name");

            int page = 1;
            int take = 4;
            int skip = (page - 1) * take;

            IQueryable<Product> baseQuery = _db.Products
                .Include(p => p.AdditionalInfos)
                .Include(p => p.ProductImages)
                .Include(p => p.Tags)
                .Include(p => p.Category)
                .Include(p => p.SellerData)
                .Include(p => p.Comments)
                .Where(p => p.SellerData == sellerData)
                .AsQueryable();

            baseQuery = baseQuery.OrderBy(p => p.CreatedTime);

            var products = await baseQuery.ToListAsync();

            var productViewModels = products.Select(p => p.FromProduct_ToProductListItemVm()).ToList();

            int total = await _db.Products
                .Include(p => p.SellerData)
                .Where(p => p.SellerData == sellerData)
                .CountAsync();

            var paginatedData = productViewModels.Skip(skip).Take(take).ToList();

            PaginationVm<IEnumerable<ProductListItemVm>> pagination = new PaginationVm<IEnumerable<ProductListItemVm>>(total, page, (int)Math.Ceiling((decimal)total / take), paginatedData, take);

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

            var conditionValues = Enum.GetValues(typeof(ProductConditionEnum));

            var selectListItems = new List<SelectListItem>();

            foreach (var value in conditionValues)
            {
                selectListItems.Add(new SelectListItem
                {
                    Text = Enum.GetName(typeof(ProductConditionEnum), value),
                    Value = ((int)value).ToString()
                });
            }

            ViewBag.Condition = new SelectList(selectListItems, "Value", "Text");

            ViewBag.AdditionalInfos = new List<AdditionalInfo>();

            if (!ModelState.IsValid)
            {
                ViewBag.Category = new SelectList(_db.Categories, "Id", "Name");
                return View();
            }
            return View();

        }

        public IActionResult Cancel()
        {
            return Redirect("/Seller/Product/Index");
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateVm vm)
        {
            var username = HttpContext.User.Identity.Name;

            SellerData? sellerData = await _db.SellerDatas.Include(s => s.Seller).FirstOrDefaultAsync(x => x.Seller.UserName == username);

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
                Category = _db.Categories.Find(vm.CategoryId),
                Tags = vm.TagsId.Select(id => _db.Tags.Find(id)).ToList(),
                DiscountRate = vm.DiscountRate,
                StockNumber = vm.StockNumber,
                Color = vm.Color,
                Weight = vm.Weight,
                Width = vm.Width,
                Height = vm.Height,
                ShippingFee = vm.ShippingFee,
                Condition = vm.Condition,
                SellerData = sellerData
            };

            EntityEntry<Product> result = await _db.Products.AddAsync(product);

            if (result.State != EntityState.Added)
                return BadRequest(result);

            Product addedProduct = result.Entity;

            await _db.SaveChangesAsync();

            List<AdditionalInfo>? additionalInfos = vm?.AdditionalInfos;

            additionalInfos?.ForEach(ai => ai.Product = addedProduct);

            if (additionalInfos != null)
                await _db.AdditionalInfos.AddRangeAsync(additionalInfos);


            productImages.ForEach(pi => pi.Product = addedProduct);


            if (addedProduct.ProductImages == null || !addedProduct.ProductImages.Any(pi => pi.IsPrimary))
                productImages.First().IsPrimary = true;

            await _db.ProductImages.AddRangeAsync(productImages);

            await _db.SaveChangesAsync();

            TempData["Create"] = true;
            return Redirect("/Seller/Product/Index");
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();
            ViewBag.Category = new SelectList(_db.Categories, "Id", "Name");
            ViewBag.Tags = new SelectList(_db.Tags, "Id", "Title");
            var data = await _db.Products
                .Include(p => p.Tags)
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

            List<ProductImage>? productImages = product.ProductImages?.ToList();

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

            List<ProductImage>? productImages = product.ProductImages?.ToList();

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
                .Include(P => P.Tags)
                .SingleOrDefaultAsync(p => p.Id == id);

            data.Title = vm.Title;
            data.Description = vm.Description;
            data.DiscountRate = vm.DiscountRate;
            data.Category = _db.Categories.Find(vm.CategoryId);
            data.Price = vm.Price;
            data.Color = vm.Color;
            data.Weight = vm.Weight;
            data.Width = vm.Width;
            data.Height = vm.Height;
            data.ShippingFee = vm.ShippingFee;
            data.Condition = vm.Condition;
            data.AdditionalInfos = vm.AdditionalInfos;


            if (data == null) return NotFound();
            if (!Enumerable.SequenceEqual(data.Tags, vm.TagsId.Select(id => _db.Tags.Find(id))))
            {
                data.Tags = vm.TagsId.Select(id => _db.Tags.Find(id)).ToList();
            }

            List<AdditionalInfo>? additionalInfos = vm?.AdditionalInfos;

            additionalInfos?.ForEach(ai => ai.Product = product);

            if (additionalInfos != null)
                await _db.AdditionalInfos.AddRangeAsync(additionalInfos);


            await _db.SaveChangesAsync();
            TempData["Update"] = true;
            return Redirect("/Seller/Product/Index"); ;
        }

        [HttpPost]
        public IActionResult ProductCommentsPartial([FromRoute] int id, string? userFilter, string? contentFilter, DateTime? dateFilter, string? statusFilter, int page = 1, int size = 4)
        {
            ViewBag.UserSearch = userFilter;
            ViewBag.ContentSearch = contentFilter;
            ViewBag.DateFilter = dateFilter;
            ViewBag.StatusFilter = statusFilter;

            var query = _db.Comments
                .Include(c => c.User)
                .Include(c => c.Product)
                .Include(c => c.Replies)
                .ThenInclude(r => r.User)
                .Where(c => c.Product.Id == id)
                .Select(c => new CommentRepliesVm()
                {
                    Comment = c,
                    Replies = c.Replies
                }).AsQueryable();

            if (!string.IsNullOrEmpty(userFilter))
            {
                userFilter = userFilter.ToLower();

                var newQuery = new List<CommentRepliesVm>();

                List<CommentRepliesVm> queryList = query.ToList();

                foreach (CommentRepliesVm q in queryList)
                {
                    if (q.Replies != null && q.Replies.Count > 0)
                    {
                        newQuery.Add(new CommentRepliesVm()
                        {
                            Comment = q.Comment,
                            Replies = q.Replies.Where(r =>
                                r.User.UserName.ToLower().Contains(userFilter) == true ||
                                r.User.Email.ToLower().Contains(userFilter) == true
                            ).ToList(),
                        });
                    }
                    else
                    {
                        newQuery.Add(new CommentRepliesVm()
                        {
                            Comment = q.Comment,
                            Replies = null
                        });

                    }
                }

                query = newQuery.AsQueryable();

                query = query
                .Where(c =>
                    c.Comment.User.UserName.ToLower().Contains(userFilter) ||
                    c.Comment.User.Email.ToLower().Contains(userFilter) ||
                    (c.Replies != null && c.Replies.Count > 0)
                );
            }


            if (dateFilter.HasValue)
            {
                DateTime filterDate = dateFilter.Value.Date;

                var newQuery = new List<CommentRepliesVm>();

                List<CommentRepliesVm> queryList = query.ToList();

                foreach (CommentRepliesVm q in queryList)
                {
                    if (q.Replies != null && q.Replies.Count > 0)
                    {
                        newQuery.Add(new CommentRepliesVm()
                        {
                            Comment = q.Comment,
                            Replies = q.Replies.Where(r =>
                                r.CreatedTime.Date == filterDate
                            ).ToList(),
                        });
                    }
                    else
                    {
                        newQuery.Add(new CommentRepliesVm()
                        {
                            Comment = q.Comment,
                            Replies = null
                        });

                    }
                }

                query = newQuery.AsQueryable();

                query = query
                .Where(c =>
                    c.Comment.CreatedTime.Date == filterDate ||
                    (c.Replies != null && c.Replies.Count > 0)
                );
            }

            if (!string.IsNullOrEmpty(contentFilter))
            {
                contentFilter = contentFilter.ToLower();

                var newQuery = new List<CommentRepliesVm>();

                List<CommentRepliesVm> queryList = query.ToList();

                foreach (CommentRepliesVm q in queryList)
                {
                    if (q.Replies != null && q.Replies.Count > 0)
                    {
                        newQuery.Add(new CommentRepliesVm()
                        {
                            Comment = q.Comment,
                            Replies = q.Replies.Where(r =>
                                r.Message.ToLower().Contains(contentFilter)
                            ).ToList(),
                        });
                    }
                    else
                    {
                        newQuery.Add(new CommentRepliesVm()
                        {
                            Comment = q.Comment,
                            Replies = null
                        });

                    }
                }

                query = newQuery.AsQueryable();
                query = query
                .Where(c =>
                    c.Comment.Message.ToLower().Contains(contentFilter) ||
                    (c.Replies != null && c.Replies.Count > 0)
                );
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                statusFilter = statusFilter.ToLower();
                if (statusFilter != "all statuses" && statusFilter != "show all")
                {
                    var newQuery = new List<CommentRepliesVm>();

                    List<CommentRepliesVm> queryList = query.ToList();

                    foreach (CommentRepliesVm q in queryList)
                    {
                        if (q.Replies != null && q.Replies.Count > 0)
                        {
                            newQuery.Add(new CommentRepliesVm()
                            {
                                Comment = q.Comment,
                                Replies = q.Replies.Where(r =>
                                    (r.IsDeleted && statusFilter == "disabled") ||
                                    (!r.IsDeleted && !r.IsArchived && statusFilter == "active") ||
                                    (r.IsArchived && statusFilter == "archived")
                                ).ToList(),
                            });
                        }
                        else
                        {
                            newQuery.Add(new CommentRepliesVm()
                            {
                                Comment = q.Comment,
                                Replies = null
                            });
                        }
                    };

                    query = newQuery.AsQueryable();

                    query = query.Where(c =>
                        (c.Comment.IsDeleted && statusFilter == "disabled") ||
                        (!c.Comment.IsDeleted && !c.Comment.IsArchived && statusFilter == "active") ||
                        (c.Comment.IsArchived && statusFilter == "archived") ||
                        (c.Replies != null && c.Replies.Count > 0)
                    );
                }
            }

            IEnumerable<CommentRepliesVm> data = query
                .Skip((page - 1) * size).Take(size)
                .ToList();

            int total = query.Count();

            PaginationVm<IEnumerable<CommentRepliesVm>> pagination = new(total, page, (int)Math.Ceiling((decimal)total / size), data, size);

            return PartialView("ProductCommentsPartial", pagination);
        }


        public async Task<IActionResult> ProductComments(int id)
        {
            int page = 1;
            int size = 4;

            List<CommentRepliesVm>? comments = await _db.Comments
                .Include(c => c.User)
                .Include(c => c.Product)
                .Include(c => c.Replies)
                .ThenInclude(r => r.User)
                .Skip((page - 1) * size).Take(size)
                .Where(c => c.Product.Id == id)
                .Select(c => new CommentRepliesVm()
                {
                    Comment = c,
                    Replies = c.Replies
                })
                .ToListAsync();

            int total = _db.Comments.Where(c => c.Product.Id == id).Count();

            PaginationVm<IEnumerable<CommentRepliesVm>> pagination = new(total, page, (int)Math.Ceiling((decimal)total / size), comments, size);

            return View(pagination);
        }

        #region Comment Actions

        public async Task<IActionResult> DeleteComment(int? id, int productId)
        {

            if (id == null) return BadRequest();
            var data = await _db.Comments.FindAsync(id);
            if (data == null) return NotFound();

            data.IsDeleted = true;
            await _db.SaveChangesAsync();
            return Redirect($"/Seller/Product/ProductComments/{productId}");
        }
        public async Task<IActionResult> RestoreComment(int? id, int productId)
        {
            if (id == null) return BadRequest();
            var data = await _db.Comments.FindAsync(id);
            if (data == null) return NotFound();
            data.IsDeleted = false;
            await _db.SaveChangesAsync();
            return Redirect($"/Seller/Product/ProductComments/{productId}");
        }

        public async Task<IActionResult> RestoreArchiveComment(int? id, int productId)
        {
            if (id == null) return BadRequest();
            var data = await _db.Comments.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = false;
            await _db.SaveChangesAsync();
            return Redirect($"/Seller/Product/ProductComments/{productId}");

        }
        public async Task<IActionResult> DeleteFromDataComment(int? id, int productId)
        {
            TempData["Delete"] = false;
            if (id == null) return BadRequest();
            var data = await _db.Comments.FirstOrDefaultAsync(p => p.Id == id);
            if (data == null) return NotFound();
            TempData["Delete"] = true;
            _db.Comments.Remove(data);

            await _db.SaveChangesAsync();
            return Redirect($"/Seller/Product/ProductComments/{productId}");

        }
        public async Task<IActionResult> ArchivedComment(int? id, int productId)
        {
            if (id == null) return BadRequest();
            var data = await _db.Comments.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = true;
            await _db.SaveChangesAsync();
            return Redirect($"/Seller/Product/ProductComments/{productId}");
        }

        #endregion

        #region Reply Actions

        public async Task<IActionResult> DeleteReply(int? id, int productId)
        {

            if (id == null) return BadRequest();
            var data = await _db.Replys.FindAsync(id);
            if (data == null) return NotFound();

            data.IsDeleted = true;
            await _db.SaveChangesAsync();
            return Redirect($"/Seller/Product/ProductComments/{productId}");
        }
        public async Task<IActionResult> RestoreReply(int? id, int productId)
        {
            if (id == null) return BadRequest();
            var data = await _db.Replys.FindAsync(id);
            if (data == null) return NotFound();
            data.IsDeleted = false;
            await _db.SaveChangesAsync();
            return Redirect($"/Seller/Product/ProductComments/{productId}");
        }

        public async Task<IActionResult> RestoreArchiveReply(int? id, int productId)
        {
            if (id == null) return BadRequest();
            var data = await _db.Replys.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = false;
            await _db.SaveChangesAsync();
            return Redirect($"/Seller/Product/ProductComments/{productId}");

        }
        public async Task<IActionResult> DeleteFromDataReplies(int? id, int productId)
        {
            TempData["Delete"] = false;
            if (id == null) return BadRequest();
            var data = await _db.Replys.FirstOrDefaultAsync(p => p.Id == id);
            if (data == null) return NotFound();
            TempData["Delete"] = true;
            _db.Replys.Remove(data);

            await _db.SaveChangesAsync();
            return Redirect($"/Seller/Product/ProductComments/{productId}");

        }
        public async Task<IActionResult> ArchivedReply(int? id, int productId)
        {
            if (id == null) return BadRequest();
            var data = await _db.Replys.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = true;
            await _db.SaveChangesAsync();
            return Redirect($"/Seller/Product/ProductComments/{productId}");
        }

        #endregion

        public async Task<IActionResult> DeleteProduct(int? id)
        {

            if (id == null) return BadRequest();
            var data = await _db.Products.FindAsync(id);
            if (data == null) return NotFound();

            data.IsDeleted = true;
            await _db.SaveChangesAsync();
            return Redirect("/Seller/Product/Index"); ;
        }
        public async Task<IActionResult> RestoreProduct(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Products.FindAsync(id);
            if (data == null) return NotFound();
            data.IsDeleted = false;
            await _db.SaveChangesAsync();
            return Redirect("/Seller/Product/Index"); ;
        }
        public async Task<IActionResult> RestoreArchiveProduct(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Products.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = false;
            await _db.SaveChangesAsync();
            return Redirect("/Seller/Product/Index"); ;
        }
        public async Task<IActionResult> DeleteFromData(int? id)
        {
            TempData["Delete"] = false;
            if (id == null) return BadRequest();
            var data = await _db.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);
            if (data == null) return NotFound();
            TempData["Delete"] = true;

            List<ProductImage>? productImages = data.ProductImages?.ToList();

            if (productImages?.Count > 0)
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
            return Redirect("/Seller/Product/Index"); ;
        }
        public async Task<IActionResult> Archived(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Products.FindAsync(id);
            if (data == null) return NotFound();
            data.IsArchived = true;
            await _db.SaveChangesAsync();
            return Redirect("/Seller/Product/Index"); ;
        }

    }
}
