using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;

namespace ZiTools
{
	public class SeekModel : ISeekModel
	{
		private List<ISearchItem> _searchItems = new List<ISearchItem>();
		private List<ITextObserver> _textObservers = new List<ITextObserver>();
		private List<ICategoryObserver> _categoryObservers = new List<ICategoryObserver>();
		private List<ISearchItemObserver> _searchItemObservers = new List<ISearchItemObserver>();

		public IList<ICategory> Categories { get; } = new List<ICategory>();
		public ICategory ActiveCategory { get; set; }
		public IEnumerable<ISearchItem> SearchItems { get => _searchItems; }
		public ISearchItem ActiveSearchItem { get; set; }
		public string SearchText { get; set; }

		public void AddFavourite(ISearchItem item)
		{
			Log.Message("AddFavourite");
		}

		public void RemoveFavourite(ISearchItem item)
		{
			Log.Message("RemoveFavourite");
		}

		public void Initialize()
		{
			
		}

		public void RegisterObserver(ITextObserver textObserver)
		{
			_textObservers.Add(textObserver);
		}

		public void RegisterObserver(ICategoryObserver categoryObserver)
		{
			_categoryObservers.Add(categoryObserver);
		}

		public void RegisterObserver(ISearchItemObserver searchItemObserver)
		{
			_searchItemObservers.Add(searchItemObserver);
		}

		public void RemoveObserver(ITextObserver textObserver)
		{
			_textObservers.Remove(textObserver);
		}

		public void RemoveObserver(ICategoryObserver categoryObserver)
		{
			_categoryObservers.Remove(categoryObserver);
		}

		public void RemoveObserver(ISearchItemObserver searchItemObserver)
		{
			_searchItemObservers.Remove(searchItemObserver);
		}

		public void UpdateItems()
		{
			throw new NotImplementedException();
		}
	}
}
