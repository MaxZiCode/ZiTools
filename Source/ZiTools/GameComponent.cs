using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace ZiTools
{
	[StaticConstructorOnStartup]
	public class ZiTools_GameComponent : GameComponent
	{
		public ZiTools_GameComponent(Game g) : base()
		{
			OSD_Global = new ObjectSeeker_Data();
		}

		public static ObjectSeeker_Data OSD_Global { get; private set; }

		public override void ExposeData()
		{
			base.ExposeData();
			OSD_Global.ExposeData();
		}
	}
}
