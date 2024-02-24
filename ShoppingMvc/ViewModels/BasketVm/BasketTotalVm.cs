namespace ShoppingMvc.ViewModels.BasketVm
{
    public class BasketTotalVm
    {
        public IEnumerable<BasketProductItemVm> Items { get; set; }
        public string TotalPrice { get; set; }
        public string SubTotalPrice { get; set; }
        public string TotalShippingFee { get; set; }
    }
}
