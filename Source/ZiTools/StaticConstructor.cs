using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

using Verse;
using Verse.Profile;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace ZiTools
{
	[StaticConstructorOnStartup]
	public static class StaticConstructor
	{
		private static bool s_hasModelInitialized;

		public static SeekModel SeekModel = new SeekModel();

		public static WindowController WindowController = new WindowController(SeekModel);

		static StaticConstructor()
		{
			Harmony harmony = new Harmony("rimworld.maxzicode.zitools.mainconstructor");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

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
					((ObjectSeeker_Window)Find.WindowStack.Windows.First(w => w is ObjectSeeker_Window)).ODB.Update();
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
					bool isSelected = WindowController.IsWindowOpened;
					row.ToggleableIcon(ref isSelected, ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true), "ZiT_ObjectsSeekerLabel".Translate(), SoundDefOf.Mouseover_ButtonToggle);
					if (isSelected != WindowController.IsWindowOpened)
					{
						if (!s_hasModelInitialized)
						{
							SeekModel.Initialize();
							s_hasModelInitialized = true;
						}
						WindowController.ToggleWindow();
					}
				}
			}
		}

		[HarmonyPatch(typeof(MemoryUtility), "ClearAllMapsAndWorld", MethodType.Normal)]
		class Patch_ClearAllMapsAndWorld
		{
			static void Postfix()
			{
				ObjectsDatabase.ClearUpdateAction();
			}
		}
		#endregion Patches
	}
}
