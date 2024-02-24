using ShoppingMvc.Models.Identity;

namespace ShoppingMvc.Models
{
    public class VisitorData : BaseEntity
    {
        public string? HostAddress { get; set; }
        public AppUser? User { get; set; }

        public VisitorData()
        {
            User = null;
        }
    }
}
