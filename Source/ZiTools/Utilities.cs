using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BetterMiniMap
{
	public static class Utilities
	{
		private static Dictionary<int, Color32[]> clearPixelArray = new Dictionary<int, Color32[]>();

		public static readonly DesignationDef FoundDesignationDef = DefDatabase<DesignationDef>.GetNamed("Found", true);

		public static Color32[] GetClearPixelArray
		{
			get
			{
				Map map = Find.CurrentMap;
				if (!Utilities.clearPixelArray.ContainsKey(Find.CurrentMap.Size.x))
				{
					Utilities.clearPixelArray[Find.CurrentMap.Size.x] = new Color32[Find.CurrentMap.Size.x * Find.CurrentMap.Size.z];
					for (int i = 0; i < Utilities.clearPixelArray[Find.CurrentMap.Size.x].Count<Color32>(); i++)
						Utilities.clearPixelArray[Find.CurrentMap.Size.x][i] = Color.clear;
				}
				return Utilities.clearPixelArray[Find.CurrentMap.Size.x];
			}
		}

		public static void RemoveDesignations()
		{
			DesignationManager designManager = Find.CurrentMap.designationManager;
			List<Designation> gameDesignations = designManager.allDesignations;
			List<Designation> totalDesignations = gameDesignations.ListFullCopy();
			for (int i = 0; i < totalDesignations.ListFullCopy().Count; i++)
			{
				Designation des = totalDesignations[i];
				if (des.def == FoundDesignationDef)
				{
					gameDesignations.Remove(des);
				}
			}
		}
	}
}
