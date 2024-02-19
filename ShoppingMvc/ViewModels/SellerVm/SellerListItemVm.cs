using ShoppingMvc.Models;
using ShoppingMvc.Models.Identity;
using ShoppingMvc.ViewModels.CommonVm;
using ShoppingMvc.ViewModels.ProductVm;

namespace ShoppingMvc.ViewModels.SellerVm
{
    public class SellerListItemVm
    {
        public SellerData SellerData { get; set; }
        public AppUser User { get; set; }

        public PaginationVm<IEnumerable<ProductListItemVm>>? Products { get; set; }
    }
}
