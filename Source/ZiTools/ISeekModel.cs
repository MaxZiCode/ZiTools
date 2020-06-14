using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZiTools
{
	public interface ISeekModel
	{
		IList<ICategory> Categories { get; }

		ICategory ActiveCategory { get; set; }

		IEnumerable<ISearchItem> SearchItems { get; }

		ISearchItem ActiveSearchItem { get; set; }

		string SearchText { get; set; }

		void Initialize();

		void UpdateItems();

		void AddFavourite(ISearchItem item);

		void RemoveFavourite(ISearchItem item);

		void RegisterObserver(ITextObserver textObserver);

		void RemoveObserver(ITextObserver textObserver);

		void RegisterObserver(ICategoryObserver categoryObserver);

		void RemoveObserver(ICategoryObserver categoryObserver);

		void RegisterObserver(ISearchItemObserver searchItemObserver);

		void RemoveObserver(ISearchItemObserver searchItemObserver);
	}
}
