using ShoppingMvc.Models;
using ShoppingMvc.ViewModels.ProductVm;

namespace ShoppingMvc.ViewModels.ProductDetailVm
{
    public class ProductDetailVm
    {
        public List<Comment> Comments { get; set; }
        public Comment Comment { get; set; }
        public ProductListItemVm Product { get; set; }

        public ProductDetailVm()
        {
            Comments = new List<Comment>();
            Product = new ProductListItemVm();    
            Comment = new Comment();
        }
    }
}
