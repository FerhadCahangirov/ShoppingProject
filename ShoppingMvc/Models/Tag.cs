
namespace ShoppingMvc.Models
{
    public class Tag : BaseEntity
    {
        private string _title;
        public SellerData Seller { get; set; }

        public string Title
        {
            get { return _title; }
            set
            {
                if (int.TryParse(value, out int intValue))
                {
                    _title = intValue.ToString();
                }
                else
                {
                    _title = value;
                }
            }
        }

        public IEnumerable<Product>? Products { get; set; }

        public Tag()
        {
            Products = new List<Product>();
            Seller = new SellerData();
        }
    }
}
