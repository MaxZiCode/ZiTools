using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

using Verse;
using Harmony;

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
				if (OSD_Global.WindowIsOpen)
				{
					ObjectSeeker_Window.Update();
				}
			}
		}
	}
}
