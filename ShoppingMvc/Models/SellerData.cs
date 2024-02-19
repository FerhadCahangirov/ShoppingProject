using ShoppingMvc.Models.Identity;

namespace ShoppingMvc.Models
{
    public class SellerData : BaseEntity
    {
        public string? IdentityImageUrl { get; set; }
        public string? IdentityNumber { get; set; }
        public string? TaxIdentificationNumber { get; set; }

        public bool? IsApproved { get; set; } = false;

        public string? ShopName { get; set; }
        public string? ShopWebsite { get; set; }
        public string? ShopDescription { get; set; }
        public string? ShopAdditionalAddressInfo { get; set; }
        public string? ShopStreet { get; set; }
        public string? ShopCity { get; set; }
        public string? ShopCountry { get; set; }
        public string? ShopState { get; set; }
        public string? ShopPostalCode { get; set; }
        public string? ShopLogoUrl { get; set; }
        public string? ThumbnailImageUrl { get; set; }

        public List<Product>? Products { get; set; }

        public AppUser Seller { get; set; }
    }
}
