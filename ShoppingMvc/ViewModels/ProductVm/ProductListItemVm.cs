using ShoppingMvc.Models.Categories;
using ShoppingMvc.Models.Tags;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMvc.ViewModels.ProductVm
{
    public class ProductListItemVm
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? Description { get; set; }
        [Column(TypeName = "smallmoney")]
        public decimal SellPrice { get; set; }
        [Column(TypeName = "smallmoney")]
        public decimal CostPrice { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string? ImageUrl { get; set; }
        public int RateRange { get; set; }
        public bool IsArchived { get; set; }
        public string Title { get; set; }
        public IEnumerable<Tag>? Tags { get; set; }
    }
}
