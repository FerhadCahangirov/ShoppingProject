using ShoppingMvc.Models;

namespace ShoppingMvc.ViewModels.OrderVm
{
    public class SellerOrderListVm
    {
        public string? SubTotal { get; set; }
        public string? ShippingTotal { get; set; }
        public string? Total { get; set; }

        public IEnumerable<SellerOrderListItemVm> Orders { get; set; }

        public SellerOrderListVm()
        {
            Orders = new List<SellerOrderListItemVm>();
        }
    }
}
