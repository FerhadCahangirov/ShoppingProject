using System.ComponentModel.DataAnnotations;

namespace ShoppingMvc.ViewModels.CategoryVm
{
    public class CategoryUpdateVm
    {
        [Required]
        public int Id { get; set; }
        [Required, MaxLength(16)]
        public string Name { get; set; }
        public bool IsArchived { get; set; }
    }
}
