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
	public sealed class MainWindow : Window
	{
		private Rect _positionRect;
		private Vector2 _initialSize = new Vector2(200f, 280f);
		private Vector2 _categoryScrollPosition = new Vector2();
		private string _text;

		public override Vector2 InitialSize => _initialSize;

		public MainWindow() : base()
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
		}

		protected override void SetInitialSizeAndPosition() => windowRect = _positionRect;

		public override void PreOpen()
		{
			base.PreOpen();
		}

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

		public override void PreClose()
		{
			base.PreClose();
		}

		private void DrawSearch(Rect inRect)
		{
			_text = Widgets.TextField(inRect, _text);
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

			foreach (var cat in categories)
			{
				Rect contrCat = cat.ContractedBy(2f);
				Widgets.DrawOptionBackground(contrCat, IsSelected(contrCat));
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

		private bool IsSelected(Rect rect) => Mouse.IsOver(rect);
	}
}
