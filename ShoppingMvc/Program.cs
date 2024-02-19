using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.Enums;
using ShoppingMvc.Models.Identity;

namespace ShoppingMvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<EvaraDbContext>(opt =>
            {
                opt.UseSqlServer(builder.Configuration.GetConnectionString("MSSql"));
            }).AddIdentity<AppUser, AppRole>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            })
            .AddDefaultTokenProviders()
            .AddUserStore<UserStore<AppUser, AppRole, EvaraDbContext, int>>()
            .AddRoleStore<RoleStore<AppRole, EvaraDbContext, int>>();

            builder.Services.AddSession();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddRazorPages();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);

                options.LoginPath = new PathString("/Auth/Login");
                options.LogoutPath = new PathString("/Auth/Logout");
                options.AccessDeniedPath = new PathString("/Home/AccessDenied");
                options.SlidingExpiration = true;
            });

            builder.Services.AddTransient<IAuthorizationHandler, SellerAndApprovedHandler>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
                options.AddPolicy("MemberPolicy", policy => policy.RequireRole("Member"));
                options.AddPolicy("SellerPolicy", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new SellerAndApprovedRequirement());
                });

                options.AddPolicy("ModeratorPolicy", policy => policy.RequireRole("Moderator"));
                options.AddPolicy("NotSellerPolicy", policy => policy.RequireAssertion(context => !context.User.IsInRole("Seller")));
                options.AddPolicy("NotAuthPolicy", policy => policy.RequireAssertion(context => !context.User.Identity.IsAuthenticated));
                options.AddPolicy("AuthRequiredPolicy", policy => policy.RequireAuthenticatedUser());
            });


            var app = builder.Build();

            using (var service = app.Services.CreateScope())
            {
                var dbContext = service.ServiceProvider.GetRequiredService<EvaraDbContext>();
                var _userManager = service.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var _roleManager = service.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

                foreach (var item in Enum.GetValues(typeof(Roles)))
                {
                    if (!_roleManager.RoleExistsAsync(item.ToString()).GetAwaiter().GetResult())
                    {
                        _roleManager.CreateAsync(new AppRole
                        {
                            Name = item.ToString()
                        }).GetAwaiter().GetResult();
                    }
                }

                if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
                    _roleManager.CreateAsync(new AppRole("Admin")).GetAwaiter().GetResult();
                if (!_roleManager.RoleExistsAsync("Member").GetAwaiter().GetResult())
                    _roleManager.CreateAsync(new AppRole("Member")).GetAwaiter().GetResult();
                if (!_roleManager.RoleExistsAsync("Moderator").GetAwaiter().GetResult())
                    _roleManager.CreateAsync(new AppRole("Moderator")).GetAwaiter().GetResult();
                if (!_roleManager.RoleExistsAsync("Seller").GetAwaiter().GetResult())
                    _roleManager.CreateAsync(new AppRole("Seller")).GetAwaiter().GetResult();

                if (_userManager.FindByEmailAsync("admin@mail.ru").GetAwaiter().GetResult() == null)
                {
                    var user = new AppUser()
                    {
                        Name = "Admin",
                        Surname = "Adminov",
                        Email = "admin@mail.ru",
                        UserName = "admin123"
                    };
                    var result = _userManager.CreateAsync(user, "Admin123$").GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(user, "Admin").GetAwaiter().GetResult();
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();


            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Default route
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"
                );

                endpoints.MapControllerRoute(name: "Seller", pattern: "{area:exists=Seller}/{controller=Product}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(name: "Admin", pattern: "{area:exists=Admin}/{controller=Slider}/{action=Index}/{id?}");
            });



            app.Run();
        }
    }
}