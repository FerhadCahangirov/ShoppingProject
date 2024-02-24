using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using ShoppingMvc.Contexts;
using ShoppingMvc.Models;
using ShoppingMvc.Models.Identity;
using ShoppingMvc.ViewModels.CommonVm;
using ShoppingMvc.ViewModels.ProductVm;
using ShoppingMvc.ViewModels.SellerVm;

namespace ShoppingMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Moderator")]
    public class SellerController : Controller
    {
        private readonly EvaraDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly int _take = 8;

        public SellerController(EvaraDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> SellerPagination(DateTime? dateFilter, string? statusFilter, string? searchFilter, int page = 1, int size = 8)
        {
            int take = _take;
            int skip = (page - 1) * take;

            var query = _db.SellerDatas.Include(sd => sd.Seller).Select(sd => new SellerListItemVm()
            {
                SellerData = sd,
                User = sd.Seller
            });

            if (dateFilter.HasValue)
            {
                DateTime filterDate = dateFilter.Value.Date;

                query = query.Where(c => c.SellerData.CreatedTime == filterDate);
            }

            query = query.OrderBy(c => c.SellerData.CreatedTime);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                statusFilter = statusFilter.ToLower();
                if (statusFilter != "all statuses" && statusFilter != "show all")
                {
                    query = query.Where(x => (x.SellerData.IsApproved == true && statusFilter == "approved") || (x.SellerData.IsApproved != true && statusFilter == "not approved"));
                }
            }

            if (!string.IsNullOrEmpty(searchFilter))
            {
                searchFilter = searchFilter.ToLower();
                query = query.Where(x => x.User.UserName.ToLower().Contains(searchFilter) || x.User.Email.ToLower().Contains(searchFilter));
            }


            var filteredData = await query.ToListAsync();
            int total = filteredData.Count();
            var paginatedData = filteredData.Skip(skip).Take(take).ToList();
            PaginationVm<IEnumerable<SellerListItemVm>> pagination = new(total, page, (int)Math.Ceiling((decimal)total / take), filteredData);

            if (paginatedData.Count == 0)
            {
                ViewBag.Message = "Nothing Found";
            }

            return PartialView("SellerPagination", pagination);
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int take = _take;
            int skip = (page - 1) * take;

            var query = _db.SellerDatas.Include(sd => sd.Seller).Select(sd => new SellerListItemVm()
            {
                SellerData = sd,
                User = sd.Seller
            });

            query = query.OrderBy(c => c.SellerData.CreatedTime);

            var filteredData = await query.ToListAsync();

            int total = filteredData.Count();
            var paginatedData = filteredData.Skip(skip).Take(take).ToList();

            PaginationVm<IEnumerable<SellerListItemVm>> pagination = new(total, page, (int)Math.Ceiling((decimal)total / take), filteredData);

            if (paginatedData.Count == 0)
            {
                ViewBag.Message = "Nothing Found";
            }

            return View(pagination);
        }

        public async Task<IActionResult> SellerProductsPagination([FromRoute] int id, int page, int size)
        {
            SellerData? sellerData = await _db.SellerDatas.Include(s => s.Seller).FirstOrDefaultAsync(s => s.Id == id);

            if (sellerData == null) return NotFound();

            List<ProductListItemVm>? products = await _db.Products
                .Include(p => p.ProductImages)
                .Include(p => p.SellerData)
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .Where(p => p.SellerData == sellerData)
                .Skip((page - 1) * size).Take(size)
                .Select(p => p.FromProduct_ToProductListItemVm())
                .ToListAsync();

            int total = products.Count;

            PaginationVm<IEnumerable<ProductListItemVm>> pagination = new(total, page, (int)Math.Ceiling((decimal)total / size), products);

            SellerListItemVm sellerListItemVm = new SellerListItemVm()
            {
                User = sellerData.Seller,
                SellerData = sellerData,
                Products = pagination
            };

            return View(viewName: "SellerDetails", model: sellerListItemVm);
        }

        public async Task<IActionResult> SellerDetails(int id, int page = 1, int size = 12)
        {
            SellerData? sellerData = await _db.SellerDatas.Include(s => s.Seller).FirstOrDefaultAsync(s => s.Id == id);

            if (sellerData == null) return NotFound();

            List<ProductListItemVm>? products = await _db.Products
                .Include(p => p.ProductImages)
                .Include(p => p.SellerData)
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .Where(p => p.SellerData == sellerData)
                .Skip((page - 1) * size).Take(size)
                .Select(p => p.FromProduct_ToProductListItemVm())
                .ToListAsync();

            int total = products.Count();


            PaginationVm<IEnumerable<ProductListItemVm>> pagination = new(total, page, (int)Math.Ceiling((decimal)total / size), products);

            SellerListItemVm sellerListItemVm = new SellerListItemVm()
            {
                User = sellerData.Seller,
                SellerData = sellerData,
                Products = pagination
            };

            return View(viewName: "SellerDetails", model: sellerListItemVm);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveSeller(int id)
        {
            SellerData? sellerData = await _db.SellerDatas.Include(s => s.Seller).Include(s => s.Products).FirstOrDefaultAsync(s => s.Id == id);

            if (sellerData == null) return NotFound();

            AppUser user = sellerData.Seller;

            IdentityResult result = await _userManager.AddToRoleAsync(user, "Seller");
            if (result.Succeeded != true) return BadRequest(result);

            sellerData.IsApproved = true;

            await _db.SaveChangesAsync();

            TempData["SellerApproved"] = true;
            return RedirectToAction("SellerDetails", id);
        }


        public async Task<IActionResult> DeleteFromData(int? id)
        {
            TempData["Delete"] = false;
            if (id == null) return BadRequest();
            var data = await _db.SellerDatas.FirstOrDefaultAsync(p => p.Id == id);
            if (data == null) return NotFound();
            TempData["Delete"] = true;
            _db.SellerDatas.Remove(data);
            await _db.SaveChangesAsync();
            return Redirect("/Admin/Seller/Index"); ;
        }
    }
}
