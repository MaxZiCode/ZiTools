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
			ICategory category;

			category = new RequestGroupCategory("Everything", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("Everything", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("Everything", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("Everything", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("Everything", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("Everything", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("Everything", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("Everything", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("Everything", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("Everything", BaseContent.WhiteTex);
			yield return new RequestGroupCategory("Everything", BaseContent.WhiteTex);
		}
	}
}
