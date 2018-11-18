using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

using Verse;
using Verse.Profile;
using RimWorld;
using Harmony;
using UnityEngine;

namespace ZiTools
{
	[StaticConstructorOnStartup]
	public static class StaticConstructor
	{
		static StaticConstructor()
		{
			var harmony = HarmonyInstance.Create("rimworld.maxzicode.zitools.mainconstructor");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		public static readonly Dictionary<ObjectSeeker_Data.CategoryOfObjects, Texture2D> TexturesOfCategoriesDict = new Dictionary<ObjectSeeker_Data.CategoryOfObjects, Texture2D>
		{
			{ ObjectSeeker_Data.CategoryOfObjects.Favorites,  ContentFinder<Texture2D>.Get("UI/Favourite Button", true) },
			{ ObjectSeeker_Data.CategoryOfObjects.All,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) },
			{ ObjectSeeker_Data.CategoryOfObjects.Buildings,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) },
			{ ObjectSeeker_Data.CategoryOfObjects.Terrains,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) },
			{ ObjectSeeker_Data.CategoryOfObjects.Plants,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) },
			{ ObjectSeeker_Data.CategoryOfObjects.Pawns,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) },
			{ ObjectSeeker_Data.CategoryOfObjects.Corpses,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) },
			{ ObjectSeeker_Data.CategoryOfObjects.Other,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) }
		};

		public static void LogDebug(string msg)
		{
			Log.Message("[ZiTools] " + msg);
		}

		#region Patches
		[HarmonyPatch(typeof(Game), "CurrentMap", MethodType.Setter)]
		class Patch_CurrentMap
		{
			static void Postfix()
			{
				if (Find.WindowStack.IsOpen(typeof(ObjectSeeker_Window)) && Current.ProgramState == ProgramState.Playing)
				{
					ObjectSeeker_Window.Update();
				}
			}
		}

		[HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls", MethodType.Normal)]
		class Patch_DoPlaySettingsGlobalControls
		{
			static void Postfix(WidgetRow row, bool worldView)
			{
				if (!worldView)
				{
					bool isSelected = Find.WindowStack.IsOpen(typeof(ObjectSeeker_Window));
					row.ToggleableIcon(ref isSelected, ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true), "ZiT_ObjectsSeekerDesc".Translate(), SoundDefOf.Mouseover_ButtonToggle);
					if (isSelected != Find.WindowStack.IsOpen(typeof(ObjectSeeker_Window)))
					{
						if (!Find.WindowStack.IsOpen(typeof(ObjectSeeker_Window)))
							ObjectSeeker_Window.DrawWindow();
						else
							Find.WindowStack.TryRemove(typeof(ObjectSeeker_Window), false);
					}
				}
			}
		}

		[HarmonyPatch(typeof(MemoryUtility), "ClearAllMapsAndWorld", MethodType.Normal)]
		class Patch_ClearAllMapsAndWorld
		{
			static void Postfix()
			{
				ObjectSeeker_Window.ClearUpdateAction();
			}
		}
		#endregion Patches
	}
}
