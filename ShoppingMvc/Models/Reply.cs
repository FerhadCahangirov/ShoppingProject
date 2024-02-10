namespace ShoppingMvc.Models
{
    public class Reply:BaseEntity
    {
        
        public int CommentId { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public Comment Comment { get; set; }
    }
}
