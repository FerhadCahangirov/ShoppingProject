using ShoppingMvc.Models.Identity;
using ShoppingMvc.Models;
using System.ComponentModel.DataAnnotations;

namespace ShoppingMvc.ViewModels.AuthVm
{
    public class SellerRegistrationVm
    {
        [Required]
        public IFormFile IdentityImage { get; set; }
        [Required]
        public string? IdentityNumber { get; set; }
        [Required]
        public string? TaxIdentificationNumber { get; set; }

        [Required]
        public string? ShopName { get; set; }
        [Required]
        public string? ShopWebsite { get; set; }
        [Required]
        public string? ShopDescription { get; set; }

        public string? ShopAdditionalAddressInfo { get; set; }
        [Required]
        public string? ShopStreet { get; set; }
        [Required]
        public string? ShopCity { get; set; }
        [Required]
        public string? ShopCountry { get; set; }
        [Required]
        public string? ShopState { get; set; }
        [Required]  
        public string? ShopPostalCode { get; set; }

        public IFormFile? LogoImage { get; set; }
        public IFormFile? ThumbnailImage { get; set; }

    }
}
