namespace ShoppingMvc.Models
{
    public class Slider:BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public string Discount { get; set; }
        public string Button { get; set; }

    }
}
