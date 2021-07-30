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
	public partial class ObjectsDatabase : IExposable
	{
		public ObjectsDatabase()
		{
			_unitToSeek = null;
			SelectedCategory = CategoryOfObjects.Favorites;
		}

		DBUnit _unitToSeek;

		Map _mapInProcess;

		static Action UpdateAction = delegate { };

		Dictionary<CategoryOfObjects, Texture2D> TexturesOfCategoriesDict;

		Dictionary<CategoryOfObjects, List<DBUnit>> CategoriesDict = new Dictionary<CategoryOfObjects, List<DBUnit>> //Category - units
			{ { CategoryOfObjects.Favorites, new List<DBUnit>() } };

		Dictionary<string, DBUnit> unitsDict = new Dictionary<string, DBUnit>(); // defName - unit;

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
				if (_unitToSeek != null)
					return _unitToSeek.Locations;
				else
					return null;
			}
		}

		public DBUnit UnitToSeek
		{
			get => _unitToSeek;
			set => _unitToSeek = CategoriesDict[CategoryOfObjects.All].Contains(value) ? value : null;
		}

		public string SelectedCategoryName { get => NamesOfCategoriesDict[SelectedCategory]; }

		public CategoryOfObjects SelectedCategory { get; set; }

		public List<DBUnit> UnitsInFavourites { get => CategoriesDict[CategoryOfObjects.Favorites]; } // TODO: add labels

		public CategoryOfObjects GetCategoryViaInt(int i) => (CategoryOfObjects)Enum.Parse(typeof(CategoryOfObjects), i.ToString());

		public Texture2D GetCategoryTexture(CategoryOfObjects category) => TexturesOfCategoriesDict[category];

		public List<DBUnit> GetUnitsByWord(string word)
		{
			List<DBUnit> names = CategoriesDict[SelectedCategory];
			if (!string.IsNullOrEmpty(word))
				names = (CategoriesDict[SelectedCategory].Where(u => u.Label.ToLower().Contains(word.ToLower()))).ToList();
			return names;
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
			UnitToSeek = null;
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

			foreach (var u in unitsDict.Values)
				u.CleanData();

			List<DBUnit> favourites = CategoriesDict[CategoryOfObjects.Favorites];
			CategoriesDict = new Dictionary<CategoryOfObjects, List<DBUnit>>
			{
				{ CategoryOfObjects.Favorites, favourites },
				{ CategoryOfObjects.All, new List<DBUnit>() },
				{ CategoryOfObjects.Buildings, new List<DBUnit>() },
				{ CategoryOfObjects.Terrains, new List<DBUnit>() },
				{ CategoryOfObjects.Plants, new List<DBUnit>() },
				{ CategoryOfObjects.Pawns, new List<DBUnit>() },
				{ CategoryOfObjects.Corpses, new List<DBUnit>() },
				{ CategoryOfObjects.Others, new List<DBUnit>() }
			};

			foreach (IntVec3 location in _mapInProcess.AllCells)
			{
				if (_mapInProcess.fogGrid.IsFogged(location))
					continue;
				this.FillNewDataTerrain(location.GetTerrain(_mapInProcess), location);
				foreach (Thing currentThing in _mapInProcess.thingGrid.ThingsAt(location))
				{
					if (currentThing is Mote)
						continue;
					Thing thingToLoad;
					bool isMinified;
					if (currentThing is MinifiedThing)
					{
						thingToLoad = ((MinifiedThing)currentThing).InnerThing;
						isMinified = true;
					}
					else
					{
						thingToLoad = currentThing;
						isMinified = false;
					}
                    if (currentThing.def.drawerType == DrawerType.None)
                    {
                        continue;
                    }
                    if (FillNewData<Building>(thingToLoad, CategoryOfObjects.Buildings, location, isMinified))
						continue;

					if (FillNewData<Plant>(thingToLoad, CategoryOfObjects.Plants, location, isMinified))
						continue;

					if (FillNewData<Pawn>(thingToLoad, CategoryOfObjects.Pawns, location, isMinified))
						continue;

					if (FillNewData<Corpse>(thingToLoad, CategoryOfObjects.Corpses, location, isMinified))
					{
						CompRottable comp = ((Corpse)thingToLoad).GetComp<CompRottable>();
						int currentTicksRemain = comp == null ? 0 : Mathf.RoundToInt(comp.PropsRot.TicksToRotStart - comp.RotProgress);
						this.unitsDict[thingToLoad.def.defName].CheskAndSetCorpseTime(currentTicksRemain);
						continue;
					}

					FillNewData<Thing>(thingToLoad, CategoryOfObjects.Others, location, isMinified);
				}
			}

			// Filling All category
			var AllObjects = from k in CategoriesDict.Keys where k != CategoryOfObjects.All && k != CategoryOfObjects.Favorites select CategoriesDict[k];
			foreach (var list in AllObjects)
			{
				CategoriesDict[CategoryOfObjects.All].AddRange(list);
			}

			// Sorting
			foreach (var c in CategoriesDict.Keys)
			{
				CategoriesDict[c].Sort((u1, u2) => string.Compare(u1.Label, u2.Label));
			}
			CategoriesDict[CategoryOfObjects.Corpses].Sort((u1, u2) => u1.CorpseTime.CompareTo(u2.CorpseTime));

			// Filling parametres
			foreach (var unit in this.CategoriesDict[CategoryOfObjects.All])
				unit.SetPatameter(CategoryOfObjects.All);
			foreach (var unit in this.CategoriesDict[CategoryOfObjects.Corpses])
				unit.SetPatameter(CategoryOfObjects.Corpses);

			// UnitToSeek checking
			if (UnitToSeek != null && !this.CategoriesDict[CategoryOfObjects.All].Contains(this.UnitToSeek))
				Clear();
#if DEBUG
			sw.Stop();
			LogDebug($"Object Seeker has filled a data for {sw.ElapsedMilliseconds} ms");
#endif
		}

		bool FillNewData<T>(Thing thing, CategoryOfObjects category, IntVec3 location, bool isMinified)
		{
			if (thing is T)
			{
				string defName = thing.def.defName;
				string label = thing.def.label;
				if (thing.Stuff != null)
				{
					defName += $" ({thing.Stuff.defName})";
					label += $" ({thing.Stuff.LabelAsStuff})";
				}
				bool isNewUnit = AddUnit(defName, label, category, location);
				if (isNewUnit)
				{
					unitsDict[defName].Icon = new ThingIconData(thing);
					unitsDict[defName].Area = thing.def.size.Area;
				}
				if (isMinified)
					unitsDict[defName].IncreaceCountOfMinified();
				if (thing.stackCount > 1)
					unitsDict[defName].StackCount += thing.stackCount - 1;
				return true;
			}
			else
				return false;
		}

		void FillNewDataTerrain(TerrainDef terrDef, IntVec3 location)
		{
			bool isNewUnit = AddUnit(terrDef.defName, terrDef.label, CategoryOfObjects.Terrains, location);
			if (isNewUnit)
				unitsDict[terrDef.defName].Icon = new TerrainIconData(terrDef);
		}

		bool AddUnit(string defName, string label, CategoryOfObjects category, IntVec3 location)
		{
			DBUnit unit;
			bool isNewUnit;
			if (!unitsDict.ContainsKey(defName))
			{
				unit = new DBUnit(label);
				unitsDict.Add(defName, unit);
				CategoriesDict[category].Add(unit);
				isNewUnit = true;
			}
			else
			{
				unit = unitsDict[defName];
				if (!CategoriesDict[category].Contains(unit))
					CategoriesDict[category].Add(unit);
				if (unit.Icon == null)
					isNewUnit = true;
				else
					isNewUnit = false;
			}
			unit.Locations.Add(location);
			return isNewUnit;
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
			Dictionary<string, DBUnit> fav = new Dictionary<string, DBUnit>(unitsDict);
			fav.RemoveAll(uDict => !CategoriesDict[CategoryOfObjects.Favorites].Contains(uDict.Value));
			Scribe_Collections.Look(ref fav, "ZiT_ObjectsDatabase.Favourites");
			if (Scribe.mode == LoadSaveMode.LoadingVars && fav != null)
			{
				CategoriesDict[CategoryOfObjects.Favorites] = fav.Values.ToList();
				foreach (var k in fav.Keys)
					unitsDict.Add(k, fav[k]);
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
