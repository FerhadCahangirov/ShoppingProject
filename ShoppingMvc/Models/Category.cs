namespace ShoppingMvc.Models
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public SellerData Seller { get; set; }

        public Category()
        {
            Name = string.Empty;
            Products = new List<Product>();
            Seller = new SellerData();
        }
    }
}
