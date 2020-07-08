using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.Noise;

namespace ZiTools
{
	public interface ISeekModel
	{
		List<Category> Categories { get; }

		Category ActiveCategory { get; set; }

		IEnumerable<ISearchItem> SearchItems { get; }

		ISearchItem ActiveSearchItem { get; set; }

		string SearchText { get; set; }

		void Initialize();

		void UpdateAllItems();

		void UpdateSearchItems();

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
