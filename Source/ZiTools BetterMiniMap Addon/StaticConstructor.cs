using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

using Verse;
using Harmony;
using BetterMiniMap;

using static ZiTools.StaticConstructor;

namespace ZiTools_BetterMiniMap
{
	[StaticConstructorOnStartup]
	public static class StaticConstructor
	{
	    public static DesignationOverlay desOv;

        static StaticConstructor()
		{

			if (ModLister.AllInstalledMods.FirstOrDefault(m => m.Name == "BetterMiniMap")?.Active == true)
			{
				//DesignationOverlay desOv = new DesignationOverlay();
				//OverlayManager.DefOverlays.Add(desOv);
				//ZiTools.ObjectSeeker_Window.UpdateAction += delegate { desOv.Visible = true; };

				var harmony = HarmonyInstance.Create("rimworld.maxzicode.zitools.addonconstructor");
				harmony.PatchAll(Assembly.GetExecutingAssembly());
			}
		}

		[HarmonyPatch(typeof(OverlayManager), MethodType.Constructor, new Type[] { typeof(Map) })]
		class Patch_OVerlayManager
		{
			static void Postfix(OverlayManager __instance, Map map)
			{
				desOv = new DesignationOverlay(map);
				__instance.DefOverlays.Add(desOv);
				ZiTools.ObjectSeeker_Window.UpdateAction += delegate
				{ desOv.Visible = true; };
#if DEBUG
				DebugMessage("Designation overlay has added by " + __instance.GetType().ToString());
#endif
			}
		}
	}
}
