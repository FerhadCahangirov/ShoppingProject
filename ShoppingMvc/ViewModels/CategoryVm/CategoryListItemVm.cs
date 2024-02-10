using ShoppingMvc.Models;

namespace ShoppingMvc.ViewModels.CategoryVm
{
    public class CategoryListItemVm
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string Name { get; set; }
        public IEnumerable<Product>? Products { get; set; }
        public bool IsArchived { get; set; }
    }
}
