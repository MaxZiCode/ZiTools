using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using Verse;
using RimWorld;
using UnityEngine;

using static ZiTools.StaticConstructor;

namespace ZiTools
{
	public class ObjectsDatabase : IExposable
	{
		public ObjectsDatabase()
		{
			_defToSeek = null;
			SelectedCategory = CategoryOfObjects.Favorites;
		}

		string _defToSeek; //defName
		Map _mapInProcess;

		static Action UpdateAction = delegate { };

		Dictionary<CategoryOfObjects, Texture2D> TexturesOfCategoriesDict;
		Dictionary<string, List<IntVec3>> LocationsDict = new Dictionary<string, List<IntVec3>>(); //defName - locations
		Dictionary<string, ThingIconData> thingIconsDict = new Dictionary<string, ThingIconData>(); //ThingIconData for textures
		Dictionary<string, TerrainIconData> terrainIconsDict = new Dictionary<string, TerrainIconData>(); //TerrainIconData for terrain textures 
		Dictionary<CategoryOfObjects, List<string>> CategoriesDict = new Dictionary<CategoryOfObjects, List<string>> //Category - defNames
			{ { CategoryOfObjects.Favorites, new List<string>() } };
		Dictionary<string, string> ThingsParams = new Dictionary<string, string>(); //defName - parameter
		Dictionary<string, string> labelsDict = new Dictionary<string, string>(); //defName - name

		public readonly Dictionary<CategoryOfObjects, string> NamesOfCategoriesDict = new Dictionary<CategoryOfObjects, string>
		{
			{ CategoryOfObjects.Favorites, "ZiT_FavoritesCategoryLabel".Translate() },
			{ CategoryOfObjects.All, "ZiT_AllCategoryLabel".Translate() },
			{ CategoryOfObjects.Buildings, "ZiT_BuildingCategoryLabel".Translate() },
			{ CategoryOfObjects.Terrains, "ZiT_TerrainCategoryLabel".Translate() },
			{ CategoryOfObjects.Plants, "ZiT_PlantCategoryLabel".Translate() },
			{ CategoryOfObjects.Pawns, "ZiT_PawnsCategoryLabel".Translate() },
			{ CategoryOfObjects.Corpses, "ZiT_СorpsesCategoryLabel".Translate() },
			{ CategoryOfObjects.Others, "ZiT_OtherCategoryLabel".Translate() }
		};

		public List<IntVec3> Positions
		{
			get
			{
				if (_defToSeek != null)
					return LocationsDict[_defToSeek];
				else
					return null;
			}
		}

		public string DefNameToSeek
		{
			get => _defToSeek;
			set
			{
				if (!string.IsNullOrEmpty(value) && CategoriesDict[CategoryOfObjects.All].Contains(value))
					_defToSeek = value;
				else
					_defToSeek = null;
			}
		}

		public string SelectedCategoryName { get => NamesOfCategoriesDict[SelectedCategory]; }
		public CategoryOfObjects SelectedCategory { get; set; }
		public List<string> DefNamesInFavourites { get => CategoriesDict[CategoryOfObjects.Favorites]; } // TODO: add labels

		public CategoryOfObjects GetCategoryViaInt(int i) => (CategoryOfObjects)Enum.Parse(typeof(CategoryOfObjects), i.ToString());

		public Texture2D GetCategoryTexture(CategoryOfObjects category) => TexturesOfCategoriesDict[category];

		public string GetParameter(string defName)
		{
			try
			{
				if (ThingsParams.ContainsKey(defName))
					return ThingsParams[defName];
				else
					return "0";
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
				return "error";
			}
		}

		public List<string> GetDefNames(string word)
		{
			List<string> names = CategoriesDict[SelectedCategory];
			if (!string.IsNullOrEmpty(word))
				names = (CategoriesDict[SelectedCategory].Where(k => labelsDict[k].ToLower().Contains(word.ToLower()))).ToList();
			return names;
		}

		public string GetLabel(string defName)
		{
			if (labelsDict.TryGetValue(defName, out string v))
				return v;
			else
				return defName;
		}

		public void DrawIcon(string defName, Rect outerRect)
		{
			if (thingIconsDict.ContainsKey(defName) && thingIconsDict?[defName] != null)
				thingIconsDict[defName].DrawIcon(outerRect);
			else if (terrainIconsDict.ContainsKey(defName) && terrainIconsDict?[defName] != null)
				terrainIconsDict[defName].DrawIcon(outerRect);
			else
				Log.Warning($"Object seeker could not load an icon for {defName} def");
		}

		public bool IsSelectedCategoryHaveObjects() => CategoriesDict[SelectedCategory].Count > 0;

		public void Update()
		{
			FindAll();
			MapMarksManager.SetMarks(MapMarksManager.ObjectSeeker_MarkDef, this.Positions);
			UpdateAction();
		}

		public void Clear()
		{
			DefNameToSeek = null;
			MapMarksManager.RemoveMarks(MapMarksManager.ObjectSeeker_MarkDef);
			UpdateAction();
		}

		public static void DoUpdateAction() => UpdateAction();

