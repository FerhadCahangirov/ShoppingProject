using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShoppingMvc.Contexts;
using ShoppingMvc.Models;
using ShoppingMvc.Models.Identity;
using ShoppingMvc.ViewModels.CommentVm;
using ShoppingMvc.ViewModels.ProductDetailVm;
using ShoppingMvc.ViewModels.ProductVm;

namespace ShoppingMvc.Controllers
{
    public class ProductDetailsController : Controller
    {
        EvaraDbContext _db { get; set; }
        private readonly UserManager<AppUser> _userManager;

        public ProductDetailsController(EvaraDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index(int id)
        {
            Product? product = _db.Products
                .Include(x => x.AdditionalInfos)
                .Include(x => x.ProductImages)
                .Include(x => x.Category)
                .Include(x => x.Tags)
                .FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound();

            List<Comment> comments = _db.Comments
                    .Include(c => c.Product)
                    .Include(c => c.User)
                    .Include(c => c.Replies)
                    .Where(c => c.Product == product)
                    .ToList();

            int totalCommentsCount = comments.Count;

            comments.ForEach(comment =>
            {
                if (comment.Replies?.Count > 0)
                    totalCommentsCount += comment.Replies.Count;
            });

            double totalRatingPercentage = 0;
            double totalRatingCount = 0;

            comments.ForEach(comment =>
            {
                totalRatingPercentage += comment.Rating;
                totalRatingCount++;
            });

            totalRatingPercentage /= totalRatingCount;

            ProductDetailVm model = new()
            {
                Comments = comments,
                Product = product.FromProduct_ToProductListItemVm(),
                Categories = _db.Categories.ToList(),
                IsAuthenticatedUserExists = HttpContext.User.Identity.IsAuthenticated,
                TotalCommentsCount = totalCommentsCount,
                TotalRatingPercentage = totalRatingPercentage
            };

            return View(model);
        }

        // POST: ProductDetails/AddComment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(CommentViewModel commentViewModel)
        {
            var name = HttpContext.User.Identity?.Name;

            AppUser? user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == name);

            if (user == null) return NotFound();

            Product? product = await _db.Products.FirstOrDefaultAsync(x => x.Id == commentViewModel.ProductId);
            if (product == null) return NotFound();

            IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors).ToArray();
            if (ModelState.IsValid)
            {
                var comment = new Comment
                {
                    User = user,
                    Product = product,
                    Message = commentViewModel.Message,
                    Rating = commentViewModel.Rating
                };
                _db.Comments.Add(comment);
                await _db.SaveChangesAsync();
                return Ok();
            }
            else
            {
                List<Comment> comments = _db.Comments.ToList();
                ProductDetailVm model = new ProductDetailVm
                {
                    Comments = comments
                };
                return RedirectToAction("Index", product.Id);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReply(ReplyViewModel vm)
        {
            var existingComment = _db.Comments.Find(vm.CommentId);
            if (existingComment == null)
            {
                return NotFound();
            }

            var username = HttpContext.User.Identity?.Name;

            AppUser? user = await _db.AppUsers.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null) return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Reply reply = new Reply
            {
                Message = vm.Message,
                Comment = existingComment,
                User = user,
                CommentId = vm.CommentId
            };

            EntityEntry<Reply> entity = await _db.Replys.AddAsync(reply);

            await _db.SaveChangesAsync();

            return Json(new { Success = true });
        }


    }
}
