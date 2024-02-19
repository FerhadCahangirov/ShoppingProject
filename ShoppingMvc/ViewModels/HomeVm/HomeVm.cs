using ShoppingMvc.Models;
using ShoppingMvc.ViewModels.CommentVm;
using ShoppingMvc.ViewModels.CommonVm;
using ShoppingMvc.ViewModels.ProductVm;
using ShoppingMvc.ViewModels.SliderVm;

namespace ShoppingMvc.ViewModels.HomeVm
{
    public class HomeVm
    {
        public PaginationVm<IEnumerable<ProductListItemVm>> PaginationProduct { get; set; }
        public IEnumerable<SliderListItemVm> SliderListItems { get; set; }
        public IEnumerable<ProductListItemVm> ProductListItems { get; set; }
        public List<Comment> Comments { get; set; }
        public IEnumerable<CommentViewModel> CommentViewModels { get; set; }

        /*public HomeVm()
        {
            Comments = new List<Comment>();
        }*/
        public Comment Comment { get; set; }

        public HomeVm()
        {
            Comments = new List<Comment>();
            Comment = new Comment(); // Initialize the new comment object
        }
    }
}
