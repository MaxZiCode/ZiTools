using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using Verse;
using BetterMiniMap;

namespace ZiTools_BetterMiniMap
{
	[StaticConstructorOnStartup]
    public class StaticConstructor
    {
		static StaticConstructor()
		{
			if (ModLister.AllInstalledMods.FirstOrDefault(m => m.Name == "BetterMiniMap")?.Active == true)
			{
				DesignationOverlay desOv = new DesignationOverlay();
				OverlayManager.DefOverlays.Add(desOv);
				ZiTools.ObjectSeeker_Window.UpdateAction += delegate { desOv.Visible = true; };
			}
		}
	}
}
