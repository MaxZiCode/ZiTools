using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace ZiTools
{
	public sealed class MainWindow : Window, ITextObserver, ICategoryObserver, ISearchItemObserver
	{
		private Rect _positionRect;
		private Vector2 _initialSize = new Vector2(200f, 280f);
		private Vector2 _categoryScrollPosition = new Vector2();
		private string _text;
		private ICategory _activeCategory;
		private ISearchItem _activeSearchItem;

		private readonly ISeekModel _model;
		private readonly ISeekController _controller;

		public override Vector2 InitialSize => _initialSize;

		public MainWindow(ISeekController controller, ISeekModel model) : base()
		{
			this.doCloseX = false;
			this.preventDrawTutor = true;
			this.draggable = false;
			this.preventCameraMotion = false;
			this.closeOnAccept = false;
			this.focusWhenOpened = true;

			_positionRect = new Rect()
			{
				x = UI.screenWidth - InitialSize.x,
				y = UI.screenHeight - InitialSize.y - 150f,
				width = InitialSize.x,
				height = InitialSize.y
			};

			_controller = controller;
			_model = model;

			_model.RegisterObserver((ITextObserver)this);
			_model.RegisterObserver((ICategoryObserver)this);
			_model.RegisterObserver((ISearchItemObserver)this);


		}

		protected override void SetInitialSizeAndPosition() => windowRect = _positionRect;

		public override void DoWindowContents(Rect inRect)
		{
			Rect searchRect = new Rect(inRect)
			{
				height = 35f
			};
			DrawSearch(searchRect);

			Rect categoriesRect = new Rect(inRect)
			{
				y = searchRect.yMax,
				height = 35f + 16f
			};
			DrawCategories(categoriesRect);

			Rect resultsRect = new Rect(inRect)
			{
				yMin = categoriesRect.yMax,
				height = 200f
			};
			DrawResults(resultsRect);

			_positionRect.height = resultsRect.yMax;
			SetInitialSizeAndPosition();
		}

		private void DrawSearch(Rect inRect)
		{
			_text = Widgets.TextField(inRect, _text);
			_controller.ChangeText(_text);
		}

		private void DrawCategories(Rect inRect)
		{
			const float gap = 0f;

			Rect faceRect = new Rect(inRect);

			float catRectSide = faceRect.height - 16f;
			var catRects = Enumerable.Range(0, _model.Categories.Count).Select(i => new Rect() { width = catRectSide, height = catRectSide, x = (catRectSide + gap) * i });

			Rect groupRect = new Rect()
			{
				width = catRects.LastOrDefault().xMax,
				height = catRectSide
			};

			ICategory selectedCategory = null;
			var enumerator = catRects.GetEnumerator();

			Widgets.BeginScrollView(faceRect, ref _categoryScrollPosition, groupRect);
			GUI.BeginGroup(groupRect);

			for (int i = 0; enumerator.MoveNext(); i++)
			{
				Rect curRect = enumerator.Current.ContractedBy(2f);
				ICategory category = _model.Categories[i];
				bool selected = category == _activeCategory;
				if (SimpleButton(curRect, selected))
				{
					selectedCategory = category;
				}
			}

			GUI.EndGroup();
			Widgets.EndScrollView();

			_controller.ChangeActiveCategory(selectedCategory);
		}

		private void DrawResults(Rect inRect)
		{
			Widgets.DrawBoxSolid(inRect, Color.green);
		}

		public void AfterUpdateText()
		{
			_text = _model.SearchText;
		}

		public void AfterUpdateSearchItem()
		{
			_activeSearchItem = _model.ActiveSearchItem;
		}

		public void AfterUpdateCategory()
		{
			_activeCategory = _model.ActiveCategory;
		}

		private bool SimpleButton(Rect rect, bool selected)
		{
			Widgets.DrawOptionBackground(rect, selected);
			return Widgets.ButtonInvisible(rect);
		}
	}
}
