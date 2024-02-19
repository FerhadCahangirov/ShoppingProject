namespace ShoppingMvc.Models
{
    public class BasketItem : BaseEntity
    {
        public int Count { get; set; }
        public Product Product { get; set; }
        public Basket Basket { get; set; }
    }
}
