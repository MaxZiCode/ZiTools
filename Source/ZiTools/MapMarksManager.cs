using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Verse;
using RimWorld;

namespace ZiTools
{
	public static class MapMarksManager
	{
		public static readonly DesignationDef ObjectSeeker_MarkDef = DefDatabase<DesignationDef>.GetNamed("ObjectSeekerMark", true);

		public static void SetMarks(DesignationDef DesDef)
		{
			ObjectSeeker_Data OSD = StaticConstructor.OSD_Global;
			if (OSD.MapInProcess == Find.CurrentMap && OSD.ThingToSeek != string.Empty && OSD.Positions.Count != 0)
			{
				RemoveMarks(DesDef);
				IntVec3 prevPos = new IntVec3();
				Stopwatch sw = Stopwatch.StartNew();
				for (int i = 0; i < OSD.Positions.Count; i++)
				{
					if (sw.Elapsed.Seconds >= 2)
					{
						Messages.Message("ZiT_WarningTimeLabel".Translate(), null, MessageTypeDefOf.NeutralEvent);
						sw.Stop();
						break;
					}

					IntVec3 pos = OSD.Positions[i];
					if (pos == prevPos)
						continue;
					Find.CurrentMap.designationManager.AddDesignation(new Designation(pos, DesDef));
					prevPos = pos;
				}
			}
		}

		public static void RemoveMarks(DesignationDef DesDef) => Find.CurrentMap.designationManager.RemoveAllDesignationsOfDef(DesDef);
	}
}
