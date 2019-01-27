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

		public static void SetMarks(DesignationDef DesDef, List<IntVec3> positions)
		{
			if (positions != null && positions.Count != 0)
			{
				RemoveMarks(DesDef);
				IntVec3 prevPos = new IntVec3(int.MaxValue, int.MaxValue,int.MaxValue); // because (0,0,0) location exists on a game map
				Stopwatch sw = Stopwatch.StartNew();
				float prevlenghtToObject = float.MaxValue;
				IntVec3 nearestPos = new IntVec3();
				for (int i = 0; i < positions.Count; i++)
				{
					if (sw.Elapsed.Seconds >= 2)
					{
						Messages.Message("ZiT_WarningTimeLabel".Translate(), null, MessageTypeDefOf.NeutralEvent);
						sw.Stop();
						break;
					}

					IntVec3 pos = positions[i];
					if (pos != prevPos)
					{
						Find.CurrentMap.designationManager.AddDesignation(new Designation(pos, DesDef));
						float lenghtToObject = (Find.CameraDriver.MapPosition - pos).LengthHorizontal;
						if (lenghtToObject < prevlenghtToObject)
						{
							prevlenghtToObject = lenghtToObject;
							nearestPos = pos;
						}
						prevPos = pos;
					}
				}
				if (sw.IsRunning)
					sw.Stop();
				Find.CameraDriver.JumpToCurrentMapLoc(nearestPos);
			}
		}

		public static void RemoveMarks(DesignationDef DesDef)
		{
			foreach (Map map in Find.Maps)
			{
				map.designationManager.RemoveAllDesignationsOfDef(DesDef);
			}
		}
	}
}
