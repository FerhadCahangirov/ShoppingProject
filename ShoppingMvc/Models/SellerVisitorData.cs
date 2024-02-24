namespace ShoppingMvc.Models
{
    public class SellerVisitorData : VisitorData
    {
        public SellerData SellerData { get; set; }
        public SellerVisitorData()
        {
            SellerData = new SellerData();
        }
    }
}
