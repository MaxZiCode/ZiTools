using UnityEngine;
using Verse;

namespace ZiTools
{
	public interface ISearchItem
	{
		string Label { get; }

		Texture2D Texture { get; }

		int Count { get; }

		Def Def { get; }
	}
}