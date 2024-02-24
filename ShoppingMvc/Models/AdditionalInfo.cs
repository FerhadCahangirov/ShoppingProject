namespace ShoppingMvc.Models
{
    public class AdditionalInfo : BaseEntity
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public Product Product { get; set; }

        public AdditionalInfo()
        {
            Key = string.Empty;
            Value = string.Empty;
            Product = new Product();
        }
    }
}
