using System.ComponentModel.DataAnnotations;

namespace ShoppingMvc.ViewModels.AuthVm
{
    public class RegisterVm
    {
        [Required(ErrorMessage ="Invalid Name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Invalid Surname")]
        public string Surname { get; set; }
        [Required(ErrorMessage = "Invalid Username"),MaxLength(32)]
        public string Username { get; set; }
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required, DataType(DataType.Password),Compare(nameof(PasswordConfirm))]
        public string Password { get; set; }
        [Required, DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }
    }
}
