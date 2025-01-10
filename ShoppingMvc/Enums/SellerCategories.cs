using System.ComponentModel;

namespace ShoppingMvc.Enums
{
    public enum SellerCategories
    {

        [Description("Electronics")]
        Electronic = 1,

        [Description("Shoes & Bags")]
        ShoesAndBags = 2,

        [Description("Watch & Glasses")]
        WatchAndGlasses = 3,

        [Description("Car & Motorcycle Accessories")]
        CarAndMotorcycleAccessories = 4,

        [Description("Sports & Outdoor")]
        SportsAndOutdoor = 5,

        [Description("Books & Stationery & Lifestyle")]
        BooksAndStationeryAndLifestyle = 6,

        [Description("Kids")]
        Kids = 7,

        [Description("Textile")]
        Textile = 8,

        [Description("Cosmetics")]
        Cosmetics = 9,

        [Description("Furniture")]
        Furniture = 10,

        [Description("Garden & Construction Market & Hardware")]
        GardenAndConstructionMarketAndHardware = 11,

        [Description("Luxury & Designer")]
        LuxuryAndDesigner = 12,

        [Description("Accessories")]
        Accessories = 13,

        [Description("Supermarket & Pet Shop & Health")]
        SupermarketAndPetShopAndHealth = 14,

        [Description("Home Textile, Decoration, Table & Kitchen")]
        HomeTextileDecorationTableAndKitchen = 15
    }
}
