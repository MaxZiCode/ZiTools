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
	public class ObjectSeeker_Data : IComparer<string>
	{
		public ObjectSeeker_Data()
		{
			LocationsDict = new Dictionary<string, List<IntVec3>> { { String.Empty, new List<IntVec3>() } };
			BuildableDefDict = new Dictionary<string, BuildableDef>();
			CategoriesDict = new Dictionary<CategoryOfObjects, List<string>>();
			CorpsesTimeRemainDict = new Dictionary<string, int>();
			ThingToSeek = string.Empty;
			SelectedCategory = CategoryOfObjects.All;
			MapInProcess = Find.CurrentMap;
		}

		public List<IntVec3> Positions { get => LocationsDict[ThingToSeek]; }

		readonly Dictionary<CategoryOfObjects, string> _namesOfCategories = new Dictionary<CategoryOfObjects, string>
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
		public Dictionary<string, BuildableDef> BuildableDefDict { get; set; }
		public Dictionary<CategoryOfObjects, List<string>> CategoriesDict { get; set; }
		public Dictionary<string, int> CorpsesTimeRemainDict { get; set; }

		public Map MapInProcess { get; set; }

		public string ThingToSeek { get; set; }
		public string SelectedCategoryName { get => _namesOfCategories[SelectedCategory]; }

		public bool WindowIsOpen { get; set; }

		public CategoryOfObjects SelectedCategory { get; set; }

		public void FindAllThings()
		{
#if DEBUG
			Stopwatch sw = Stopwatch.StartNew(); 
#endif
			this.MapInProcess = Find.CurrentMap;

			CategoriesDict.Clear();
			CorpsesTimeRemainDict.Clear();
			LocationsDict.Clear();
			LocationsDict.Add(String.Empty, new List<IntVec3>());

			foreach (IntVec3 location in MapInProcess.AllCells)
			{
				if (MapInProcess.fogGrid.IsFogged(location))
					continue;
				TerrainDef ter = location.GetTerrain(MapInProcess);
				FillData<TerrainDef>(location, ter.label, CategoryOfObjects.Terrains, def: ter);
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
							int currentTicksRemain = Mathf.RoundToInt(comp.PropsRot.TicksToRotStart - comp.RotProgress);
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
			if (!this.LocationsDict.ContainsKey(this.ThingToSeek))
				this.ThingToSeek = string.Empty;
#if DEBUG
			sw.Stop();
			DebugMessage($"Object Seeker has filled data for {sw.ElapsedMilliseconds} ms"); 
#endif
		}

		private bool FillData<T>(IntVec3 location, string label, CategoryOfObjects category, Thing currentThing = null, BuildableDef def = null)
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

				if (def == null)
					def = currentThing.def;
				if (!BuildableDefDict.ContainsKey(label))
					BuildableDefDict.Add(label, def);
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
