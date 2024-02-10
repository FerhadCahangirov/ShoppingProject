namespace ShoppingMvc.Models
{
    public class ProductImage : ImageEntity
    {
        public bool IsPrimary { get; set; }
        public int ProductId { get; set; }

        public Product Product { get; set; }
    }
}