		public static void SetUpdateAction(Action action) => UpdateAction += action;

		public static void ClearUpdateAction() => UpdateAction = delegate { };

		public void FindAll()
		{
			if (TexturesOfCategoriesDict == null) // NOTE: works only after a loading game
				this.InitializeTextures();
#if DEBUG
			Stopwatch sw = Stopwatch.StartNew();
#endif
			this._mapInProcess = Find.CurrentMap;

			LocationsDict.Clear();
			ThingsParams.Clear();
			
			List<string> favourites = CategoriesDict[CategoryOfObjects.Favorites];
			CategoriesDict = new Dictionary<CategoryOfObjects, List<string>>
			{
				{ CategoryOfObjects.Favorites, favourites },
				{ CategoryOfObjects.All, new List<string>() },
				{ CategoryOfObjects.Buildings, new List<string>() },
				{ CategoryOfObjects.Terrains, new List<string>() },
				{ CategoryOfObjects.Plants, new List<string>() },
				{ CategoryOfObjects.Pawns, new List<string>() },
				{ CategoryOfObjects.Corpses, new List<string>() },
				{ CategoryOfObjects.Others, new List<string>() }
			};
			Dictionary<string, int> corpsesTimeRemainDict = new Dictionary<string, int>();
			foreach (IntVec3 location in _mapInProcess.AllCells)
			{
				if (_mapInProcess.fogGrid.IsFogged(location))
					continue;
				TerrainDef ter = location.GetTerrain(_mapInProcess);
				FillNewData<TerrainDef>(location, ter.defName, ter.label, CategoryOfObjects.Terrains);
				if (!terrainIconsDict.ContainsKey(ter.defName))
					terrainIconsDict.Add(ter.defName, new TerrainIconData(ter));
				List<Thing> allThingsOnLocation = location.GetThingList(_mapInProcess);
				if (allThingsOnLocation.Count > 0)
				{
					foreach (Thing currentThing in allThingsOnLocation)
					{
						string defName = currentThing.def.defName;
						string label = currentThing.def.label;
						if (FillNewData<Plant>(location, defName, label, CategoryOfObjects.Plants, currentThing))
							continue;
						if (FillNewData<Pawn>(location, defName, label, CategoryOfObjects.Pawns, currentThing))
							continue;

						if (FillNewData<Corpse>(location, defName, label, CategoryOfObjects.Corpses, currentThing))
						{
							CompRottable comp = ((Corpse)currentThing).GetComp<CompRottable>();
							int currentTicksRemain = comp == null ? 0 : Mathf.RoundToInt(comp.PropsRot.TicksToRotStart - comp.RotProgress);
							currentTicksRemain = currentTicksRemain > 0 ? currentTicksRemain : 0;
							if (corpsesTimeRemainDict.ContainsKey(defName))
							{
								if (corpsesTimeRemainDict[defName] > currentTicksRemain && currentTicksRemain > 0)
									corpsesTimeRemainDict[defName] = currentTicksRemain;
							}
							else
								corpsesTimeRemainDict.Add(defName, currentTicksRemain);
							continue;
						}

						if (currentThing.Stuff != null)
						{
							defName += $" ({currentThing.Stuff.defName})";
							label += $" ({currentThing.Stuff.LabelAsStuff})";
						}

						if (FillNewData<Building>(location, defName, label, CategoryOfObjects.Buildings, currentThing))
							continue;
						FillNewData<Thing>(location, defName, label, CategoryOfObjects.Others, currentThing);
					}
				}
			}
			
			// Filling All category
			var AllObjects = (from k in CategoriesDict.Keys where k != CategoryOfObjects.All && k != CategoryOfObjects.Favorites select CategoriesDict[k]);
			foreach (var list in AllObjects)
			{
				CategoriesDict[CategoryOfObjects.All].AddRange(list);
			}
			if (CategoriesDict[CategoryOfObjects.Favorites].Count > 0) // TODO: Change fav category
				CategoriesDict[CategoryOfObjects.Favorites].RemoveAll(n => !CategoriesDict[CategoryOfObjects.All].Contains(n));
			
			// Filling parametres
			foreach (string defName in this.CategoriesDict[CategoryOfObjects.All])
			{
				if (CategoriesDict[CategoryOfObjects.Corpses].Contains(defName))
				{
					if (corpsesTimeRemainDict[defName] == 0)
					{
						ThingsParams.Add(defName, "-");
					}
					else
						ThingsParams.Add(defName, corpsesTimeRemainDict[defName].ToStringTicksToDays());
					ThingsParams[defName] += $" ({LocationsDict[defName].Count})";
				}
				else
				{
					if (!LocationsDict.ContainsKey(defName))
						ThingsParams.Add(defName, "0");
					else
						ThingsParams.Add(defName, LocationsDict[defName].Count.ToString());
				}
			}
			
			// Sorting
			CorpseTimeComparer comparer = new CorpseTimeComparer(corpsesTimeRemainDict);
			for (int i = 0; i < Enum.GetNames(typeof(CategoryOfObjects)).Length; i++)
			{
				CategoryOfObjects curCateg = this.GetCategoryViaInt(i);
				if (curCateg == CategoryOfObjects.Corpses)
					CategoriesDict[curCateg].Sort(comparer);
				else
					CategoriesDict[curCateg].Sort(); // NOTE: keep the sorting by defName? I guess so
			}

			// DefNameToSeek checking
			if (DefNameToSeek != null && !this.LocationsDict.ContainsKey(this.DefNameToSeek))
				Clear();
#if DEBUG
			sw.Stop();
			LogDebug($"Object Seeker has filled a data for {sw.ElapsedMilliseconds} ms");
#endif
		}

