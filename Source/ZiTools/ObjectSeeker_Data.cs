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
				TerrainDef ter = location.GetTerrain(_mapInProcess);
				FillNewDataTerrain(ter, location);
				List<Thing> allThingsOnLocation = location.GetThingList(_mapInProcess);
				foreach (Thing currentThing in allThingsOnLocation)
				{
					string defName = currentThing.def.defName;
					string label = currentThing.def.label;

					if (FillNewData<Plant>(currentThing, CategoryOfObjects.Plants, location))
						continue;
					if (FillNewData<Pawn>(currentThing, CategoryOfObjects.Pawns, location))
						continue;

					if (FillNewData<Corpse>(currentThing, CategoryOfObjects.Corpses, location))
					{
						CompRottable comp = ((Corpse)currentThing).GetComp<CompRottable>();
						int currentTicksRemain = comp == null ? 0 : Mathf.RoundToInt(comp.PropsRot.TicksToRotStart - comp.RotProgress);
						unitsDict[defName].CheskAndSetCorpseTime(currentTicksRemain);
						continue;
					}

					if (currentThing.Stuff != null)
					{
						defName += $" ({currentThing.Stuff.defName})";
						label += $" ({currentThing.Stuff.LabelAsStuff})";
					}

					if (FillNewData<Building>(currentThing, CategoryOfObjects.Buildings, location))
						continue;

					FillNewData<Thing>(currentThing, CategoryOfObjects.Others, location); 
				}
			}

			// Filling All category
			var AllObjects = from k in CategoriesDict.Keys where k != CategoryOfObjects.All && k != CategoryOfObjects.Favorites select CategoriesDict[k];
			foreach (var list in AllObjects)
			{
				CategoriesDict[CategoryOfObjects.All].AddRange(list);
			}

			if (CategoriesDict[CategoryOfObjects.Favorites].Count > 0) // TODO: Change fav category
				CategoriesDict[CategoryOfObjects.Favorites].RemoveAll(n => !CategoriesDict[CategoryOfObjects.All].Contains(n));

			// Sorting
			foreach (var c in CategoriesDict.Keys)
			{
				CategoriesDict[c].Sort((u1, u2) => string.Compare(u1.Label, u2.Label));
			}
			CategoriesDict[CategoryOfObjects.Corpses].Sort((u1, u2) => u1.CorpseTime.CompareTo(u2.CorpseTime));
			;

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

		bool FillNewData<T>(Thing thing, CategoryOfObjects category, IntVec3 location)
		{
			if (thing is T)
			{
				string defName = thing.def.defName;
				if (AddUnit(defName, thing.def.label, category, location))
				{
					unitsDict[defName].Icon = new ThingIconData(thing);
					unitsDict[defName].Area = thing.def.size.Area;
				}
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
			LogDebug("ExposeData started");
			List<DBUnit> fav = CategoriesDict[CategoryOfObjects.Favorites];
			Scribe_Collections.Look(ref fav, "ODB_favourites", LookMode.Reference);
			if (fav != null)
				CategoriesDict[CategoryOfObjects.Favorites] = fav;
			LogDebug("ExposeData finished");
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
