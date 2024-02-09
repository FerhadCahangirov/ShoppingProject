using ShoppingMvc.Models.Cards;

namespace ShoppingMvc.Models
{
    public class Comment:BaseEntity
    {
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime PostedDate { get; set; }
        public List<Reply> Replies { get; set; }

        public AppUser User {  get; set; }
        public Product Product { get; set; }
    }
}
