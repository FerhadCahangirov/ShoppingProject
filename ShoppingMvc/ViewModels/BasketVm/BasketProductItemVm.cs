using ShoppingMvc.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMvc.ViewModels.BasketVm
{
    public class BasketProductItemVm
    {
        public int Id { get; set; }
        [Column(TypeName = "smallmoney")]
        public string? Description { get; set; }
        public float SellPrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? Title { get; set; }
        public int Count { get; set; }
    }

    public static class BasketProductItemVmConvertion
    {
        public static BasketProductItemVm FromProduct_ToBasketProductItemVm(this Product product)
        {
            return new BasketProductItemVm
            {
                Id = product.Id,
                ImageUrl = product.ProductImages?.FirstOrDefault(pi => pi.IsPrimary)?.ImageUrl,
                Title = product.Title,
                SellPrice = (float)product.Price,
            };

        }
    }
}
