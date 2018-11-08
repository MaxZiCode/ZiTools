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
		public static readonly ObjectSeeker_Data OSD_Global = new ObjectSeeker_Data();

		static StaticConstructor()
		{
			var harmony = HarmonyInstance.Create("rimworld.maxzicode.zitools.mainconstructor");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		public static void DebugMessage(string msg)
		{
			Log.Message("[ZiTools] " + msg);
		}

		[HarmonyPatch(typeof(Game), "CurrentMap", MethodType.Setter)]
		class Patch_CurrentMap
		{
			static void Postfix()
			{
				if (OSD_Global.WindowIsOpen && Current.ProgramState == ProgramState.Playing)
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
					bool isSelected = OSD_Global.WindowIsOpen;
					row.ToggleableIcon(ref isSelected, ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true), "ZiT_ObjectsSeekerDesc".Translate(), SoundDefOf.Mouseover_ButtonToggle);
					if (isSelected && !OSD_Global.WindowIsOpen)
					{
						ObjectSeeker_Window.DrawWindow();
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
	}
}
