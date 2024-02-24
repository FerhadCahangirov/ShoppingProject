using ShoppingMvc.Models.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMvc.Models
{
    public class Reply : BaseEntity
    {
        public string Message { get; set; }
        [ForeignKey("Comment")]
        public int CommentId { get; set; }

        public Comment Comment { get; set; }
        public AppUser User { get; set; }

        public Reply()
        {
            User = new AppUser();
            Comment = new Comment();
            Message = string.Empty;
        }
    }
}
