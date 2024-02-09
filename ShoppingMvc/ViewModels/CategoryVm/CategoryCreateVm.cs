using System.ComponentModel.DataAnnotations;

namespace ShoppingMvc.ViewModels.CategoryVm
{
    public class CategoryCreateVm
    {
        [Required,MaxLength(16)]
        public string Name { get; set; }
    }
}
