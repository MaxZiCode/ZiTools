using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;


namespace BetterMiniMap.Overlays
{
	public class FoundObjects_Overlay : MarkerOverlay
	{
		static readonly SelectWindowData _sWD = new SelectWindowData();

		public FoundObjects_Overlay(bool visible = true) : base(visible) { }

		public static SelectWindowData SWD { get => _sWD;}

		public override int GetUpdateInterval() => 20;

		public override void Render()
		{
			if (_sWD.MapInProcess == Find.CurrentMap)
			{
				Utilities.RemoveDesignations();
				if (_sWD.ThingToSeek != string.Empty && SWD.Positions.Count != 0)
				{
					IntVec3 prevPos = new IntVec3();
					for (int i = 0; i < SWD.Positions.Count; i++)
					{
						IntVec3 pos = SWD.Positions[i];
						if (pos == prevPos)
							continue;
						base.CreateMarker(pos, 5, Color.black, Color.magenta, 0.3f);

						Find.CurrentMap.designationManager.AddDesignation(new Designation(pos, Utilities.FoundDesignationDef));
						prevPos = pos;
					} 
				}
			}
		}

		public override bool GetShouldUpdateOverlay()
		{
			return _sWD.WasUpdated();
		}
	}
}
