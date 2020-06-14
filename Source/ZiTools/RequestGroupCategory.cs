using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Verse;

namespace ZiTools
{
	public class RequestGroupCategory : ICategory
	{
		private List<ThingRequestGroup> _groups = new List<ThingRequestGroup>();
		private List<ThingRequestGroup> _ignorGroups = new List<ThingRequestGroup>();

		public string Label { get; }
		public Texture2D Texture { get; }

		public RequestGroupCategory(string label, Texture2D texture)
		{
			this.Label = label ?? throw new ArgumentNullException(nameof(label));
			this.Texture = texture ?? throw new ArgumentNullException(nameof(texture));
		}

		public void AddGroup(ThingRequestGroup group) => _groups.Add(group);

		public void AddIgnorGroup(ThingRequestGroup group) => _ignorGroups.Add(group);

		public IEnumerable<ISearchItem> GetSearchItems(IEnumerable<ISearchItem> searchItems)
		{
			// TODO: Доделать.
			return searchItems;
		}
	}
}
