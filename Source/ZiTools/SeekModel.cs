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
		private readonly List<ISearchItem> _allItems = new List<ISearchItem>();
		private readonly List<ISearchItem> _searchItems = new List<ISearchItem>();
		private readonly List<ITextObserver> _textObservers = new List<ITextObserver>();
		private readonly List<ICategoryObserver> _categoryObservers = new List<ICategoryObserver>();
		private readonly List<ISearchItemObserver> _searchItemObservers = new List<ISearchItemObserver>();

		private string _text;
		private Category _activeCategory;
		private ISearchItem _activeSearchItem;

		public List<Category> Categories { get; } = new List<Category>();
		public Category ActiveCategory 
		{ 
			get => _activeCategory; 
			set
			{
				_activeCategory = value;
				UpdateSearchItems();
				NotifyCategoryObservers();
			}
		}
		public IEnumerable<ISearchItem> SearchItems { get => _searchItems; }
		public ISearchItem ActiveSearchItem 
		{ 
			get => _activeSearchItem;
			set
			{
				_activeSearchItem = value;
				NotifySearchItemObservers();
			} 
		}
		public string SearchText 
		{ 
			get => _text; 
			set
			{
				_text = value;
				UpdateSearchItems();
				NotifyTextObservers();
			}
		}

		private void NotifyTextObservers() => _textObservers.ForEach(to => to.AfterUpdateText());

		private void NotifyCategoryObservers() => _categoryObservers.ForEach(co => co.AfterUpdateCategory());

		private void NotifySearchItemObservers() => _searchItemObservers.ForEach(so => so.AfterUpdateSearchItem());

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
			Categories.AddRange(ItemFilterFactory.GetFilters());
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

		public void UpdateAllItems()
		{
			_allItems.Clear();
			_allItems.AddRange(SearchItemFactory.GetSearchItems(Current.Game.CurrentMap));
		}

		public void UpdateSearchItems()
		{
			_searchItems.Clear();

			// TODO: Added for tests, del later.
			UpdateAllItems();

			IEnumerable<ISearchItem> items = _allItems;
			if (_activeCategory != null)
			{
				items = _activeCategory.GetFilteredItems(items);
			}
			if (!string.IsNullOrEmpty(_text))
			{
				items = items.Where(i => i.Label.IndexOf(_text, 0, StringComparison.OrdinalIgnoreCase) != -1);
			}
			_searchItems.AddRange(items);
		}
	}
}
