using ShoppingMvc.Enums;

namespace ShoppingMvc.Models
{
    public class Order: BaseEntity
    {
        public string? AdditionalAddressInfo { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        public string CardholderName { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string CVV { get; set; }

        public string? PhoneNumber { get; set; }

        public PaymentMethods PaymentMethod { get; set; }

        public Basket Basket { get; set; }
        public IEnumerable<OrderTracking>? OrderTrackings { get; set; }

        public Order()
        {
            Basket = new Basket();
            OrderTrackings = new List<OrderTracking>(); 
        }
    }
}
