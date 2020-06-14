using System.Collections.Generic;
using UnityEngine;

namespace ZiTools
{
	public interface ICategory
	{
		string Label { get; }

		Texture2D Texture { get; }

		IEnumerable<ISearchItem> GetSearchItems();
	}
}