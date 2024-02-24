using ShoppingMvc.Models;
using ShoppingMvc.Models.Identity;
using ShoppingMvc.ViewModels.OrderVm;

namespace ShoppingMvc.ViewModels.AuthVm
{
    public class AccountVm
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? ProfileImageFile { get; set; }
        public string? ProfileImageUrl { get; set; }


        public IEnumerable<CustomerOrderListItemVm> Orders { get; set; }

        public AccountVm()
        {
            Orders = new List<CustomerOrderListItemVm>();
        }
    }

    public static class ProfileSettingsConvertion
    {
        public static AccountVm FromAppUser_ToAccountVm(this AppUser user)
        {
            return new AccountVm()
            {
                Name = user.Name,
                Surname = user.Surname,
                UserName = user.UserName,   
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
            };
        }
    }
}
