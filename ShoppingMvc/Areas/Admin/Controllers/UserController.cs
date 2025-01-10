using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.Models.Identity;
using ShoppingMvc.ViewModels.CommonVm;
using ShoppingMvc.ViewModels.UserVm;

namespace ShoppingMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Moderator")]
    public class UserController : Controller
    {
        private readonly EvaraDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<AppUser> _userManager;

        public UserController(EvaraDbContext db, IWebHostEnvironment env, UserManager<AppUser> userManager)
        {
            _db = db;
            _env = env;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            int take = 8;
            int skip = 0;

            List<UserListItemVm> users = new List<UserListItemVm>();
            foreach (AppUser user in _db.Users.ToList())
            {
                var roleNames = await _userManager.GetRolesAsync(user);
                string roleName = roleNames.First();
                users.Add(new UserListItemVm
                {
                    RoleName = string.IsNullOrEmpty(roleName) ? "customer" : roleName.ToLower(),
                    User = user
                });
            }

            var totalUsers = users.Count();
            var usersList = users.Skip(skip).Take(take).ToList();
            
            PaginationVm<IEnumerable<UserListItemVm>> paginatedData = new PaginationVm<IEnumerable<UserListItemVm>>(totalUsers, skip + 1, (int)Math.Ceiling((decimal)totalUsers / take), usersList, take);

            return View(paginatedData);
        }

    }
}
