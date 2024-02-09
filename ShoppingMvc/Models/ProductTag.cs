using ShoppingMvc.Models.Cards;
using ShoppingMvc.Models.Tags;
using System.Reflection.Metadata;

namespace ShoppingMvc.Models
{
    public class ProductTag
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public int ProductId { get; set; }
        public Tag? Tag { get; set; }
        public  Product?  Product{ get; set; }
    }
}
