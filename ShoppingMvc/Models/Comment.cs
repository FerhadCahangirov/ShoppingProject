using ShoppingMvc.Models.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMvc.Models
{
    public class Comment : BaseEntity
    {
        public string Message { get; set; }
        public double Rating { get; set; }

        [InverseProperty("Comment")]
        public List<Reply>? Replies { get; set; } = new List<Reply>();

        public AppUser User { get; set; } = new AppUser();
        public Product Product { get; set; } = new Product();
    }
}
