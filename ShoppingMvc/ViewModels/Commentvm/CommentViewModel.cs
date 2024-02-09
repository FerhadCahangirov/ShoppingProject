using ShoppingMvc.Models;

namespace ShoppingMvc.ViewModels.Commentvm
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime PostedDate { get; set; }
        public List<Reply> Replies { get; set; }
    }
}
