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
		//BOOLS
		public static bool isUIon = false;
		private static bool isGameLoaded = false;
		//ACTIVE BOOLS
		public static bool isChoosingCulture = false;
		//OBJECTS
		private static int windowIDs = 1530;
		private static MapBox instance = null;
		public static Culture selectedCulture;
		public static Rect mainWindowRect = new Rect(0f, 0f, 140f, 150f);

		void Start()
        {
			isGameLoaded = Config.gameLoaded;
			instance = MapBox.instance;
        }
		void OnGUI()
		{
			if (WorldBoxMod.isUIon)
			{
				CultureUI();
			}
		}

		void Update()
		{
			if (WorldBoxMod.isGameLoaded)
			{
				CultureUpdate();
			}
		}

		public void CultureUpdate()
		{
			WorldTile mouseTile = null;
			if (Input.GetKeyDown(KeyCode.U) )
			{
				WorldBoxMod.isUIon = !WorldBoxMod.isUIon;
			}
			if(Input.GetMouseButtonDown(0))
            {
				mouseTile = instance.getMouseTilePos();
            }
			else { return; }
			if(WorldBoxMod.isChoosingCulture)
            {
				if(mouseTile.zone.culture == null)
                {
					return;
                }
				WorldBoxMod.selectedCulture = mouseTile.zone.culture;
				WorldTip.showNow("Selected " + WorldBoxMod.selectedCulture.name, true, "top", 3f);
				WorldBoxMod.isChoosingCulture = false;
				Config.lockGameControls = false;
			}
		}

		void CultureUI()
        {
			//windowIDs = 1530;
			WorldBoxMod.mainWindowRect = GUI.Window(windowIDs, WorldBoxMod.mainWindowRect, CultureWindow, "CultureUI");
			//++windowIDs;
		}

		public static void CultureWindow(int windowID)
		{
			string selectTitle = "Select Culture";
			if(WorldBoxMod.selectedCulture != null)
            {
				selectTitle = WorldBoxMod.selectedCulture.name;
            }
			if(GUI.Button(new Rect(2f, 20f, 138f, 15f), selectTitle))
            {
				WorldBoxMod.isChoosingCulture = !WorldBoxMod.isChoosingCulture;
				if(WorldBoxMod.isChoosingCulture) { WorldTip.showNow("Selecting Culture", true, "top", 3f); }
				Config.lockGameControls = !Config.lockGameControls;
            }
			if(WorldBoxMod.selectedCulture != null)
            {
				if (GUI.Button(new Rect(2f, 40f, 138f, 18f), "Finish Current Tech"))
				{
					
				}
				if (GUI.Button(new Rect(2f, 60f, 138f, 18f), "Expand Influence"))
				{

				}
				if (GUI.Button(new Rect(2f, 80f, 138f, 18f), "Reduce Influence"))
				{

				}
				if (GUI.Button(new Rect(2f, 100f, 138f, 18f), "Unlock All Tech"))
				{

				}
				if (GUI.Button(new Rect(2f, 120f, 138f, 18f), "Unlock All Tech"))
				{

				}
			}
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
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


