using System;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics;
using UnityEngine;
using Verse;
using RimWorld;

using static ZiTools.StaticConstructor;

namespace ZiTools
{
	public class ObjectSeeker_Data : IComparer<string>, IExposable
	{
		public ObjectSeeker_Data()
		{
			LocationsDict = new Dictionary<string, List<IntVec3>> { { String.Empty, new List<IntVec3>() } };
			ThingsDict = new Dictionary<string, Thing>();
			CategoriesDict = new Dictionary<CategoryOfObjects, List<string>>
			{
				{ CategoryOfObjects.Favorites, new List<string>() }
			};
			CorpsesTimeRemainDict = new Dictionary<string, int>();
			TerrainDefDict = new Dictionary<string, TerrainDef>();
			ThingToSeek = string.Empty;
			SelectedCategory = CategoryOfObjects.Favorites;
			MapInProcess = Find.CurrentMap;
		}

		public List<IntVec3> Positions { get => LocationsDict[ThingToSeek]; }

		public readonly Dictionary<CategoryOfObjects, Texture2D> TexturesOfCategoriesDict = new Dictionary<CategoryOfObjects, Texture2D>
		{
			{ CategoryOfObjects.Favorites,  ContentFinder<Texture2D>.Get("UI/Favourite Button", true) },
			{ CategoryOfObjects.All,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) },
			{ CategoryOfObjects.Buildings,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) },
			{ CategoryOfObjects.Terrains,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) },
			{ CategoryOfObjects.Plants,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) },
			{ CategoryOfObjects.Pawns,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) },
			{ CategoryOfObjects.Corpses,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) },
			{ CategoryOfObjects.Other,  ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true) }
		};

		public readonly Dictionary<CategoryOfObjects, string> NamesOfCategoriesDict = new Dictionary<CategoryOfObjects, string>
		{
			{ CategoryOfObjects.Favorites, "ZiT_FavoritesCategoryLabel".Translate() },
			{ CategoryOfObjects.All, "ZiT_AllCategoryLabel".Translate() },
			{ CategoryOfObjects.Buildings, "ZiT_BuildingCategoryLabel".Translate() },
			{ CategoryOfObjects.Terrains, "ZiT_TerrainCategoryLabel".Translate() },
			{ CategoryOfObjects.Plants, "ZiT_PlantCategoryLabel".Translate() },
			{ CategoryOfObjects.Pawns, "ZiT_PawnsCategoryLabel".Translate() },
			{ CategoryOfObjects.Corpses, "ZiT_СorpsesCategoryLabel".Translate() },
			{ CategoryOfObjects.Other, "ZiT_OtherCategoryLabel".Translate() }
		};
		public Dictionary<string, List<IntVec3>> LocationsDict { get; set; }
		public Dictionary<string, Thing> ThingsDict { get; set; }
		public Dictionary<string, TerrainDef> TerrainDefDict { get; set; }
		public Dictionary<CategoryOfObjects, List<string>> CategoriesDict { get; set; }
		public Dictionary<string, int> CorpsesTimeRemainDict { get; set; }

		public Map MapInProcess { get; set; }

		public string ThingToSeek { get; set; }
		public string SelectedCategoryName { get => NamesOfCategoriesDict[SelectedCategory]; }
		
		public CategoryOfObjects SelectedCategory { get; set; }

		public void FindAllThings()
		{
#if DEBUG
			Stopwatch sw = Stopwatch.StartNew();
#endif
			this.MapInProcess = Find.CurrentMap;

			CorpsesTimeRemainDict.Clear();
			LocationsDict.Clear();
			LocationsDict.Add(String.Empty, new List<IntVec3>());
			List<string> favourites = CategoriesDict[CategoryOfObjects.Favorites];
			CategoriesDict.Clear();
			CategoriesDict.Add(CategoryOfObjects.Favorites, favourites);

			foreach (IntVec3 location in MapInProcess.AllCells)
			{
				if (MapInProcess.fogGrid.IsFogged(location))
					continue;
				TerrainDef ter = location.GetTerrain(MapInProcess);
				FillData<TerrainDef>(location, ter.label, CategoryOfObjects.Terrains);
				if (!TerrainDefDict.ContainsKey(ter.label))
					TerrainDefDict.Add(ter.label, ter);
				List<Thing> allThingsOnLocation = location.GetThingList(MapInProcess);
				if (allThingsOnLocation.Count > 0)
				{
					foreach (Thing currentThing in allThingsOnLocation)
					{
						string label = currentThing.def.label;
						if (FillData<Plant>(location, label, CategoryOfObjects.Plants, currentThing))
							continue;
						if (FillData<Pawn>(location, label, CategoryOfObjects.Pawns, currentThing))
							continue;

						if (FillData<Corpse>(location, label, CategoryOfObjects.Corpses, currentThing))
						{
							CompRottable comp = ((Corpse)currentThing).GetComp<CompRottable>();
							int currentTicksRemain = comp == null ? 0 : Mathf.RoundToInt(comp.PropsRot.TicksToRotStart - comp.RotProgress);
							currentTicksRemain = currentTicksRemain > 0 ? currentTicksRemain : 0;
							if (CorpsesTimeRemainDict.ContainsKey(label))
							{
								if (CorpsesTimeRemainDict[label] > currentTicksRemain && currentTicksRemain > 0)
									CorpsesTimeRemainDict[label] = currentTicksRemain;
							}
							else
								CorpsesTimeRemainDict.Add(label, currentTicksRemain);
							continue;
						}

						if (currentThing.Stuff != null)
							label = $"{label} ({currentThing.Stuff.LabelAsStuff})";

						if (FillData<Building>(location, label, CategoryOfObjects.Buildings, currentThing))
							continue;
						FillData<Thing>(location, currentThing.def.label, CategoryOfObjects.Other, currentThing);
					}
				}
			}
			CategoriesDict.Add(CategoryOfObjects.All, new List<string>());
			var AllObjects = (from k in CategoriesDict.Keys where k != CategoryOfObjects.All && k != CategoryOfObjects.Favorites select CategoriesDict[k]);
			foreach (var list in AllObjects)
			{
				CategoriesDict[CategoryOfObjects.All].AddRange(list);
			}
			if (!this.LocationsDict.ContainsKey(this.ThingToSeek))
				this.ThingToSeek = string.Empty;
#if DEBUG
			sw.Stop();
			DebugMessage($"Object Seeker has filled data for {sw.ElapsedMilliseconds} ms"); 
#endif
		}

		private bool FillData<T>(IntVec3 location, string label, CategoryOfObjects category, Thing currentThing = null)
		{
			if (currentThing is T || currentThing == null)
			{
				if (CategoriesDict.ContainsKey(category))
				{
					if (!CategoriesDict[category].Contains(label))
						CategoriesDict[category].Add(label);
				}
				else
					CategoriesDict.Add(category, new List<string>(new string[] { label }));

				if (LocationsDict.ContainsKey(label))
					LocationsDict[label].Add(location);
				else
					LocationsDict.Add(label, new List<IntVec3>(new IntVec3[] { location }));
				
				if (!ThingsDict.ContainsKey(label))
					ThingsDict.Add(label, currentThing);
				return true;
			}
			else
				return false;
		}

		int IComparer<string>.Compare(string x, string y)
		{
			if (CorpsesTimeRemainDict[x] > 0 && CorpsesTimeRemainDict[x] < CorpsesTimeRemainDict[y] || CorpsesTimeRemainDict[y] == 0 && CorpsesTimeRemainDict[x] > 0)
				return -1;
			if (CorpsesTimeRemainDict[x] > CorpsesTimeRemainDict[y] || CorpsesTimeRemainDict[x] == 0 && CorpsesTimeRemainDict[y] > 0)
				return 1;
			else
				return 0;
		}

		public void ExposeData()
		{
			List<string> fav = CategoriesDict[CategoryOfObjects.Favorites];
			Scribe_Collections.Look<string>(ref fav, "ObjectSeeker_favourites");
			CategoriesDict[CategoryOfObjects.Favorites] = fav;
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
			Other,
		}
	}
}
