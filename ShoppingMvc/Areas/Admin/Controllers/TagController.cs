using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.Models.Cards;
using ShoppingMvc.Models.Tags;
using ShoppingMvc.ViewModels.CommonVm;
using ShoppingMvc.ViewModels.ProductVm;
using ShoppingMvc.ViewModels.TagVm;

namespace ShoppingMvc.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TagController : Controller
	{
		EvaraDbContext _db { get; set; }
		IWebHostEnvironment _env { get; set; }

		public TagController(EvaraDbContext db, IWebHostEnvironment env)
		{
			_db = db;
			_env = env;
		}

		public async Task<IActionResult> ProductPagination(int page = 1, int count = 8)
		{
			var items = await _db.Tags.Skip((page - 1) * count).Take(count).Select(c => new TagListItemVm
			{
				Id = c.Id,
				CreatedTime = c.CreatedTime,
				UpdatedTime = c.UpdatedTime,
				IsDeleted = c.IsDeleted,
				IsArchived = c.IsArchived,
				Title = c.Title,

			}).ToListAsync();
			int totalCount = await _db.Tags.CountAsync();
			PaginationVm<IEnumerable<TagListItemVm>> pag = new(totalCount, page, (int)Math.Ceiling((decimal)totalCount / count), items);
			return PartialView("_TagPaginationPartial", pag);
		}
		public async Task<IActionResult> Index(string categoryFilter, DateTime? dateFilter, string statusFilter, int page = 1)
		{
			int take = 4;
			int skip = (page - 1) * take;

			var query = _db.Tags.Select(c => new TagListItemVm
			{
				Id = c.Id,
				CreatedTime = c.CreatedTime,
				UpdatedTime = c.UpdatedTime,
				IsDeleted = c.IsDeleted,
				IsArchived = c.IsArchived,
				Title = c.Title,
			});

			// Apply category filter
			if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All categories")
			{
				query = query.Where(c => c.Title == categoryFilter);
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

			PaginationVm<IEnumerable<TagListItemVm>> pag = new PaginationVm<IEnumerable<TagListItemVm>>(total, page, (int)Math.Ceiling((decimal)total / take), paginatedData);

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
		public async Task<IActionResult> Create(TagCreateVm vm)
		{
			
			Tag tag = new Tag()
			{
				Title = vm.Title,
			};
			_db.Tags.AddAsync(tag);
			await _db.SaveChangesAsync();
			TempData["Create"] = true;
			return RedirectToAction(nameof(Index));
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
