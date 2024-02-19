using ShoppingMvc.Models.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMvc.Models
{
    public class Basket: BaseEntity
    {
        public bool IsOrdered { get; set; }
        [Column(TypeName = "smallmoney")]

        public List<BasketItem> BasketItems { get; set; }
        public AppUser User { get; set; }
    }
}
