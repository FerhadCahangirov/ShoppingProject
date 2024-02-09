/*using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;
using ShoppingMvc.Models;
using ShoppingMvc.ViewModels.HomeVm;

namespace ShoppingMvc.Areas.Admin.Controllers
{
    public class CommentsController : Controller
    {
       EvaraDbContext _db {  get; set; }

        public CommentsController(EvaraDbContext db)
        {
            _db = db;
        }


        public async Task<IActionResult> Index()
        {
            var model = new HomeVm();

            // Fetch comments from the database and include related replies
            var commentsFromDatabase = await _db.Comments.Include(c => c.Replies).ToListAsync();
            model.Comments.AddRange(commentsFromDatabase);

            return View(model);
        }
        [HttpPost]
        public IActionResult AddComment(Comment comment)
        {
            // Save comment to the database
            _db.Comments.Add(comment);
            _db.SaveChangesAsync();
            return RedirectToAction("Index", "ProductDetails"); // Redirect to home page or wherever needed
        }

        [HttpPost]
        public async Task<IActionResult> AddReply(Reply reply)
        {
            // Save reply to the database
            _db.Replys.Add(reply);
            _db.SaveChangesAsync();
            return RedirectToAction("Index", "ProductDetails"); // Redirect to home page or wherever needed
        }
    }
}
*/