		bool FillNewData<T>(IntVec3 location, string defname, string label, CategoryOfObjects category, Thing currentThing = null)
		{
			if (currentThing is T || currentThing == null)
			{
				if (!labelsDict.ContainsKey(defname))
					labelsDict.Add(defname, label);
				if (currentThing != null && !thingIconsDict.ContainsKey(defname))
					thingIconsDict.Add(defname, new ThingIconData(currentThing));

				if (!CategoriesDict[category].Contains(defname))
					CategoriesDict[category].Add(defname);

				if (LocationsDict.ContainsKey(defname))
					LocationsDict[defname].Add(location);
				else
					LocationsDict.Add(defname, new List<IntVec3>(new IntVec3[] { location }));

				return true;
			}
			else
				return false;
		}

		void InitializeTextures()
		{
			TexturesOfCategoriesDict = new Dictionary<CategoryOfObjects, Texture2D>
			{
				{ CategoryOfObjects.Favorites,  ContentFinder<Texture2D>.Get("UI/Favourite Button", true) },
				{ CategoryOfObjects.All,  ContentFinder<Texture2D>.Get("UI/All Button", true) },
				{ CategoryOfObjects.Buildings,  ContentFinder<Texture2D>.Get("UI/Designators/Deconstruct", true) },
				{ CategoryOfObjects.Terrains,  ContentFinder<Texture2D>.Get("UI/Designators/RemoveFloor", true) },
				{ CategoryOfObjects.Plants,  DefDatabase<ThingDef>.GetNamed("Plant_TreeOak").uiIcon },
				{ CategoryOfObjects.Pawns,  DefDatabase<ThingDef>.GetNamed("Muffalo").uiIcon },
				{ CategoryOfObjects.Corpses,  ContentFinder<Texture2D>.Get("Things/Mote/ThoughtSymbol/Skull", true) },
				{ CategoryOfObjects.Others,  DefDatabase<ThingDef>.GetNamed("ChunkSlagSteel").uiIcon }
			};
		}

		public void ExposeData()
		{
			LogDebug("ExposeData started");
			List<string> fav = CategoriesDict[CategoryOfObjects.Favorites];
			Scribe_Collections.Look<string>(ref fav, "ObjectSeeker_favourites");
			if (fav != null)
				CategoriesDict[CategoryOfObjects.Favorites] = fav;
			LogDebug("ExposeData finished");
		}

		//optimized inner class for drawing a terrain icon
		public class TerrainIconData 
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

		//optimized inner class for drawing a thing icon
		public class ThingIconData 
		{
			Color drawColor;
			readonly float resolvedIconAngle;
			readonly Texture resolvedIcon;

			public const float ThingIconSize = 50f;

			public ThingIconData(Thing thing)
			{
				this.drawColor = thing.DrawColor;
				this.resolvedIconAngle = 0f;
				if (!thing.def.uiIconPath.NullOrEmpty())
				{
					this.resolvedIcon = thing.def.uiIcon;
					this.resolvedIconAngle = thing.def.uiIconAngle;
				}
				else if (thing is Pawn || thing is Corpse)
				{
					if (!(thing is Pawn pawn))
					{
						pawn = ((Corpse)thing).InnerPawn;
					}
					if (!pawn.RaceProps.Humanlike)
					{
						if (!pawn.Drawer.renderer.graphics.AllResolved)
						{
							pawn.Drawer.renderer.graphics.ResolveAllGraphics();
						}
						Material material = pawn.Drawer.renderer.graphics.nakedGraphic.MatAt(Rot4.East, null);
						this.resolvedIcon = material.mainTexture;
						this.drawColor = material.color;
					}
					else
					{
						this.resolvedIcon = PortraitsCache.Get(pawn, new Vector2(ThingIconSize, ThingIconSize), default(Vector3), 1f);
					}
				}
				else
				{
					this.resolvedIcon = thing.Graphic.ExtractInnerGraphicFor(thing).MatAt(thing.def.defaultPlacingRot, null).mainTexture;
				}
			}

			public void DrawIcon(Rect outerRect)
			{
				//t = t.GetInnerIfMinified();
				GUI.color = this.drawColor;
				Widgets.DrawTextureRotated(outerRect, this.resolvedIcon, this.resolvedIconAngle);
				GUI.color = Color.white;
			}
		}
	}

	public enum CategoryOfObjects
	{
		Favorites,
		All,
		Buildings,
		Terrains,
		Plants,
		Pawns,
		Corpses,
		Others,
	}
}
