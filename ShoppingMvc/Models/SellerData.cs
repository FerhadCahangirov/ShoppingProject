﻿using ShoppingMvc.Enums;
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
        public string? ShopPostalCode { get; set; }
        public string? ShopLogoUrl { get; set; }
        public string? ThumbnailImageUrl { get; set; }

        public SellerCategories ShopCategory { get; set; }

        public bool IsBanned { get; set; } = false;

        public IEnumerable<Product>? Products { get; set; }
        public IEnumerable<OrderTracking> OrderTrackings { get; set; }
        public IEnumerable<SellerVisitorData> SellerVisitorDatas {  get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Tag> Tags { get; set; }

        public AppUser Seller { get; set; }

        public SellerData()
        {
            Products = new List<Product>();
            OrderTrackings = new List<OrderTracking>();
            Seller = new AppUser();
            SellerVisitorDatas = new List<SellerVisitorData>();
            Categories = new List<Category>();
            Tags = new List<Tag>();
        }
    }
}
