namespace ShoppingMvc.Models
{
    public class ProductVisitorData : VisitorData
    {
        public Product Product { get; set; }
        public ProductVisitorData() 
        { 
            Product = new Product();
        }
    }
}
