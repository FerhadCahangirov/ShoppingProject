using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.Enums;
using ShoppingMvc.Helpers;
using ShoppingMvc.Models;
using ShoppingMvc.Models.Identity;
using ShoppingMvc.ViewModels.AuthVm;
using ShoppingMvc.ViewModels.OrderVm;

namespace ShoppingMvc.Controllers
{

    public class AuthController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly EvaraDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, EvaraDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Policy = "NotAuthPolicy")]
        public async Task<IActionResult> Register()
        {
            return View();
        }
        [Authorize(Policy = "NotAuthPolicy")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var user = new AppUser()
            {
                Name = vm.Name,
                Surname = vm.Surname,
                Email = vm.Email,
                UserName = vm.Username
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    return View();
                }
            }
            ViewBag.RegistrationSuccess = true;
            return RedirectToAction("Login", "Auth");
        }
        [Authorize(Policy = "NotAuthPolicy")]
        public async Task<IActionResult> Login()
        {
            return View();
        }
        [Authorize(Policy = "NotAuthPolicy")]
        [HttpPost]
        public async Task<IActionResult> Login(string? returnUrl, LoginVm vm)
        {
            AppUser user;
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (vm.UsernameorEmail.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(vm.UsernameorEmail);
            }
            else
            {
                user = await _userManager.FindByNameAsync(vm.UsernameorEmail);
            }
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, vm.Password, vm.IsRemember, true);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Invalid Login attempt");
                    return View();
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
                return View();
            }
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        public IActionResult SellerRegistration()
        {
            var shopCategories = Enum.GetValues(typeof(SellerCategories))
                .Cast<SellerCategories>()
                .Select(c => new SelectListItem
                {
                    Value = c.ToString(),
                    Text = c.ToString()
                });

            ViewBag.ShopCategories = new SelectList(shopCategories, "Value", "Text");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SellerRegistration(SellerRegistrationVm vm)
        {
            string? username = Request.HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return NotFound();

            AppUser? user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                var shopCategories = Enum.GetValues(typeof(SellerCategories))
                .Cast<SellerCategories>()
                .Select(c => new SelectListItem
                {
                    Value = c.ToString(),
                    Text = c.ToString()
                });

                ViewBag.ShopCategories = new SelectList(shopCategories, "Value", "Text");
                return View(vm);
            }
            if (vm.IdentityImage != null)
            {
                if (!vm.IdentityImage.IsCorrectType())
                {
                    ModelState.AddModelError("ImageFile", "Wrong file type");
                }
                if (!vm.IdentityImage.IsValidSize())
                {
                    ModelState.AddModelError("ImageFile", "Files length must be less than kb");
                }
            }

            string filename = Guid.NewGuid() + Path.GetExtension(vm.IdentityImage?.FileName);

            SellerData sellerData = new SellerData()
            {
                IdentityNumber = vm.IdentityNumber,
                TaxIdentificationNumber = vm.TaxIdentificationNumber,
                ShopDescription = vm.ShopDescription,
                ShopName = vm.ShopName,
                ShopWebsite = vm.ShopWebsite,
                IsApproved = false,
                ShopCity = vm.ShopCity,
                ShopCountry = vm.ShopCountry,
                ShopStreet = vm.ShopStreet,
                ShopPostalCode = vm.ShopPostalCode,
                ShopAdditionalAddressInfo = vm.ShopAdditionalAddressInfo,
                Seller = user,
                ShopCategory = vm.ShopCategory,
            };

            using (Stream fs = new FileStream(Path.Combine(_webHostEnvironment.WebRootPath, "Assets", "assets", "sellerIdentities", filename), FileMode.Create))
            {
                await vm.IdentityImage.CopyToAsync(fs);
                sellerData.IdentityImageUrl = Path.Combine("Assets", "assets", "sellerIdentities", filename);
            }

            if (vm.LogoImage != null)
            {
                if (!vm.LogoImage.IsCorrectType())
                {
                    ModelState.AddModelError("ImageFile", "Wrong file type");
                }
                if (!vm.LogoImage.IsValidSize())
                {
                    ModelState.AddModelError("ImageFile", "Files length must be less than kb");
                }

                string logofileName = Guid.NewGuid() + Path.GetExtension(vm.IdentityImage?.FileName);
                using (Stream fs = new FileStream(Path.Combine(_webHostEnvironment.WebRootPath, "Assets", "assets", "sellerImages", logofileName), FileMode.Create))
                {
                    await vm.LogoImage.CopyToAsync(fs);
                    sellerData.ShopLogoUrl = Path.Combine("Assets", "assets", "sellerImages", logofileName);
                }

            }

            if (vm.ThumbnailImage != null)
            {
                if (!vm.ThumbnailImage.IsCorrectType())
                {
                    ModelState.AddModelError("ImageFile", "Wrong file type");
                }
                if (!vm.ThumbnailImage.IsValidSize())
                {
                    ModelState.AddModelError("ImageFile", "Files length must be less than kb");
                }

                string thumbNailfileName = Guid.NewGuid() + Path.GetExtension(vm.IdentityImage?.FileName);
                using (Stream fs = new FileStream(Path.Combine(_webHostEnvironment.WebRootPath, "Assets", "assets", "sellerImages", thumbNailfileName), FileMode.Create))
                {
                    await vm.ThumbnailImage.CopyToAsync(fs);
                    sellerData.ThumbnailImageUrl = Path.Combine("Assets", "assets", "sellerImages", thumbNailfileName);
                }
            }

            await _db.SellerDatas.AddAsync(sellerData);

            await _db.SaveChangesAsync();

            TempData["SellerRequestSent"] = true;
            return Redirect("/Home/Index");
        }


        [Authorize(Policy = "AuthRequiredPolicy")]
        public async Task<IActionResult> Account()
        {
            var username = HttpContext.User.Identity?.Name;

            AppUser? user = await _db.Users.FirstOrDefaultAsync(x => x.UserName == username);
            if (user == null) return NotFound();

            AccountVm accountVm = user.FromAppUser_ToAccountVm();

            IEnumerable<Order> orders = await _db.Orders
                .Include(o => o.Basket)
                .ThenInclude(b => b.User)
                .Include(o => o.Basket)
                .ThenInclude(b => b.BasketItems)
                .ThenInclude(bi => bi.Product)
                .Where(o => o.Basket.User.UserName == username).ToListAsync();

            IEnumerable<CustomerOrderListItemVm> ordersVm = orders.Select(o => o.FromOrder_ToCustomerOrderListItemVm());
            accountVm.Orders = ordersVm;

            return View(accountVm);
        }

        [Authorize(Policy = "AuthRequiredPolicy")]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(AccountVm vm)
        {
            AppUser? user = await _db.Users.FirstOrDefaultAsync(x => x.UserName == vm.UserName);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
                return View(vm);

            user.Name = vm.Name;
            user.Surname = vm.Surname;
            user.UserName = vm.UserName;
            user.Email = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;

            if (vm.ProfileImageFile != null)
            {
                if (!vm.ProfileImageFile.IsCorrectType())
                {
                    ModelState.AddModelError("ImageFile", "Wrong file type");
                }
                if (!vm.ProfileImageFile.IsValidSize())
                {
                    ModelState.AddModelError("ImageFile", "Files length must be less than kb");
                }

                string fileName = Guid.NewGuid() + Path.GetExtension(vm.ProfileImageFile?.FileName);

                using (Stream fs = new FileStream(Path.Combine(_webHostEnvironment.WebRootPath, "Assets", "assets", "userProfiles", fileName), FileMode.Create))
                {
                    await vm.ProfileImageFile.CopyToAsync(fs);
                    user.ProfileImageUrl = Path.Combine(Path.Combine("Assets", "assets", "userProfiles", fileName));
                }
            }

            IdentityResult result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded) return BadRequest(result);

            await _db.SaveChangesAsync();

            return RedirectToAction("Account", "Auth");
        }
    }
}
