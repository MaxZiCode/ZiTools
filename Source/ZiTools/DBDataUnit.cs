using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace ZiTools
{
	public class DBUnit
	{
		public readonly List<IntVec3> Locations = new List<IntVec3>();

		public DBUnit(string label)
		{
			this.Label = label;
		}

		public string Label { get; private set; }

		public string Parameter { get; set; }

		public IDBIcon Icon { get; set; }

		public float Area { get; set; }

		public int CorpseTime { get; private set; }

		public void SetPatameter(CategoryOfObjects category)
		{
			switch (category)
			{
				case CategoryOfObjects.Corpses:
				{
					if (CorpseTime > 0)
						Parameter = CorpseTime.ToStringTicksToDays();
					else
						Parameter = "-";
					Parameter += $" ({Locations.Count})";
				}
				break;
				default:
				{
					float thingsCount = Locations.Count;
					if (Area > 1f)
						thingsCount /= Area;
					Parameter = thingsCount.ToString();
				}
				break;
			}
		}
		public void CheskAndSetCorpseTime(int ticks)
		{
			if (ticks > 0 && ticks < CorpseTime)
				CorpseTime = ticks;
		}

		public void CleanData()
		{
			Locations.Clear();
			CorpseTime = new int();
			Parameter = string.Empty;
		}
	}
}
