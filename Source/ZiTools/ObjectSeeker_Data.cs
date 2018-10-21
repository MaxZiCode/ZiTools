using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace ZiTools
{
	public class ObjectSeeker_Data : IComparer<string>
	{
		readonly List<FloatMenuOption> _floatMenuCategoriesOpt;

		public ObjectSeeker_Data()
		{
			LocationsDict = new Dictionary<string, List<IntVec3>> { { String.Empty, new List<IntVec3>() } };
			CategoriesDict = new Dictionary<CategoryOfObjects, List<string>>();
			CorpsesTimeRemainDict = new Dictionary<string, int>();
			ThingToSeek = string.Empty;
			SelectedCategory = CategoryOfObjects.Buildings;
			MapInProcess = Find.CurrentMap;

			_floatMenuCategoriesOpt = new List<FloatMenuOption>()
				{
					new FloatMenuOption("ZiT_BuildingCategoryLabel".Translate(), delegate{ SelectedCategory = CategoryOfObjects.Buildings; }),
					new FloatMenuOption("ZiT_TerrainCategoryLabel".Translate(), delegate{ SelectedCategory = CategoryOfObjects.Terrains; }),
					new FloatMenuOption("ZiT_PlantCategoryLabel".Translate(), delegate{ SelectedCategory = CategoryOfObjects.Plants; }),
					new FloatMenuOption("ZiT_PawnsCategoryLabel".Translate(), delegate{ SelectedCategory = CategoryOfObjects.Pawns; }),
					new FloatMenuOption("ZiT_СorpsesCategoryLabel".Translate(), delegate{ SelectedCategory = CategoryOfObjects.Corpses; }),
					new FloatMenuOption("ZiT_OtherCategoryLabel".Translate(), delegate{ SelectedCategory = CategoryOfObjects.Other; })
				};
		}

		public List<IntVec3> Positions { get => LocationsDict[ThingToSeek]; }

		public Dictionary<string, List<IntVec3>> LocationsDict { get; set; }
		public Dictionary<CategoryOfObjects, List<string>> CategoriesDict { get; set; }
		public Dictionary<string, int> CorpsesTimeRemainDict { get; set; }

		public Map MapInProcess { get; set; }

		public FloatMenu CategoryMenu { get => new FloatMenu(_floatMenuCategoriesOpt); }

		public string ThingToSeek { get; set; }
		public string SelectedCategoryName { get => _floatMenuCategoriesOpt[(int)SelectedCategory].Label; }

		public bool WindowIsOpen { get; set; }

		public CategoryOfObjects SelectedCategory { get; set; }

		public void FindAllThings()
		{
			this.MapInProcess = Find.CurrentMap;

			CategoriesDict.Clear();
			CorpsesTimeRemainDict.Clear();
			LocationsDict.Clear();
			LocationsDict.Add(String.Empty, new List<IntVec3>());

			foreach (IntVec3 location in MapInProcess.AllCells)
			{
				if (MapInProcess.fogGrid.IsFogged(location))
					continue;
				FillData<TerrainDef>(location, location.GetTerrain(MapInProcess).label, CategoryOfObjects.Terrains);
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
			Buildings,
			Terrains,
			Plants,
			Pawns,
			Corpses,
			Other,
		}
	}
}
