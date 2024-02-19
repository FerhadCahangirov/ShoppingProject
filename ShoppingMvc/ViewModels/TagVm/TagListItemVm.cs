using ShoppingMvc.Models;

namespace ShoppingMvc.ViewModels.TagVm
{
	public class TagListItemVm
	{
        private string _title;

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
        public int Id { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime? CreatedTime { get; set; }
		public DateTime? UpdatedTime { get; set; }
		public bool IsArchived { get; set; }
        public IEnumerable<Tag>? Tags { get; set; }
	}
}
