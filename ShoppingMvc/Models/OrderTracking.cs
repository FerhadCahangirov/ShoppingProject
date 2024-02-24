using ShoppingMvc.Enums;

namespace ShoppingMvc.Models
{
    public class OrderTracking : BaseEntity
    {
        public ShippingStatus ShippingStatus { get; set; }

        public Order Order { get; set; }
        public SellerData SellerData { get; set; }

        public OrderTracking()
        {
            Order = new Order();
            SellerData = new SellerData();
        }
    }
}
