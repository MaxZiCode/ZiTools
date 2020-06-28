using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ZiTools
{
	public class WindowController : ISeekController
	{
		private readonly ISeekModel _model;
		private readonly MainWindow _mainWindow;

		public bool IsWindowOpened => Find.WindowStack.IsOpen(_mainWindow.GetType());

		public WindowController(ISeekModel model)
		{
			this._model = model ?? throw new ArgumentNullException(nameof(model));
			_mainWindow = new MainWindow(this, _model);
		}

		public void ToggleWindow()
		{
			if (!IsWindowOpened)
			{
				Find.WindowStack.Add(_mainWindow);
			}
			else
			{
				Find.WindowStack.TryRemove(_mainWindow, false);
			}	
		}

		public void AddFavourite(ISearchItem item)
		{
			_model.AddFavourite(item);
		}

		public void ChangeActiveCategory(ICategory category)
		{
			if (category == null)
				return;

			if (_model.ActiveCategory == category)
			{
				_model.ActiveCategory = null;
			}
			else
			{
				_model.ActiveCategory = category;
			}
		}

		public void ChangeActiveSearchItem(ISearchItem item)
		{
			_model.ActiveSearchItem = item;
		}

		public void ChangeText(string text)
		{
			if (text != _model.SearchText)
				_model.SearchText = text;
		}

		public void RemoveFavourite(ISearchItem item)
		{
			_model.RemoveFavourite(item);
		}
	}
}
