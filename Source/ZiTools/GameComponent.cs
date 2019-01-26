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
		private bool _isFirtsLaunch = true; //need to avoid an error when the game not found a data for loading

		public ZiTools_GameComponent(Game g) : base()
		{
			ObjectsDatabase = new ObjectsDatabase();
		}

		public ObjectsDatabase ObjectsDatabase { get; private set; }

		public override void ExposeData()
		{
#if DEBUG
			LogDebug("Exposing " + Scribe.mode.ToStringSafe()); 
#endif
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (Scribe.mode == LoadSaveMode.Saving)
					_isFirtsLaunch = false;
				Scribe_Values.Look(ref _isFirtsLaunch, "ZiT_isFirtsLaunch", true);
				if (!_isFirtsLaunch)
					this.ObjectsDatabase.ExposeData();
			}
#if DEBUG
			LogDebug("Exposing finished!");
#endif
		}

		public static ObjectsDatabase GetObjectsDatabase() => ((ZiTools_GameComponent)Current.Game.components.Find(gc => gc is ZiTools_GameComponent)).ObjectsDatabase;
	}
}