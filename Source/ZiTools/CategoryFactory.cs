using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ZiTools
{
	static class CategoryFactory
	{
		public static IEnumerable<ICategory> GetCategories()
		{
			RequestGroupCategory reqCategory;

			reqCategory = new RequestGroupCategory("Everything", BaseContent.WhiteTex);
			reqCategory.AddGroup(ThingRequestGroup.Everything);
			yield return reqCategory;
			yield return new RequestGroupCategory("2", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("3", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("4", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("5", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("6", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("7", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("8", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("9", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("0", BaseContent.WhiteTex);
		}
	}
}
