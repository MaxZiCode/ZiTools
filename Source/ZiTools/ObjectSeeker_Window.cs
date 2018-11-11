using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

using static ZiTools.StaticConstructor;

namespace ZiTools
{
	public class ObjectSeeker_Window : Window
	{
		Vector2 ScrollPosition;
		string _text;

		static Action UpdateAction = delegate { };

		public ObjectSeeker_Window() : base()
		{
			this.doCloseX = true;
			this.preventDrawTutor = true;
			this.draggable = true;
			this.preventCameraMotion = false;
			this.closeOnAccept = false; 

			this.ScrollPosition = new Vector2();
		}

		protected override void SetInitialSizeAndPosition() => this.windowRect = new Rect(UI.screenWidth - this.InitialSize.x, UI.screenHeight - this.InitialSize.y - 150f, this.InitialSize.x, this.InitialSize.y);

		public override void PreOpen()
		{
			base.PreOpen();
			Clear();
			Update();
		}

		public override void DoWindowContents(Rect inRect)
		{
			float textFieldH = 35f, sqSize = 50f, lightRectGap = 10f, buttonX = inRect.xMax - 2f * (lightRectGap + sqSize);
			Text.Font = GameFont.Medium;
			Rect titleRect = new Rect(inRect) { height = Text.LineHeight + 7f };
			Rect textFieldRect = new Rect(titleRect.x, titleRect.yMax, buttonX - textFieldH - 5f, textFieldH);
			Rect updateButtonRect = new Rect(textFieldRect.xMax + 2f, textFieldRect.y, textFieldH, textFieldH);

			Widgets.Label(titleRect, "ZiT_ObjectsSeekerLabel".Translate());
			_text = Widgets.TextField(textFieldRect, _text);
			if (Widgets.ButtonImageWithBG(updateButtonRect, ContentFinder<Texture2D>.Get("UI/Update Button", true)))
			{
				Update();
				SoundDefOf.PageChange.PlayOneShotOnCamera();
			}
			TooltipHandler.TipRegion(updateButtonRect, "ZiT_UpdateButtonLabel".Translate());
			MouseoverSounds.DoRegion(updateButtonRect, SoundDefOf.Mouseover_Category);
			Text.Font = GameFont.Small;
			
			Rect lightRect = new Rect(buttonX, inRect.yMax - sqSize * 4f, lightRectGap, sqSize);
			Rect selectButtonRect = new Rect(lightRect.xMax, lightRect.y, sqSize, sqSize);
			Vector2 categorySize = Text.CalcSize(OSD_Global.SelectedCategoryName);
			Widgets.Label(new Rect(lightRect.x + (inRect.xMax - lightRect.x - categorySize.x) / 2f, (lightRect.y - categorySize.y) / 2f, categorySize.x, categorySize.y), OSD_Global.SelectedCategoryName);
			for (int i = 0; i < 8; i++)
			{
				ObjectSeeker_Data.CategoryOfObjects currentCategory = (ObjectSeeker_Data.CategoryOfObjects)Enum.Parse(typeof(ObjectSeeker_Data.CategoryOfObjects), i.ToString());
				Widgets.DrawWindowBackground(lightRect);
				if (Widgets.ButtonImage(selectButtonRect, ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true)))
				{
					OSD_Global.SelectedCategory = currentCategory;
					SoundDefOf.Click.PlayOneShotOnCamera();
				}
				MouseoverSounds.DoRegion(selectButtonRect, SoundDefOf.Mouseover_Category);
				if (OSD_Global.SelectedCategory == currentCategory)
					Widgets.DrawHighlightSelected(lightRect);
				TooltipHandler.TipRegion(selectButtonRect, OSD_Global.NamesOfCategoriesDict[currentCategory]);
				lightRect.x = selectButtonRect.xMax;
				selectButtonRect.x = lightRect.xMax;
				if (i % 2 == 1)
				{
					lightRect.y += sqSize;
					lightRect.x = buttonX;
					selectButtonRect.y += sqSize;
					selectButtonRect.x = lightRect.xMax; 
				}
			}

			float curY = textFieldRect.yMax;
			Rect mainRect = new Rect(inRect) { yMin = curY, xMax = lightRect.x };
			if (!OSD_Global.CategoriesDict.ContainsKey(OSD_Global.SelectedCategory))
			{
				Widgets.Label(mainRect, "ZiT_NotFoundString".Translate(OSD_Global.SelectedCategoryName));
				return;
			}

