using ShoppingMvc.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMvc.ViewModels.ProductVm
{
    public class ProductUpdateVm
    {
        [Required]
        public int Id {  get; set; }
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        [Column(TypeName = "smallmoney")]
        public decimal Price { get; set; }
        public int DiscountRate { get; set; }
        public int StockNumber { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public int CategoryId { get; set; }
        public List<int>? TagsId { get; set; }
        
        public List<ProductImage>? ProductImagesData { get; set; }
        public List<AdditionalInfo>? AdditionalInfos { get; set; }
        public IFormFileCollection? ProductImages { get; set; }
    }

    public static class ProductUpdateVmConvertion
    {
        public static ProductUpdateVm FromProduct_ToProductUpdateVm(this Product product)
        {
            return new ProductUpdateVm()
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                DiscountRate = product.DiscountRate,
                StockNumber = product.StockNumber,
                Color = product.Color,
                Size = product.Size,
                CategoryId = product.CategoryId,
                TagsId = product?.TagProducts?.Select(tp => tp.TagId).ToList(),
                AdditionalInfos = product?.AdditionalInfos?.ToList(),
                ProductImagesData = product?.ProductImages
            };
        }
    }
}
