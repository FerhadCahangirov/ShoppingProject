using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShoppingMvc.Contexts;
using ShoppingMvc.Models;
using ShoppingMvc.ViewModels.BasketVm;
using ShoppingMvc.ViewModels.Commentvm;
using ShoppingMvc.ViewModels.ProductDetailVm;
using ShoppingMvc.ViewModels.ProductVm;

namespace ShoppingMvc.Controllers
{
    public class ProductDetailsController : Controller
    {
        EvaraDbContext _db { get; set; }

        public ProductDetailsController(EvaraDbContext db)
        {
            _db = db;
        }

        private List<Comment> _comments = new List<Comment>();

        public IActionResult Index(int id)
        {
            Product? product = _db.Products
                .Include(x => x.AdditionalInfos)
                .Include(x => x.ProductImages)
                .FirstOrDefault(x => x.Id == id);

            if (product == null)    
                return NotFound();

            ProductDetailVm model = new ()
            {
                Comments = _db.Comments.ToList(),
                Product = product.FromProduct_ToProductListItemVm(),
            };

            return View(model);
        }

        // POST: ProductDetails/AddComment
        [HttpPost]
        public async  Task<IActionResult> AddComment(CommentViewModel commentViewModel)
        {
            IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors).ToArray();
            if (ModelState.IsValid)
            {
                var comment = new Comment
                {
                    UserName = commentViewModel.UserName,
                    Message = commentViewModel.Message,
                };
                _db.Comments.Add(comment);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", "ProductDetails");
            }
            else
            {
                List<Comment> comments = _db.Comments.ToList();// Retrieve all comments
                ProductDetailVm model = new ProductDetailVm
                {
                    Comments = comments,
                    Comment = new Comment(),
                };
                return View("Index", model);
            }
        }
       

        // POST: ProductDetails/AddReply
        [HttpPost]
        public async Task<IActionResult> AddReply(Reply reply)
        {
            var existingComment = _db.Comments.Find(reply.CommentId);
            if (existingComment == null)
            {
                return NotFound();
            }

            _db.Replys.Add(reply);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult>AddBasket(int? id)
        {
            if(id==null || id<=0) return BadRequest();
            if(!await _db.Products.AnyAsync(p=>p.Id == id)) return NotFound();
            var basket = JsonConvert.DeserializeObject <List<BasketProductandCountVm>>(HttpContext.Request.Cookies["basket"]??"[]");
            var existItem = basket.Find(p => p.Id == id);
            if(existItem == null)
            {
                basket.Add(new BasketProductandCountVm
                {
                    Id = (int)id,
                    Count = 1
                        
                });
            }
            else
            {
                existItem.Count++;
            }
            HttpContext.Response.Cookies.Append("basket",JsonConvert.SerializeObject(basket));
            return Ok();
        }

        public async Task<IActionResult> RemoveBasket(int? id)
        {
            if (id == null || id <= 0) return BadRequest();
            if (!await _db.Products.AnyAsync(p => p.Id == id)) return NotFound();
            var dasket = JsonConvert.DeserializeObject<List<BasketProductandCountVm>>(HttpContext.Request.Cookies["basket"] ?? "[]");
            var existItem = dasket.Find(b => b.Id == id);
            if (existItem.Count == 1)
            {
                dasket.Remove(new BasketProductandCountVm()
                {
                    Id = (int)id,
                    Count = 1
                });
            }
            else
            {
                existItem.Count--;
            }
            HttpContext.Response.Cookies.Append("basket", JsonConvert.SerializeObject(dasket));
            return Ok();
        }

    }
}
