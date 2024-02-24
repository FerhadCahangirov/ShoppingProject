using ShoppingMvc.Enums;
using ShoppingMvc.Models;
using ShoppingMvc.ViewModels.BasketVm;
using System.ComponentModel.DataAnnotations;

namespace ShoppingMvc.ViewModels.CheckoutVm
{
    public class CheckoutCompleteVm
    {
        public BasketTotalVm Basket { get; set; }

        public string? AdditionalAddressInfo { get; set; }

        [Required(ErrorMessage = "Street is required.")]
        [StringLength(50, ErrorMessage = "Street must be between 1 and 50 characters long.")]
        public string? Street { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(50, ErrorMessage = "City must be between 1 and 50 characters long.")]
        public string? City { get; set; }

        [Required(ErrorMessage = "State is required.")]
        [StringLength(50, ErrorMessage = "State must be between 1 and 50 characters long.")]
        public string? State { get; set; }

        [Required(ErrorMessage = "Postal code is required.")]
        [StringLength(10, ErrorMessage = "Postal code must be between 1 and 10 characters long.")]
        public string? PostalCode { get; set; }

        [Required(ErrorMessage = "Cardholder name is required.")]
        [StringLength(50, ErrorMessage = "Cardholder name must be between 1 and 50 characters long.")]
        public string CardholderName { get; set; }

        [Required(ErrorMessage = "Card number is required.")]
        [StringLength(16, ErrorMessage = "Card number must be 16 digits long.")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Expiry month is required.")]
        [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12.")]
        public int ExpiryMonth { get; set; }

        [Required(ErrorMessage = "Expiry year is required.")]
        [Range(2022, 2050, ErrorMessage = "Expiry year must be between 2022 and 2050.")]
        public int ExpiryYear { get; set; }

        [Required(ErrorMessage = "CVV is required.")]
        [StringLength(3, ErrorMessage = "CVV must be 3 digits long.")]
        public string CVV { get; set; }

        [StringLength(15, ErrorMessage = "Phone number must be between 1 and 15 characters long.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        [EnumDataType(typeof(PaymentMethods))]
        public PaymentMethods PaymentMethod { get; set; }

    }
}
