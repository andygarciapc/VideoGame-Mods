using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CultureUI
{
	public class WorldBoxMod : MonoBehaviour
	{
		//UI Bools
		public static bool ShowHideMaster = false;
		public static bool ShowHide = false;
		public static bool CultureUIOn = false;
		public static bool selectingCulture = false;
		public static bool expandingCulture = false;
		public static bool reducingCulture = false;
		public static bool changeCultureColor = false;
		public static bool creatingCulture = false;
		public static bool deletingCulture = false;
		//Culture UI
		private static Culture selectedCulture;
		//Other
		private static bool culturemodOn = false;

		void OnGUI()
		{
			WorldBoxMod.CultureUI();
		}

		void Update()
		{
			WorldBoxMod.culturemodOn = Config.gameLoaded;

			if (WorldBoxMod.culturemodOn)
			{
				CultureButtonInitialize();
			}
		}

		public void CultureButtonInitialize()
		{
			bool uPress = Input.GetKeyDown("u");

			if (uPress)
			{
				WorldBoxMod.ShowHideMaster = !WorldBoxMod.ShowHideMaster;
			}
			if (WorldBoxMod.expandingCulture)
			{
				ExpandCulture(WorldBoxMod.selectedCulture);
			}
			if (WorldBoxMod.reducingCulture)
			{
				ReduceCulture();
			}
			if (WorldBoxMod.changeCultureColor)
			{
				RandomCultureColor(WorldBoxMod.selectedCulture);
				WorldBoxMod.changeCultureColor = !WorldBoxMod.changeCultureColor;
			}
			if (WorldBoxMod.creatingCulture)
			{
				if (Input.GetMouseButtonDown(0))
				{
					City pCity = MapBox.instance.getMouseTilePos().zone.city;
					Race pRace = Reflection.GetField(pCity.GetType(), pCity, "race") as Race;
					Culture newCulture = CultureManager.instance.newCulture(pRace, pCity);
					ExpandCulture(newCulture);
					WorldBoxMod.creatingCulture = !WorldBoxMod.creatingCulture;
				}
			}
			if(WorldBoxMod.deletingCulture)
            {
				DeleteCulture(WorldBoxMod.selectedCulture);
				WorldBoxMod.deletingCulture = !WorldBoxMod.deletingCulture;
            }
		}

		public static void CultureUI()
		{
			Rect buttonSize = new Rect(Screen.width - 100, 60, 100f, 20f);
			WorldTile mousePos = MapBox.instance.getMouseTilePos();

			if (WorldBoxMod.ShowHideMaster)
			{
				if (GUI.Button(new Rect(new Vector2(buttonSize.x, buttonSize.y + 20), new Vector2(buttonSize.width, buttonSize.height)), "CultureUI"))
				{
					WorldBoxMod.CultureUIOn = !WorldBoxMod.CultureUIOn;
				}// CULTUREUI END
				if (WorldBoxMod.CultureUIOn)
				{
					if (GUI.Button(new Rect(Screen.width - 240, 0, 120f, 20f), "Select Culture"))
					{
						WorldBoxMod.selectingCulture = true;
					}
					if (WorldBoxMod.selectingCulture)
					{
						if (Input.GetMouseButtonDown(0))
						{
							WorldBoxMod.selectedCulture = mousePos.zone.culture;
							if (WorldBoxMod.selectedCulture != null)
							{
								WorldTip.showNow(WorldBoxMod.selectedCulture.name + " is now selected", true, "top", 1f);
								WorldBoxMod.selectingCulture = false;
							}
							else
							{
								WorldTip.showNow("No culture selected", true, "top", 1f);
								WorldBoxMod.selectingCulture = false;
							}
						}
					}
					if (WorldBoxMod.selectedCulture != null)
					{
						float cultureUIx = Screen.width - 240;
						if (GUI.Button(new Rect(cultureUIx, 20, 120f, 20f), "Instant Research"))
						{
							WorldBoxMod.selectedCulture.addFinishedTech(WorldBoxMod.selectedCulture.researching_tech);
							WorldBoxMod.selectedCulture.researching_tech = string.Empty;
							WorldBoxMod.selectedCulture.research_progress = 0f;
							WorldBoxMod.selectedCulture.researching_tech = WorldBoxMod.selectedCulture.findNextTechToResearch();
							WorldTip.showNow(WorldBoxMod.selectedCulture.name + " has advanced in tech", true, "top", 1f);
						}

						if (GUI.Button(new Rect(cultureUIx, 40, 120f, 20f), "Expand Culture"))
						{
							if(WorldBoxMod.reducingCulture)
                            {
								WorldBoxMod.reducingCulture = !WorldBoxMod.reducingCulture;
                            }
							WorldBoxMod.expandingCulture = !WorldBoxMod.expandingCulture;
						}

						if (GUI.Button(new Rect(cultureUIx, 60, 120f, 20f), "Reduce Culture"))
						{
							if(WorldBoxMod.expandingCulture)
                            {
								WorldBoxMod.expandingCulture = !WorldBoxMod.expandingCulture;
                            }
							WorldBoxMod.reducingCulture = !WorldBoxMod.reducingCulture;
						}

						if (GUI.Button(new Rect(cultureUIx, 80, 120f, 20f), "Change Color"))
						{
							WorldBoxMod.changeCultureColor = !WorldBoxMod.changeCultureColor;
						}

						if (GUI.Button(new Rect(cultureUIx, 100, 120f, 20f), "New Culture"))
						{
							WorldBoxMod.creatingCulture = !WorldBoxMod.creatingCulture;
						}

						if (GUI.Button(new Rect(cultureUIx, 120, 120f, 20f), "Delete Culture"))
						{
							WorldBoxMod.deletingCulture = !WorldBoxMod.deletingCulture;
						}

					}
				}
			}

		}

		public void DeleteCulture(Culture selectedCulture)
        {
			selectedCulture.clearZones();
			CultureManager.instance.list.Remove(selectedCulture);
			CultureManager.instance.dict.Remove(selectedCulture.id);
        }

		public void ExpandCulture(Culture selectedCulture)
        {
			TileZone mousetileZone = MapBox.instance.getMouseTilePos().zone;
			List<WorldTile> zoneTiles;
			bool gPress = Input.GetKey("g");
			if (gPress)
            {
				zoneTiles = mousetileZone.tiles;
				selectedCulture.addZone(MapBox.instance.getMouseTilePos().zone);
				for(int i = 0; i < zoneTiles.Count; ++i)
                {
					for (int j = 0; j < zoneTiles[i].units.Count; ++i)
                    {
						SetActorCulture(selectedCulture, zoneTiles[i].units[j]);
					}

                }
            }
			if (WorldBoxMod.creatingCulture)
            {
				zoneTiles = mousetileZone.tiles;
				selectedCulture.addZone(MapBox.instance.getMouseTilePos().zone);
				for(int i = 0; i < zoneTiles.Count; ++i)
                {
					for (int j = 0; j < zoneTiles[i].units.Count; ++i)
					{
						SetActorCulture(selectedCulture, zoneTiles[i].units[j]);
					}

				}
            }
        }

		public void RandomCultureColor(Culture selectedCulture)
        {
			Race selectedRace = AssetManager.raceLibrary.get(selectedCulture.race);
			selectedCulture.color = selectedRace.culture_colors.GetRandom<string>();
			selectedCulture.color32 = new Color32(
			(byte)UnityEngine.Random.Range(0, 255),
			(byte)UnityEngine.Random.Range(0, 255),
			(byte)UnityEngine.Random.Range(0, 255),
			255
			);
		}

		public void ReduceCulture()
        {
			TileZone mousetileZone = MapBox.instance.getMouseTilePos().zone;
			List<WorldTile> zoneTiles;
			bool gPress = Input.GetKey("g");
			if(gPress)
            {
				zoneTiles = mousetileZone.tiles;
				WorldBoxMod.selectedCulture.removeZone(MapBox.instance.getMouseTilePos().zone);
				for (int i = 0; i < zoneTiles.Count; ++i)
				{
					for (int j = 0; j < zoneTiles[i].units.Count; ++i)
					{
						SetActorCulture(null, zoneTiles[i].units[j]);
					}

				}
			}
        }

		void SetActorCulture(Culture culture, Actor actor)
        {
			actor.CallMethod(
				"setCulture", new object[] { culture }
				);
        }

	}


	public static class Reflection
	{
		public static object CallMethod(this object o, string methodName, params object[] args)
		{
			MethodInfo method = o.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
			bool flag = method != null;
			object result;
			if (flag)
			{
				result = method.Invoke(o, args);
			}
			else
			{
				result = null;
			}
			return result;
		}

		public static object GetField(Type type, object instance, string fieldName)
		{
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo field = type.GetField(fieldName, bindingAttr);
			return field.GetValue(instance);
		}

		public static void SetField<T>(object originalObject, string fieldName, T newValue)
		{
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo field = originalObject.GetType().GetField(fieldName, bindingAttr);
			field.SetValue(originalObject, newValue);
		}
	}
}


