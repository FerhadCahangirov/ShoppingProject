using ShoppingMvc.Models;

namespace ShoppingMvc.ViewModels.OrderVm
{
    public class CustomerOrderListItemVm
    {
        public int Id { get; set; }
        public string? OrderedDate { get; set; }
        public string? Status { get; set; }
        public string? TotalPrice { get; set; }
        public int ItemCount { get; set; }
    }

    public static class OrderListItemVmConvertion
    {
        public static CustomerOrderListItemVm FromOrder_ToCustomerOrderListItemVm(this Order order)
        {
            if (order.Basket == null)
                throw new ArgumentException("The Basket object is null.");
            else if (order.Basket.BasketItems == null)
                throw new ArgumentException("The Basket Items object is null.");


            decimal totalPrice = order.Basket.BasketItems.ToList()
                .Sum(bi => bi == null ? 0 : (bi.Product == null ? 0 : ((bi.Product.Price - (bi.Product.Price * bi.Product.DiscountRate / 100)) + bi.Product.ShippingFee) * bi.Count));

            int totalCount = order.Basket.BasketItems.ToList()
                .Sum(bi => bi == null ? 0 : (bi.Product == null ? 0 : bi.Count));

            return new CustomerOrderListItemVm()
            {
                Id = order.Id,
                OrderedDate = order.CreatedTime.ToString("yyyyMMdd"),
                TotalPrice = totalPrice.ToString("0.00"),
                ItemCount = totalCount
            };
        }
    }

}
