using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

using static ZiTools.StaticConstructor;

namespace ZiTools
{
	public class ZiTools_GameComponent : GameComponent
	{
		public ZiTools_GameComponent(Game g) : base()
		{
			ObjectsDatabase = new ObjectsDatabase();
		}

		public ObjectsDatabase ObjectsDatabase { get; private set; }

		public override void ExposeData()
		{
#if DEBUG
			LogDebug("Exposing..."); 
#endif
			base.ExposeData();
			this.ObjectsDatabase.ExposeData();
#if DEBUG
			LogDebug("Exposing finished!");
#endif
		}

		public static ObjectsDatabase GetObjectsDatabase() => ((ZiTools_GameComponent)Current.Game.components.Find(gc => gc is ZiTools_GameComponent)).ObjectsDatabase;
	}
}