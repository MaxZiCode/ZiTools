using System.Collections.Generic;

namespace ZiTools
{
	public abstract class Category
	{
		public string Label { get; set; }

		public abstract IEnumerable<ISearchItem> GetFilteredItems(IEnumerable<ISearchItem> searchItems);
	}
}