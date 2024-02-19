using ShoppingMvc.Models.Identity;

namespace ShoppingMvc.ViewModels.AuthVm
{
    public class ProfileSettingsVm
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? ProfileImageFile { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    public static class ProfileSettingsConvertion
    {
        public static ProfileSettingsVm FromAppUser_ToProfileSettingsVm(this AppUser user)
        {
            return new ProfileSettingsVm()
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
