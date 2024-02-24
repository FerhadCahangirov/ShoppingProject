namespace ShoppingMvc.Models
{
    public class BasketItem : BaseEntity
    {
        public int Count { get; set; }
        public Product Product { get; set; }
        public Basket Basket { get; set; }

        public BasketItem()
        {
            Count = 0;
            Product = new Product();
            Basket = new Basket();
        }
    }
}
