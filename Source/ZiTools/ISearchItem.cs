using UnityEngine;

namespace ZiTools
{
	public interface ISearchItem
	{
		string Label { get; }

		Texture2D Texture { get; }

		int Count { get; }
	}
}