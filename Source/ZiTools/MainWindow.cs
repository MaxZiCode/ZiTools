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

		private ISeekModel _model;
		private ISeekController _controller;

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
				y = UI.screenHeight - InitialSize.y - 148f,
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
			DrawSelect(new Rect());
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
			Rect catRect = new Rect()
			{
				width = catRectSide,
				height = catRectSide
			};

			List<Rect> categories = Enumerable.Range(0, 5).Select(i => new Rect(catRect) { x = (catRect.width + gap) * i }).ToList();

			Rect groupRect = new Rect()
			{
				width = categories.Last().xMax,
				height = catRect.height
			};

			Widgets.BeginScrollView(faceRect, ref _categoryScrollPosition, groupRect);
			GUI.BeginGroup(groupRect);

			for (int i = 0; i < categories.Count; i++)
			{
				Rect contrCtg = categories[i].ContractedBy(2f);
				ICategory category = _model.Categories[0];
				bool selected = category == _activeCategory;
				if (SimpleButton(contrCtg, selected))
				{
					ICategory selectedCtg = selected ? null : category;
					_controller.ChangeActiveCategory(selectedCtg);
				}
			}

			GUI.EndGroup();
			Widgets.EndScrollView();
		}

		private void DrawResults(Rect inRect)
		{
			Widgets.DrawBoxSolid(inRect, Color.green);
		}

		private void DrawSelect(Rect inRect)
		{
			Widgets.DrawBoxSolid(inRect, Color.grey);
		}

		public void AfterUpdateText()
		{
			_text = _model.SearchText;
		}

		public void AfterUpdateSearchItem()
		{
			_activeCategory = _model.ActiveCategory;
		}

		public void AfterUpdateCategory()
		{
			_activeSearchItem = _model.ActiveSearchItem;
		}

		private bool SimpleButton(Rect rect, bool selected)
		{
			Widgets.DrawOptionBackground(rect, selected);
			return Widgets.ButtonInvisible(rect);
		}
	}
}
