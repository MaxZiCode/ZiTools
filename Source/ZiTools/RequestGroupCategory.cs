using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Verse;
using Verse.Noise;

namespace ZiTools
{
	public class RequestGroupCategory : Category
	{
		private readonly List<ThingRequestGroup> groups = new List<ThingRequestGroup>();
		private readonly List<ThingRequestGroup> ignorGroups = new List<ThingRequestGroup>();

		public RequestGroupCategory(string label)
		{
			this.Label = label ?? throw new ArgumentNullException(nameof(label));
		}

		public void AddGroup(ThingRequestGroup group) => groups.Add(group);

		public void AddIgnorGroup(ThingRequestGroup group) => ignorGroups.Add(group);

		public override IEnumerable<ISearchItem> GetFilteredItems(IEnumerable<ISearchItem> searchItems)
		{
			foreach (var item in searchItems)
			{
				if (item.Def is ThingDef tDef && 
					groups.All(g => ThingListGroupHelper.Includes(g, tDef)) && 
					!ignorGroups.Any(ig => ThingListGroupHelper.Includes(ig, tDef)))
				{
					yield return item;
				}
			}
		}
	}
}
