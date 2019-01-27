using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace ZiTools
{
	public class DBUnit : IExposable
	{
		private string _label;
		public readonly List<IntVec3> Locations = new List<IntVec3>();

		public DBUnit(string label)
		{
			this._label = label;
			this.CleanData();
		}

		public DBUnit() : this(string.Empty) { }

		public string Label { get => _label; }

		public string Parameter { get; set; }

		public IDBIcon Icon { get; set; }

		public float Area { get; set; }

		public int StackCount { get; set; }

		public int CorpseTime { get; private set; }

		public void SetPatameter(CategoryOfObjects category)
		{
			switch (category)
			{
				case CategoryOfObjects.Corpses:
				{
					if (CorpseTime > 0 && CorpseTime < int.MaxValue)
						Parameter = CorpseTime.ToStringTicksToDays();
					else
						Parameter = "-";
					Parameter += $" ({Locations.Count})";
				}
				break;
				default:
				{
					float thingsCount = Locations.Count;
					if (StackCount > 1)
						thingsCount = StackCount;
					else if (Area > 1f)
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
			CorpseTime = int.MaxValue;
			Parameter = "0";
			StackCount = 0;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref this._label, "ZiT_Unit.Label");
		}
	}
}
