using ShoppingMvc.Models.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMvc.Models
{
    public class Basket: BaseEntity
    {
        public bool IsOrdered { get; set; }

        public List<BasketItem> BasketItems { get; set; }
        public AppUser User { get; set; }

        public Basket()
        {
            BasketItems = new List<BasketItem>();
            User = new AppUser();
        }
    }
}
