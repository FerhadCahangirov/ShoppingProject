using Microsoft.AspNetCore.Identity;

namespace ShoppingMvc.Models.Identity
{
    public class AppRole : IdentityRole<int>
    {
        public AppRole() : base() { }

        public AppRole(string roleName) : base(roleName) { }
    }
}
