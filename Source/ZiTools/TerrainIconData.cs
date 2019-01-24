using Verse;
using UnityEngine;

namespace ZiTools
{
	// class for drawing a terrain icon
	public class TerrainIconData : IDBIcon
	{
		readonly Texture2D icon;
		Rect iconTexCoords;
		readonly float iconAngle;
		Color iconDrawColor;

		public TerrainIconData(TerrainDef entDef)
		{
			this.iconDrawColor = entDef.uiIconColor;
			this.icon = entDef.uiIcon;
			this.iconTexCoords = new Rect(0f, 0f, 64f / (float)this.icon.width, 64f / (float)this.icon.height);
			this.iconAngle = entDef.uiIconAngle;
		}

		public void DrawIcon(Rect outerRect)
		{
			GUI.color = this.iconDrawColor;
			Widgets.DrawTextureFitted(outerRect, this.icon, 1f, Vector2.one, this.iconTexCoords, this.iconAngle);
			GUI.color = Color.white;
		}
	}
}
