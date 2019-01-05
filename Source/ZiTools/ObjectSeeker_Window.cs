using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

using static ZiTools.StaticConstructor;
using static ZiTools.ObjectsDatabase;

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
			ODB.Clear();
			ODB.Update();
		}

		public override void DoWindowContents(Rect inRect)
		{
			float textFieldH = 35f, buttonWidth = 50f, buttonHeigth = 50f, buttonX = inRect.xMax - 2f * (buttonWidth + 1f);
			Text.Font = GameFont.Medium;
			Rect titleRect = new Rect(inRect) { height = Text.LineHeight + 7f };
			Rect textFieldRect = new Rect(titleRect.x, titleRect.yMax, buttonX - textFieldH - 5f, textFieldH);
			Rect updateButtonRect = new Rect(textFieldRect.xMax + 2f, textFieldRect.y, textFieldH, textFieldH);

			Widgets.Label(titleRect, "ZiT_ObjectsSeekerLabel".Translate());
			_text = Widgets.TextField(textFieldRect, _text);
			if (Widgets.ButtonImageWithBG(updateButtonRect, ContentFinder<Texture2D>.Get("UI/Update Button", true)))
			{
				ODB.Update();
				SoundDefOf.PageChange.PlayOneShotOnCamera();
			}
			TooltipHandler.TipRegion(updateButtonRect, "ZiT_UpdateButtonLabel".Translate());
			MouseoverSounds.DoRegion(updateButtonRect, SoundDefOf.Mouseover_Category);
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

			float objCount = string.IsNullOrEmpty(_text) ? // TODO: change search
				ODB.NamesInSelectedCategory.Count :
				(ODB.NamesInSelectedCategory.FindAll(i => i.Contains(_text))).Count;
			Rect rect1 = new Rect(0.0f, 0.0f, mainRect.width - 16f, (objCount + 1) * lineHeight);
			curY = rect1.y;
			Widgets.BeginScrollView(mainRect, ref _scrollPosition, rect1, true);
			GUI.BeginGroup(rect1);
			string favChange = null;
			this.GroupOfThingsMaker(new Rect(rect1.x, curY, rect1.width, lineHeight), "ZiT_NameLabel".Translate(), ODB.SelectedCategory == CategoryOfObjects.Corpses ? "ZiT_TimeUntilRotted".Translate() : "ZiT_CellsCountLabel".Translate(), ref favChange, false);
			curY += lineHeight;
			foreach (string currentName in ODB.NamesInSelectedCategory)
			{
				if (string.IsNullOrEmpty(_text) || currentName.Contains(_text))
				{ 
					this.GroupOfThingsMaker(new Rect(rect1.x, curY, rect1.width, lineHeight), currentName, ODB.GetParameter(currentName), ref favChange);
					curY += lineHeight;
				}
			}
			List<string> list = ODB.NamesInFavourites;
			if (!String.IsNullOrEmpty(favChange))
			{
				if (!list.Contains(favChange))
					list.Add(favChange);
				else
					list.Remove(favChange); 
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

		void GroupOfThingsMaker(Rect inRect, string label, string param, ref string favChange, bool createFindButton = true)
		{
			Rect rectImage = new Rect(inRect.x, inRect.y, inRect.height, inRect.height);
			Rect rectLabel = new Rect(inRect.x, inRect.y, inRect.width - inRect.height, inRect.height);
			Rect rectFavButton = new Rect(rectLabel.xMax, inRect.y, inRect.height, inRect.height);
			if (createFindButton)
			{
				if (label == ODB.NameToSeek)
					Widgets.DrawHighlightSelected(rectLabel);
				else
					Widgets.DrawHighlightIfMouseover(rectLabel);
				ODB.DrawIcon(label, rectImage);
				if (Widgets.ButtonInvisible(rectLabel))
				{
					if (ODB.NameToSeek != label)
					{
						ODB.NameToSeek = label;
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
				
				if (Mouse.IsOver(rectFavButton) || ODB.NamesInFavourites.Contains(label))
				{
					if (Widgets.ButtonImage(rectFavButton.ScaledBy(0.85f), ODB.GetCategoryTexture(CategoryOfObjects.Favorites)))
					{
						favChange = label;
						SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
					}
				}
			}
			rectLabel.xMin = rectImage.xMax + 2f;
			TooltipHandler.TipRegion(rectLabel, label);
			Rect rectParam = new Rect(inRect.width - Text.CalcSize(param).x, inRect.y, Text.CalcSize(param).x, inRect.height);
			Widgets.Label(rectLabel.RightPartPixels(rectParam.width), param);

			rectLabel.width -= rectParam.width;
			Widgets.Label(rectLabel.LeftPartPixels(rectLabel.width), label);
		}
	}
}
