using ShoppingMvc.Models.Tags;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMvc.ViewModels.ProductVm
{
    public class ProductUpdateVm
    {
        public IFormFile MainImage { get; set; }
        public string? Description { get; set; }
        [Column(TypeName = "smallmoney")]
        public decimal SellPrice { get; set; }
        [Column(TypeName = "smallmoney")]
        public decimal CostPrice { get; set; }
        public int? CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public string Title { get; set; }
        public int RateRange { get; set; }
        public bool IsArchived { get; set; }
        public Tag? Tag { get; set; }
        public IEnumerable<int> TagsId { get; set; }
    }
}
