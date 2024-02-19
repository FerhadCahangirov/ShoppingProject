using ShoppingMvc.Enums;
using ShoppingMvc.Models;

namespace ShoppingMvc.ViewModels.ProductVm
{
    public class ProductListItemVm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Price { get; set; }
        public int DiscountRate { get; set; }
        public int StockNumber { get; set; }
        public string? Color { get; set; }
        public string CategoryName { get; set; }
        public List<string>? Tags { get; set; } = new List<string>();
        public List<ProductImage>? ProductImages { get; set; } = new List<ProductImage>();
        public IEnumerable<AdditionalInfo>? AdditionalInfos { get; set; } = Enumerable.Empty<AdditionalInfo>();
        public IEnumerable<Comment>? Comments { get; set; } = Enumerable.Empty<Comment>();
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsArchived { get; set; }
        public string? PrimaryImageName { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public string? SellPrice { get; set; }

        public string? Width { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }
        public string? ShippingFee { get; set; }

        public ProductConditionEnum Condition { get; set; }
    }

    public static class ProductListVmConvertion
    {
        public static ProductListItemVm FromProduct_ToProductListItemVm(this Product product)
        {
            return new ProductListItemVm
            {
                Id = product.Id,
                CategoryName = product.Category?.Name,
                Tags = product?.Tags?.Select(t => t?.Title).Where(t => t != null).ToList(),
                Title = product.Title,
                Description = product.Description,
                DiscountRate = product.DiscountRate,
                Price = product.Price.ToString("0.00"),
                StockNumber = product.StockNumber,
                AdditionalInfos = product.AdditionalInfos,
                Comments = product.Comments,
                CreatedDate = product.CreatedTime,
                UpdatedDate = product.UpdatedTime,
                IsDeleted = product.IsDeleted,
                IsArchived = product.IsArchived,
                SellPrice = (product.Price - (product.Price * product.DiscountRate / 100)).ToString("0.00"),
                PrimaryImageName = product?.ProductImages?.FirstOrDefault(pi => pi.IsPrimary)?.ImageName,
                PrimaryImageUrl = product?.ProductImages?.FirstOrDefault(pi => pi.IsPrimary)?.ImageUrl,
                ProductImages = product?.ProductImages,
                Width = product?.Width.ToString("0.00"),
                Height = product?.Height.ToString("0.00"),
                Weight = product?.Weight.ToString("0.00"),
                ShippingFee = product?.ShippingFee.ToString("0.00"),
                Condition = product.Condition
            };
        }
    }


}
