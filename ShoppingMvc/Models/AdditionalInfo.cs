namespace ShoppingMvc.Models
{
    public class AdditionalInfo : BaseEntity
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public Product Product { get; set; }
    }
}
