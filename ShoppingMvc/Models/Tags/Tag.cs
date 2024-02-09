using ShoppingMvc.Models.Cards;

namespace ShoppingMvc.Models.Tags
{
	public class Tag:BaseEntity
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
        public IEnumerable<ProductTag>? TagProduct { get; set; }
    }
}
