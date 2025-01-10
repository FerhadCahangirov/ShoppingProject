using ShoppingMvc.Models;
using System.ComponentModel.DataAnnotations;
using ShoppingMvc.Enums;

namespace ShoppingMvc.ViewModels.AuthVm
{
    public class SellerRegistrationVm
    {
        [Required(ErrorMessage = "Identity image is required.")]
        [Display(Name = "Identity Image")]
        public IFormFile IdentityImage { get; set; }

        [Required(ErrorMessage = "Identity number is required.")]
        [Display(Name = "Identity Number")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "Identity number must be exactly 9 digits.")]
        public string? IdentityNumber { get; set; }

        [Required(ErrorMessage = "Tax identification number is required.")]
        [Display(Name = "Tax Identification Number")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "Tax identification number must be exactly 9 digits.")]
        public string? TaxIdentificationNumber { get; set; }

        [Required(ErrorMessage = "Shop name is required.")]
        [Display(Name = "Shop Name")]
        public string? ShopName { get; set; }

        [Required(ErrorMessage = "Shop website is required.")]
        [Display(Name = "Shop Website")]
        public string? ShopWebsite { get; set; }

        [Required(ErrorMessage = "Shop description is required.")]
        [Display(Name = "Shop Description")]
        public string? ShopDescription { get; set; }

        [Display(Name = "Shop Additional Address Info")]
        public string? ShopAdditionalAddressInfo { get; set; }

        [Required(ErrorMessage = "Shop street is required.")]
        [Display(Name = "Shop Street")]
        public string? ShopStreet { get; set; }

        [Required(ErrorMessage = "Shop city is required.")]
        [Display(Name = "Shop City")]
        public string? ShopCity { get; set; }

        [Required(ErrorMessage = "Shop country is required.")]
        [Display(Name = "Shop Country")]
        public string? ShopCountry { get; set; }

        [Required(ErrorMessage = "Shop category is required.")]
        [Display(Name = "Shop Category")]
        public SellerCategories ShopCategory { get; set; }

        [Required(ErrorMessage = "Shop postal code is required.")]
        [Display(Name = "Shop Postal Code")]
        public string? ShopPostalCode { get; set; }

        [Display(Name = "Logo Image")]
        public IFormFile? LogoImage { get; set; }

        [Display(Name = "Thumbnail Image")]
        public IFormFile? ThumbnailImage { get; set; }
    }
}