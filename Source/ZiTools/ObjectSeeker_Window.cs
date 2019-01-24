using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace ZiTools
{
	public class ObjectSeeker_Window : Window
	{
		public ObjectSeeker_Window() : base()
		{
			this.doCloseX = true;
			this.preventDrawTutor = true;
			this.draggable = true;
			this.preventCameraMotion = false;
			this.closeOnAccept = false;

			this.ODB = ZiTools_GameComponent.GetObjectsDatabase();
		}

		Vector2 _scrollPosition = new Vector2();
		string _text;

		public ObjectsDatabase ODB { get; private set; }

		protected override void SetInitialSizeAndPosition() => this.windowRect = new Rect(UI.screenWidth - this.InitialSize.x, UI.screenHeight - this.InitialSize.y - 150f, this.InitialSize.x, this.InitialSize.y);

		public override void PreOpen()
		{
			base.PreOpen();
			ODB.Update();
			ODB.Clear();
		}

		public override void DoWindowContents(Rect inRect)
		{
			float textFieldH = 35f, buttonWidth = 50f, buttonHeigth = 50f, buttonX = inRect.xMax - 2f * (buttonWidth + 1f);
			Text.Font = GameFont.Medium;
			Rect titleRect = new Rect(inRect) { height = Text.LineHeight + 7f };
			Rect textFieldRect = new Rect(titleRect.x, titleRect.yMax, buttonX - textFieldH - 5f, textFieldH);
			Rect updateButtonRect = new Rect(textFieldRect.xMax + 2f, textFieldRect.y, textFieldH, textFieldH);

			Widgets.Label(titleRect, "ZiT_ObjectsSeekerLabel".Translate());
			if (Widgets.ButtonImageWithBG(updateButtonRect, ContentFinder<Texture2D>.Get("UI/Update Button", true)))
			{
				ODB.Update();
				SoundDefOf.PageChange.PlayOneShotOnCamera();
			}
			TooltipHandler.TipRegion(updateButtonRect, "ZiT_UpdateButtonLabel".Translate());
			MouseoverSounds.DoRegion(updateButtonRect, SoundDefOf.Mouseover_Category);
			_text = Widgets.TextField(textFieldRect, _text); //it's here for bigger font
			Text.Font = GameFont.Small;

			float lineHeight = Text.LineHeight;
			Rect catButtRect = new Rect(buttonX, inRect.yMax - (buttonHeigth + 1f) * 4f, buttonWidth, buttonHeigth);
			Vector2 categorySize = Text.CalcSize(ODB.SelectedCategoryName);
			Widgets.Label(new Rect(catButtRect.x + (inRect.xMax - catButtRect.x - categorySize.x) / 2f, (catButtRect.y - categorySize.y) / 2f, categorySize.x, categorySize.y), ODB.SelectedCategoryName);

			for (int i = 0; i < 8; i++) //categories tab
			{
				CategoryOfObjects currentCategory = ODB.GetCategoryViaInt(i);
				if (ODB.SelectedCategory == currentCategory)
				{
					GUI.color = Widgets.WindowBGFillColor;
					GUI.DrawTexture(catButtRect, BaseContent.BlackTex);
					GUI.color = new Color(1f, 0.9f, 0f);
					Widgets.DrawBox(catButtRect, 3);
					GUI.color = Color.white;
				}
				else
				{
					GUI.color = Widgets.WindowBGFillColor;
					GUI.DrawTexture(catButtRect, BaseContent.WhiteTex);
					GUI.color = Color.white;
					Widgets.DrawBox(catButtRect, 2);
				}

				if (Widgets.ButtonImage(catButtRect.ScaledBy(0.85f), ODB.GetCategoryTexture(currentCategory)))
				{
					ODB.SelectedCategory = currentCategory;
					SoundDefOf.Click.PlayOneShotOnCamera();
				}
				TooltipHandler.TipRegion(catButtRect, ODB.NamesOfCategoriesDict[currentCategory]);
				MouseoverSounds.DoRegion(catButtRect, SoundDefOf.Mouseover_Category);
				catButtRect.x = catButtRect.xMax + 1f;
				if (i % 2 == 1)
				{
					catButtRect.x = buttonX;
					catButtRect.y += catButtRect.height + 1f;
				}
			}

			float curY = textFieldRect.yMax;
			Rect mainRect = new Rect(inRect) { yMin = curY, xMax = catButtRect.x };
			if (!ODB.IsSelectedCategoryHaveObjects())
			{
				Widgets.Label(mainRect, "ZiT_NotFoundString".Translate(ODB.SelectedCategoryName));
				return;
			}

			List<DBUnit> units = ODB.GetUnitsByWord(_text);
			Rect rect1 = new Rect(0.0f, 0.0f, mainRect.width - 16f, (units.Count + 1) * lineHeight);
			curY = rect1.y;
			Widgets.BeginScrollView(mainRect, ref _scrollPosition, rect1, true);
			GUI.BeginGroup(rect1);

			DBUnit favChange = null;
			DBUnit cusomUnit = new DBUnit("ZiT_NameLabel".Translate())
			{
				Parameter = ODB.SelectedCategory == CategoryOfObjects.Corpses ? "ZiT_TimeUntilRotted".Translate() : "ZiT_CellsCountLabel".Translate()
			};
			Rect position = new Rect(rect1.x, curY, rect1.width, lineHeight);

			this.DrawObjectsList(position, cusomUnit, ref favChange, true);
			position.y += lineHeight;
			foreach (var unit in units)
			{
				this.DrawObjectsList(position, unit, ref favChange);
				position.y += lineHeight;
			}
			List<DBUnit> favList = ODB.UnitsInFavourites;
			if (favChange != null)
			{
				if (!favList.Contains(favChange))
					favList.Add(favChange);
				else
					favList.Remove(favChange);
			}
			GUI.EndGroup();
			Widgets.EndScrollView();
		}

		public override void PreClose()
		{
			base.PreClose();
			ODB.Clear();
		}

		public override Vector2 InitialSize { get => new Vector2(400f, 280f); }

		public static void DrawWindow() => Find.WindowStack.Add(new ObjectSeeker_Window());

		void DrawObjectsList(Rect inRect, DBUnit unit, ref DBUnit favChange, bool onlyLabel = false)
		{
			string label = unit.Label;
			string param = unit.Parameter;

			Rect rectImage = new Rect(inRect.x, inRect.y, inRect.height, inRect.height);
			Rect rectParam = new Rect(inRect.width - Text.CalcSize(param).x - rectImage.width, inRect.y, Text.CalcSize(param).x, inRect.height);
			Rect rectLabel = new Rect(rectImage.xMax + 2f, inRect.y, inRect.width - 2 * rectImage.width - rectParam.width, inRect.height);
			Rect rectFavButton = new Rect(rectImage) { x = rectParam.xMax };
			Rect rectSearchButton = new Rect(inRect) { width = inRect.width - rectFavButton.width };
			
			Widgets.Label(rectLabel.LeftPartPixels(rectLabel.width), label);
			Widgets.Label(rectParam.RightPartPixels(rectParam.width), param);
			
			if (!onlyLabel)
			{
				TooltipHandler.TipRegion(rectLabel, label);
				if (unit == ODB.UnitToSeek)
					Widgets.DrawHighlightSelected(rectSearchButton);
				else
					Widgets.DrawHighlightIfMouseover(rectSearchButton);

				if (Widgets.ButtonInvisible(rectSearchButton))
				{
					if (unit != ODB.UnitToSeek)
					{
						ODB.UnitToSeek = unit;
						MapMarksManager.SetMarks(MapMarksManager.ObjectSeeker_MarkDef, ODB.Positions);
						ObjectsDatabase.DoUpdateAction();
						SoundDefOf.Designate_PlanAdd.PlayOneShotOnCamera();
					}
					else
					{
						ODB.Clear();
						SoundDefOf.Designate_PlanRemove.PlayOneShotOnCamera();
					}
				}
				unit.Icon.DrawIcon(rectImage);
				if (Mouse.IsOver(rectFavButton) || ODB.UnitsInFavourites.Contains(unit))
				{
					if (Widgets.ButtonImage(rectFavButton.ScaledBy(0.85f), ODB.GetCategoryTexture(CategoryOfObjects.Favorites)))
					{
						favChange = unit;
						SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
					}
				} 
			}
		}
	}
}
