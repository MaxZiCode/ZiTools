using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ZiTools
{
	static class ItemFilterFactory
	{
		private static RequestGroupCategory reqFilter;

		public static IEnumerable<Category> GetFilters()
		{
			reqFilter = new RequestGroupCategory("Everything");
			reqFilter.AddGroup(ThingRequestGroup.Everything);
			yield return reqFilter;

			yield return new RequestGroupCategory("2");
			yield return new RequestGroupCategory("3");
			yield return new RequestGroupCategory("4");
			yield return new RequestGroupCategory("5");
			yield return new RequestGroupCategory("6");
			yield return new RequestGroupCategory("7");
			yield return new RequestGroupCategory("8");
			yield return new RequestGroupCategory("9");
			yield return new RequestGroupCategory("0");
		}
	}
}
