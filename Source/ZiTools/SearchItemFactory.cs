using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;

namespace ZiTools
{
	public static class SearchItemFactory
	{
		public static IEnumerable<ISearchItem> GetSearchItems(Map map)
		{
			var items = from loc in map.AllCells 
						select map.thingGrid.ThingsListAtFast(loc)
						into things
						from thing in things
						group thing by thing.def
						into g
						select new SearchThingDef(g.Key, g.Count());

			return items;
		}
	}
}
