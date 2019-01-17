using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Verse;
using BetterMiniMap;
using BetterMiniMap.Overlays;

namespace ZiTools_BetterMiniMap
{
	public class DesignationOverlay : MarkerOverlay
	{
		public DesignationOverlay(Map map, bool visible = true) : base(map, visible)
		{
			this.def = new OverlayDef();
			def.defName = "ObjectSeeker_OverlayDef";
			def.label = "ZiT_ObjectsSeekerLabel".Translate();
			def.overlayClass = this.GetType();
			def.priority = 100000;
			this.map = map;
		}

		public override int GetUpdateInterval() => int.MaxValue;

		public override void Render()
		{
			var marks = map.designationManager.allDesignations.FindAll(d => d.def.defName == "ObjectSeekerMark");
			foreach (var des in marks)
			{
				base.CreateMarker(des.target.Cell, 6, UnityEngine.Color.magenta, BetterMiniMapSettings.FadedColor(UnityEngine.Color.magenta), 0.5f);
			}
			foreach (var des in marks)
			{
				base.CreateMarker(des.target.Cell, 4, UnityEngine.Color.black, BetterMiniMapSettings.FadedColor(UnityEngine.Color.black), 0.5f);
			}
		}

		public override int OverlayPriority => def.priority;
	}
}
