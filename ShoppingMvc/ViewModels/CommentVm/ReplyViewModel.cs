using System.ComponentModel.DataAnnotations;

namespace ShoppingMvc.ViewModels.CommentVm
{
    public class ReplyViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Message { get; set; }
        public DateTime PostedDate { get; set; }
        public int ProductId { get; set; }
        public int CommentId { get; set; }
    }
}
