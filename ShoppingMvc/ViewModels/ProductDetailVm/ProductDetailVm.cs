using ShoppingMvc.Models;
using ShoppingMvc.Models.Cards;

namespace ShoppingMvc.ViewModels.ProductDetailVm
{
    public class ProductDetailVm
    {
        public List<Comment> Comments { get; set; }
        public Comment Comment { get; set; }
        public Product Product { get; set; }

        public ProductDetailVm()
        {
            Comments = new List<Comment>();
            Product = new Product();    
            Comment = new Comment();
        }
    }
}
