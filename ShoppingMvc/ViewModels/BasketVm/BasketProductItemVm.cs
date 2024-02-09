using ShoppingMvc.Models.Categories;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMvc.ViewModels.BasketVm
{
    public class BasketProductItemVm
    {
        public int Id { get; set; }
        [Column(TypeName = "smallmoney")]
        public string Description { get; set; }
        public float SellPrice { get; set; }
        public string? ImageUrl { get; set; }
        public string Title { get; set; }
        public int Count { get; set; }
    }
}
