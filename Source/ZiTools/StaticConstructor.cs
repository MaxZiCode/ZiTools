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
			static Texture2D icon = ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true);
			static string tooltip = "ZiT_ObjectsSeekerLabel".Translate();
			static Type objectSeeker_Window = typeof(ObjectSeeker_Window);
			static void Postfix(WidgetRow row, bool worldView)
			{
				if (!worldView)
				{
					bool isSelected = Find.WindowStack.IsOpen(objectSeeker_Window);
					row.ToggleableIcon(ref isSelected, icon, tooltip, SoundDefOf.Mouseover_ButtonToggle);
					bool isSelected2 = Find.WindowStack.IsOpen(objectSeeker_Window);
					if (isSelected != isSelected2)
					{
						if (!isSelected2)
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
				ObjectsDatabase.ClearUpdateAction();
			}
		}
		#endregion Patches
	}
}
