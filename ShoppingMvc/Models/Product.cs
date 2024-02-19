using ShoppingMvc.Enums;
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
        public int StockNumber { get; set; }
        public string? Color { get; set; }
        [ForeignKey("SellerData")]
        public int SellerId { get; set; }

        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Weight { get; set; }
        public decimal ShippingFee { get; set; }

        public ProductConditionEnum Condition {  get; set; }

        public IEnumerable<AdditionalInfo>? AdditionalInfos { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
        public Category? Category { get; set; }
        public IEnumerable<Tag>? Tags { get; set; }
        public List<Comment>? Comments { get; set; }
        public SellerData SellerData { get; set; }
    }
}