			float objCount = string.IsNullOrEmpty(_text) ?
				OSD_Global.CategoriesDict[OSD_Global.SelectedCategory].Count :
				(OSD_Global.CategoriesDict[OSD_Global.SelectedCategory].FindAll(i => i.Contains(_text.ToLower()))).Count;
			Rect rect1 = new Rect(0.0f, 0.0f, mainRect.width - 16f, (objCount + 1) * Text.LineHeight);
			curY = rect1.y;
			Widgets.BeginScrollView(mainRect, ref ScrollPosition, rect1, true);
			GUI.BeginGroup(rect1);
			curY += this.GroupOfThingsMaker(rect1.x, curY, rect1.width, "ZiT_NameLabel".Translate(), OSD_Global.SelectedCategory == ObjectSeeker_Data.CategoryOfObjects.Corpses ? "ZiT_TimeUntilRotted".Translate() : "ZiT_CellsCountLabel".Translate(), false);

			OSD_Global.CategoriesDict[OSD_Global.SelectedCategory].Sort();
			if (OSD_Global.SelectedCategory == ObjectSeeker_Data.CategoryOfObjects.Corpses)
				OSD_Global.CategoriesDict[OSD_Global.SelectedCategory].Sort(OSD_Global);
			foreach (string currentName in OSD_Global.CategoriesDict[OSD_Global.SelectedCategory])
			{
				if (string.IsNullOrEmpty(_text) || currentName.Contains(_text.ToLower()))
				{
					string param = OSD_Global.SelectedCategory == ObjectSeeker_Data.CategoryOfObjects.Corpses ?
								(OSD_Global.CorpsesTimeRemainDict[currentName] > 0 ?
								OSD_Global.CorpsesTimeRemainDict[currentName].ToStringTicksToDays() :
								"-") :
								OSD_Global.LocationsDict[currentName]?.Count.ToString();
					curY += this.GroupOfThingsMaker(rect1.x, curY, rect1.width, currentName, param);
				}
			}
			GUI.EndGroup();
			Widgets.EndScrollView();
		}

		public override void PreClose()
		{
			base.PreClose();
			Clear();
		}

		public static void SetUpdateAction(Action action) => UpdateAction += action;

		public static void ClearUpdateAction() => UpdateAction = delegate { };

		public override Vector2 InitialSize { get => new Vector2(400f, 280f); }

		public static void DrawWindow() => Find.WindowStack.Add(new ObjectSeeker_Window());

		float GroupOfThingsMaker(float x, float y, float width, string label, string param, bool createFindButton = true)
		{
			Rect rectLabel = new Rect(x, y, width, Text.LineHeight);
			Rect rectImage = new Rect(x, y, Text.LineHeight, Text.LineHeight);
			rectLabel.xMin = rectImage.xMax + 2f;
			if (createFindButton)
			{
				if (OSD_Global.ThingsDict[label] != null)
					Widgets.ThingIcon(rectImage, OSD_Global.ThingsDict[label]);
				else if (OSD_Global.TerrainDefDict.ContainsKey(label) && OSD_Global.TerrainDefDict?[label] != null)
				{
					Designator_Build desBuild = new Designator_Build(OSD_Global.TerrainDefDict[label]);
					GUI.color = desBuild.IconDrawColor;
					Widgets.DrawTextureFitted(rectImage, desBuild.icon, 1f, Vector2.one, desBuild.iconTexCoords, desBuild.iconAngle);
					GUI.color = Color.white;
				}

				if (label == OSD_Global.ThingToSeek)
					Widgets.DrawHighlightSelected(rectLabel);
				if (Widgets.ButtonInvisible(rectLabel))
				{
					if (OSD_Global.ThingToSeek != label)
					{
						OSD_Global.ThingToSeek = label;
						MapMarksManager.SetMarks(MapMarksManager.ObjectSeeker_MarkDef);
						UpdateAction();
						SoundDefOf.Designate_PlanAdd.PlayOneShotOnCamera(); 
					}
					else
					{
						Clear();
						SoundDefOf.Designate_PlanRemove.PlayOneShotOnCamera();
					}
#if DEBUG
					if (OSD_Global.ThingsDict[label] != null)
						DebugMessage(OSD_Global.ThingsDict[label].LabelCap);
#endif
				}
				else
					Widgets.DrawHighlightIfMouseover(rectLabel);
			}
			TooltipHandler.TipRegion(rectLabel, label);
			Rect rectParam = new Rect(width - Text.CalcSize(param).x, y, Text.CalcSize(param).x, Text.LineHeight);
			Widgets.Label(rectLabel.RightPartPixels(rectParam.width), param);

			rectLabel.width -= rectParam.width;
			Widgets.Label(rectLabel.LeftPartPixels(rectLabel.width), label);
			
			return rectLabel.height;
		}

		public static void Update()
		{
			OSD_Global.FindAllThings();
			MapMarksManager.SetMarks(MapMarksManager.ObjectSeeker_MarkDef);
			UpdateAction();
		}

		void Clear()
		{
			OSD_Global.ThingToSeek = string.Empty;
			MapMarksManager.RemoveMarks(MapMarksManager.ObjectSeeker_MarkDef);
			UpdateAction();
		}
	}
}
