using ShoppingMvc.Models;
using ShoppingMvc.ViewModels.ProductVm;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMvc.ViewModels.BasketVm
{
    public class BasketProductItemVm
    {
        public int Id { get; set; }
        public ProductListItemVm Product { get; set; }
        public int Count { get; set; }
        public Basket Basket { get; set; }
        public string TotalItemPrice { get; set; }
    }
}
