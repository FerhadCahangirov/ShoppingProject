using ShoppingMvc.Models.Identity;

namespace ShoppingMvc.ViewModels.UserVm
{
    public class UserListItemVm
    {
        public AppUser User { get; set; }
        public string RoleName { get; set; }   

        public UserListItemVm()
        {
            User = new AppUser();
            RoleName = string.Empty;
        }
    }
}
