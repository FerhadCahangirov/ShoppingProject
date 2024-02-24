using ShoppingMvc.Models;
using ShoppingMvc.Models.Identity;
using ShoppingMvc.ViewModels.BasketVm;
using ShoppingMvc.ViewModels.ProductVm;
using System.Globalization;

namespace ShoppingMvc.ViewModels.OrderVm
{
    public class SellerOrderListItemVm
    {
        public int Id { get; set; }
        public string? OrderedDate { get; set; }
        public string? Status { get; set; }
        public string? SubTotalPrice { get; set; }
        public string? TotalPrice { get; set; }
        public string? TotalShippingFee { get; set; }
        public int ItemCount { get; set; }
        public string? PaymentMethod { get; set; }

        public IEnumerable<BasketProductItemVm> Items { get; set; }
        public AppUser Customer { get; set; }
        public Order Order { get; set; }

        public SellerOrderListItemVm()
        {
            Items = new List<BasketProductItemVm>();
            Customer = new AppUser();
            Order = new Order();
        }
    }

    public static class SellerOrderListItemVmConvertion
    {
        public static SellerOrderListItemVm FromOrder_ToSellerOrderListItemVm(this Order order, AppUser seller)
        {

            IEnumerable<BasketItem> basketItems = order.Basket.BasketItems
                .Where(bi => bi.Product.SellerData.Seller.UserName == seller.UserName).ToList();

            IEnumerable<BasketProductItemVm> basketProductItemVms = basketItems
                .Select(bi => new BasketProductItemVm()
                {
                    Id = bi.Id,
                    Count = bi.Count,
                    Product = bi.Product.FromProduct_ToProductListItemVm(),
                    Basket = bi.Basket,
                    TotalItemPrice = (((bi.Product.Price - (bi.Product.Price * bi.Product.DiscountRate / 100)) + bi.Product.ShippingFee) * bi.Count).ToString("0.00")
                }).ToList();

            decimal totalSubPrice = basketItems
                .Sum(bi => (bi.Product.Price - (bi.Product.Price * bi.Product.DiscountRate / 100)) * bi.Count);

            decimal totalShippingFee = basketItems
                .Sum(bi => bi.Product.ShippingFee);

            decimal totalPrice = basketItems
                .Sum(bi => (bi.Product.Price - (bi.Product.Price * bi.Product.DiscountRate / 100) + bi.Product.ShippingFee) * bi.Count);

            int totalCount = basketItems
                .Sum(bi => bi.Count);

            AppUser customer = order.Basket.User;

            return new SellerOrderListItemVm
            {
                Id = order.Id,
                OrderedDate = order.CreatedTime.ToString("ddd, MMM d, yyyy, h:mmtt", CultureInfo.InvariantCulture),
                PaymentMethod = order.PaymentMethod.ToString(),
                TotalPrice = totalPrice.ToString("0.00"),
                SubTotalPrice = totalSubPrice.ToString("0.00"),
                TotalShippingFee = totalShippingFee.ToString("0.00"),
                ItemCount = totalCount,
                Items = basketProductItemVms,
                Customer = customer,
                Order = order,
            };
        }
    }
}
