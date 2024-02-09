namespace ShoppingMvc.ViewModels.SliderVm
{
    public class SliderCreateVm
    {
        public IFormFile MainImage { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Discount { get; set; }
        public string Button { get; set; }
    }
}
