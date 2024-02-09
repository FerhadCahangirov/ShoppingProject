using ShoppingMvc.Models.Cards;

namespace ShoppingMvc.Models.Categories
{
    public class Category:BaseEntity
    {
        public string Name { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }
}
