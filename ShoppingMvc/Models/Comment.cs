namespace ShoppingMvc.Models
{
    public class Comment : BaseEntity
    {
        public string UserName { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }

        public List<Reply> Replies { get; set; }
        public AppUser User { get; set; }
        public Product Product { get; set; }
    }
}
