using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMvc.Models.Identity
{
    public class AppUser : IdentityUser<int>
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? ProfileImageUrl { get; set; }

        public override string? PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }

        public List<Comment>? Comments { get; set; }
        public List<Reply>? ReplyComments { get; set; }
        public List<Basket>? Baskets { get; set; }

        public AppUser()
        {
            Comments = new List<Comment>();
            ReplyComments = new List<Reply>();
            Baskets = new List<Basket>();
        }
    }
}
