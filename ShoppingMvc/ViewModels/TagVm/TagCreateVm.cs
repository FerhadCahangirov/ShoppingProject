namespace ShoppingMvc.ViewModels.TagVm
{
	public class TagCreateVm
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
	}
}
