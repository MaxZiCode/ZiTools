using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ZiTools
{
	public class SearchThingDef : ISearchItem
	{
		public string Label { get; }
		public Texture2D Texture { get; }
		public int Count { get; }

		public SearchThingDef(ThingDef thingDef, int count)
		{
			Label = thingDef.LabelCap;
			Texture = thingDef.uiIcon;
			this.Count = count;
		}
	}
}
