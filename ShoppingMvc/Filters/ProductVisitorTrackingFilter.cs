using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.Models;
using ShoppingMvc.Models.Identity;

namespace ShoppingMvc.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ProductVisitorTrackingFilterFactoryAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var filter = serviceProvider.GetRequiredService<ProductVisitorTrackingFilter>();
            return filter;
        }
    }

    public class ProductVisitorTrackingFilter : IAsyncActionFilter
    {
        private readonly EvaraDbContext _db;

        public ProductVisitorTrackingFilter(EvaraDbContext db)
        {
            _db = db;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            int id = Convert.ToInt32(context.ActionArguments["id"]);

            Product? product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null) throw new ArgumentException("Product Not Found");

            string? hostAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            string? username = context.HttpContext.User.Identity?.Name;

            if (!string.IsNullOrEmpty(username))
            {
                AppUser? user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == username);

                ProductVisitorData productVisitorData = new ProductVisitorData()
                {
                    Product = product,
                    User = user,
                    HostAddress = hostAddress,
                };

                await _db.ProductVisitorDatas.AddAsync(productVisitorData);
                await _db.SaveChangesAsync();


                await next();
            }
            else
            {
                ProductVisitorData productVisitorData = new ProductVisitorData()
                {
                    Product = product,
                    HostAddress = hostAddress,
                };

                await _db.ProductVisitorDatas.AddAsync(productVisitorData);
                await _db.SaveChangesAsync();

                await next();
            }
        }
    }
}
