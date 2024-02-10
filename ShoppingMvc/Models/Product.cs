using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMvc.Models
{
    public class Product : BaseEntity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        [Column(TypeName = "smallmoney")]
        public decimal Price { get; set; }
        public int DiscountRate { get; set; }
        public int CategoryId { get; set; }
        public int StockNumber { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }

        public IEnumerable<AdditionalInfo>? AdditionalInfos { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
        public Category? Category { get; set; }
        public IEnumerable<ProductTag>? TagProducts { get; set; }
        public List<Comment>? Comments { get; set; }
    }
}
