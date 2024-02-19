using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.Helpers;
using ShoppingMvc.Models;
using ShoppingMvc.ViewModels.CommonVm;
using ShoppingMvc.ViewModels.SliderVm;

namespace ShoppingMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SliderController : Controller
    {
        EvaraDbContext _db { get; set; }
        IWebHostEnvironment _env { get; set; }

        public SliderController(EvaraDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> SliderPagination(int page = 1, int count = 4)
        {
            var items = await _db.Sliders.Skip((page - 1) * count).Take(count).Select(c => new SliderListItemVm
            {
                Id = c.Id,
                CreatedTime = c.CreatedTime,
                UpdatedTime = c.UpdatedTime,
                ImageUrl = c.ImageUrl,
                IsDeleted = c.IsDeleted,
                Title = c.Title,
                Description = c.Description,
                Discount = c.Discount,
                Button = c.Button,
            }).ToListAsync();
            int totalCount = await _db.Sliders.CountAsync();
            PaginationVm<IEnumerable<SliderListItemVm>> pag = new(totalCount, page, (int)Math.Ceiling((decimal)totalCount / count), items);
            return PartialView("SliderPagination", pag);
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int take = 4;
            int skip = (page - 1) * take;

            var query = _db.Sliders.Select(c => new SliderListItemVm
            {
                Id = c.Id,
                CreatedTime = c.CreatedTime,
                UpdatedTime = c.UpdatedTime,
                ImageUrl = c.ImageUrl,
                IsDeleted = c.IsDeleted,
                Title = c.Title,
                Description = c.Description,
                Discount = c.Discount,
                Button = c.Button,
            });


            query = query.OrderBy(c => c.CreatedTime); 

            var results = query.ToList();

            var filteredData = await query.ToListAsync();

            int total = filteredData.Count();

            var paginatedData = filteredData.Skip(skip).Take(take).ToList();

            PaginationVm<IEnumerable<SliderListItemVm>> pag = new PaginationVm<IEnumerable<SliderListItemVm>>(total, page, (int)Math.Ceiling((decimal)total / take), paginatedData);

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
            return Redirect("/Admin/Slider/Index");
        }
        [HttpPost]
        public async Task<IActionResult> Create(SliderCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View();

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
            Slider slider = new Slider()
            {
                Title = vm.Title,
                Description = vm.Description,
                Button = vm.Button,
                Discount = vm.Discount,
                ImageUrl = filename,
            };
            _db.Sliders.AddAsync(slider);
            await _db.SaveChangesAsync();
            TempData["Create"] = true;
            return Redirect("/Admin/Slider/Index");
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Sliders.FindAsync(id);
            if (data == null) return NotFound();
            return View(new SliderUpdateVm
            {
                ImageUrl = data.ImageUrl,
                Title = data.Title,
                Description = data.Description,
                Button = data.Button,
                Discount = data.Discount,
                
            });
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, SliderUpdateVm vm)
        {
            TempData["Update"] = false;
            if (id == null || id < 0) return BadRequest();
            if (!ModelState.IsValid)
            {
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

            var data = await _db.Sliders.FindAsync(id);
            if (data == null) return NotFound();
            data.Title = vm.Title;
            data.Description = vm.Description;
            data.Discount = vm.Discount;
            data.Button = vm.Button;
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
            await _db.SaveChangesAsync();
            TempData["Update"] = true;
            return Redirect("/Admin/Slider/Index");;
        }
        public async Task<IActionResult> DeleteProduct(int? id)
        {
            
            if (id == null) return BadRequest();
            var data = await _db.Sliders.FindAsync(id);
            if (data == null) return NotFound();
            
            data.IsDeleted = true;
            await _db.SaveChangesAsync();
            return Redirect("/Admin/Slider/Index");;
        }
        public async Task<IActionResult> RestoreProduct(int? id)
        {
            if (id == null) return BadRequest();
            var data = await _db.Sliders.FindAsync(id);
            if (data == null) return NotFound();
            data.IsDeleted = false;
            await _db.SaveChangesAsync();
            return Redirect("/Admin/Slider/Index");;
        }
        public async Task<IActionResult> DeleteFromData(int? id)
        {
            TempData["Delete"] = false;
            if (id == null) return BadRequest();
            var data = await _db.Sliders.FindAsync(id);
            if (data == null) return NotFound();
            TempData["Delete"] = true;
            _db.Sliders.Remove(data);
            await _db.SaveChangesAsync();
            return Redirect("/Admin/Slider/Index");;
        }
    }
}
