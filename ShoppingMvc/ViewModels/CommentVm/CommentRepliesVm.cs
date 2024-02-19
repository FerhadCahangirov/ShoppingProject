using ShoppingMvc.Models;

namespace ShoppingMvc.ViewModels.CommentVm
{
    public class CommentRepliesVm
    {
        public Comment Comment { get; set; } = new Comment();
        public List<Reply>? Replies { get; set; } = new List<Reply>();

        public CommentRepliesVm()
        {
            Comment = new Comment();
            Replies = new List<Reply>();
        }
    }
}
