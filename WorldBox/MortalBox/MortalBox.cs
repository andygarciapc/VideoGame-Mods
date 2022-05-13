//Author information
//  Author name: Andy Garcia
//  Author email: andygarciapc@gmail.com
//
//Mod information
//  Mod name: MortalBox
//  Date program began: 2021-Nov-05
//  Date of last update: 0000-Month-00
//
//Purpose
//  Allow the player to control a team of creatures.
//
//This file
//   File name: MortalBox.cs
//   Language: C#
//
//
//===== Begin code area ================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;

namespace MortalBox
{
    public class WorldBoxMod : MonoBehaviour
    {
		// GAME BOOLS
		private static bool isControlling = false;
		// CORE BOOLS
		private static bool isGameLoaded = false;
		//UI BOOLS
		private static bool isUIon = false;
		private static bool isShowingTroops = false;
		private static bool isGoingTo = false;
		private static bool isSelectingTroops = false;
		private static bool releasingUnit = false; 
		private static bool toControl = false;
		//GLOBAL VARIABLES
		public static Rect mainButtonWindowRect = new Rect(0f, 0f, 124f, 97f);
		public static Rect mainTroopWindowRect = new Rect(124f, 0f, 19f, 197f);
		public static Rect controlledUnitWindowRect = new Rect(0f, 0f, 124f, 82f);
		public static Rect button = new Rect(0f, 0f, 80f, 15f);
		public static double curStamina = 0f;
		const double maxStamina = 100f;

		private static UInt16 troopCount = 0;
		private static Actor theChosen;
		private static List<Actor> troops = new List<Actor>(troopCount);

		void Start()
        {
			isGameLoaded = Config.gameLoaded;
			MortalBoxTraitBoot();
        }

		void OnGUI()
        {
			if(WorldBoxMod.isUIon)
            {
				MortalBoxUI();
            }
		}

		void Update()
        {
			if(isGameLoaded)
            {
				MortalBox();
            }
        }

		private void MortalBox()
        {
			WorldTile mouseTile = null;
			if (Input.GetKeyDown(KeyCode.U))
			{
				WorldBoxMod.isUIon = !WorldBoxMod.isUIon;
				if(!WorldBoxMod.isUIon)
                {
					SetUIFalse();
                }
			}

			if(WorldBoxMod.isSelectingTroops || WorldBoxMod.isGoingTo)
            {
				if (Input.GetMouseButton(0))
				{
					mouseTile = MapBox.instance.getMouseTilePos();
				}
            }
			if (WorldBoxMod.isSelectingTroops)
            {
				TroopSelector(mouseTile);
            }
			if (WorldBoxMod.isGoingTo)
			{
				SendTroops(WorldBoxMod.troops, mouseTile);
			}
			if (WorldBoxMod.isControlling)
            {
				bool escPress = Input.GetKeyDown(KeyCode.Escape);
				UnitController(WorldBoxMod.theChosen);
				if(escPress)
                {
					WorldTip.showNow("Released Control Of " + WorldBoxMod.theChosen.coloredName, true, "top", 3f);
					WorldBoxMod.theChosen = null;
					WorldBoxMod.isControlling = false;
					Config.setControllableCreature(null);
				}
            }
        }

		private void MortalBoxUI()
        {
			WorldBoxMod.mainButtonWindowRect = GUI.Window(0, WorldBoxMod.mainButtonWindowRect, MortalBoxButtonWindow, "MortalBox");
			if (WorldBoxMod.isShowingTroops && WorldBoxMod.troopCount > 0)
			{
				WorldBoxMod.mainTroopWindowRect = GUI.Window(1, WorldBoxMod.mainTroopWindowRect, MortalBoxTroopWindow, "");
			}
			if(WorldBoxMod.isControlling)
            {
				WorldBoxMod.controlledUnitWindowRect = GUI.Window(2, WorldBoxMod.controlledUnitWindowRect, MortalBoxUnitWindow, "");
			}
		}

		private void MortalBoxTraitBoot()
        {
			Dictionary<string, string> lText = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "localizedText") as Dictionary<string, string>;
			ActorTrait incarnation = new ActorTrait();
			incarnation.id = "incarnation";
			incarnation.icon = AssetManager.traits.get("blessed").icon;
			incarnation.baseStats.mod_armor = 70f;
			incarnation.baseStats.mod_attackSpeed = 150f;
			incarnation.baseStats.mod_speed = 200f;
			incarnation.baseStats.mod_health = 3000f;
			incarnation.baseStats.mod_crit = 50f;
			incarnation.baseStats.range = 30f;
			incarnation.baseStats.scale = 0.13f;
			incarnation.baseStats.mod_damage = 65f;
			incarnation.baseStats.intelligence = 50;
			incarnation.baseStats.stewardship = 50;
			incarnation.baseStats.warfare = 50;
			incarnation.baseStats.diplomacy = 50;
			incarnation.action_death = new WorldAction(ActionLibrary.tryToGrowTree);
			incarnation.inherit = 0f;
			incarnation.birth = 0f;
			AssetManager.traits.add(incarnation);
			lText.Add("trait_" + incarnation.id, "Incarnate");
			lText.Add("trait_" + incarnation.id + "_info", "Crafted by God Himself");

			ActorTrait sprinting = new ActorTrait();
			sprinting.id = "sprinting";
			sprinting.icon = AssetManager.traits.get("agile").icon;
			sprinting.baseStats.mod_speed = 200f;
			sprinting.baseStats.dodge = 20f;
			sprinting.inherit = 0f;
			sprinting.birth = 0f;  
			AssetManager.traits.add(sprinting);
			lText.Add("trait_" + sprinting.id, "Sprinting");
			lText.Add("trait_" + sprinting.id + "_info", "Run forest, run");

			ActorTrait leader = new ActorTrait();
			leader.id = "leader";
			leader.icon = AssetManager.traits.get("veteran").icon;
			leader.baseStats.diplomacy = 30;
			leader.baseStats.stewardship = 30;
			leader.baseStats.warfare = 30;
			leader.inherit = 5f;
			leader.birth = 5f;
			AssetManager.traits.add(leader);
			lText.Add("trait_" + leader.id, "Leader");
			lText.Add("trait_" + leader.id + "_info", "They say some are born to lead");

			ActorTrait superhuman = new ActorTrait();
			superhuman.id = "superhuman";
			superhuman.icon = AssetManager.traits.get("strong_minded").icon;
			superhuman.baseStats.mod_armor = 50f;
			superhuman.baseStats.mod_attackSpeed = 70f;
			superhuman.baseStats.mod_speed = 100f;
			superhuman.baseStats.mod_health = 2500f;
			superhuman.baseStats.mod_crit = 50f;
			superhuman.baseStats.range = 15f;
			superhuman.baseStats.mod_damage = 50f;
			superhuman.baseStats.scale = .04f;
			superhuman.inherit = 5f;
			superhuman.birth = .005f;
			AssetManager.traits.add(superhuman);
			lText.Add("trait_" + superhuman.id, "Super Human");
			lText.Add("trait_" + superhuman.id + "_info", "Maybe they are from another planet... or something?");
		}

		public static void MortalBoxButtonWindow(int windowID)
		{
			if (GUI.Button(new Rect(2f, 20f, 120f, 15f), "Select Troops"))
			{
				WorldBoxMod.isSelectingTroops = !WorldBoxMod.isSelectingTroops;
				if (WorldBoxMod.isSelectingTroops) { WorldTip.showNow("Selecting Troops", true, "top", 3f); }
				else { WorldTip.showNow("Finished Selecting", true, "top", 3f); }
				Config.lockGameControls = !Config.lockGameControls;
			}

			if (GUI.Button(new Rect(2f, 35f, 120f, 15f), "Show Troops"))
			{
				WorldBoxMod.isShowingTroops = !WorldBoxMod.isShowingTroops;
			}

			if (GUI.Button(new Rect(2f, 50f, 120f, 15f), "Go To"))
			{
				WorldBoxMod.isGoingTo = !WorldBoxMod.isGoingTo;
			}

			if (GUI.Button(new Rect(2f, 65f, 120f, 15f), "Troops"))
			{

			}

			if (GUI.Button(new Rect(2f, 80f, 120f, 15f), "Troops"))
			{

			}

			if (GUI.Button(new Rect(2f, 80f, 120f, 15f), "Troops"))
			{

			}

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public static void MortalBoxTroopWindow(int windowID)
		{
			float x = 2f, y = 15f, width = 84f;
			int colRepeating = 0;

			if (GUI.Button(new Rect(x, 2, 80f, 15f), "Possess"))
			{
				WorldBoxMod.toControl = !WorldBoxMod.toControl;
				if(!WorldBoxMod.toControl) { WorldBoxMod.isControlling = false; WorldBoxMod.theChosen = null; Config.setControllableCreature(null); }
			}
			if (GUI.Button(new Rect(x+80, 2, 80f, 15f), "Release"))
			{
				WorldBoxMod.releasingUnit = !WorldBoxMod.releasingUnit;
			}

			for (int i = 0; i < WorldBoxMod.troopCount; ++i)
			{
				if (i % 12 == 0)
				{
					colRepeating = 0;
					x += 80f;
					width += 80f;
				}
				ActorStatus playerStatus = Reflection.GetField(WorldBoxMod.troops[i].GetType(), WorldBoxMod.troops[i], "data") as ActorStatus;
				if(!playerStatus.alive)
                {
					WorldBoxMod.troops.Remove(WorldBoxMod.troops[i]);
                }

				if (GUI.Button(new Rect(x - 80, y + (colRepeating * 15f), 80f, 15f), WorldBoxMod.troops[i].coloredName))
				{
					if(WorldBoxMod.toControl)
                    {
						Config.setControllableCreature(WorldBoxMod.troops[i]);
						WorldBoxMod.theChosen = WorldBoxMod.troops[i];
						WorldBoxMod.isControlling = true;
						WorldTip.showNow("Possessed " + WorldBoxMod.theChosen.coloredName, true, "top", 3f);
                    }
					if (WorldBoxMod.releasingUnit)
					{
						WorldBoxMod.troops.Remove(WorldBoxMod.troops[i]);
						--WorldBoxMod.troopCount;
					}
				}
				++colRepeating;
			}
			WorldBoxMod.mainTroopWindowRect.width = width;
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public static void MortalBoxUnitWindow(int windowID)
        {
			GUI.backgroundColor = Color.white;
			GUI.contentColor = Color.black;
			BaseStats playerStats = Reflection.GetField(WorldBoxMod.theChosen.GetType(), WorldBoxMod.theChosen, "curStats") as BaseStats;
			string presentedHealth = WorldBoxMod.theChosen.base_data.health + "/" + playerStats.health;
			float x = 2f, y = 2f, width = 120f;
			GUI.Label(new Rect(x,y,width, 30f), "Name: " + WorldBoxMod.theChosen.coloredName);
			GUI.Label(new Rect(x,y+31f,width,30f), "Health: " + presentedHealth);
			GUI.Label(new Rect(x, y + (31 * 2), width, 30f), "Stamina: " + WorldBoxMod.maxStamina);

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public static void UnitController(Actor unit)
        {
			WorldTile actorTile = unit.currentTile;
			if(unit == null)
            {
				return;
            }
			Camera.main.transform.position = new Vector2(WorldBoxMod.theChosen.transform.position.x, WorldBoxMod.theChosen.transform.position.y);
			bool upLeft = (Input.GetKey("w") && Input.GetKey("a"));
			bool upRight = (Input.GetKey("w") && Input.GetKey("d"));
			bool downLeft = (Input.GetKey("s") && Input.GetKey("a"));
			bool downRight = (Input.GetKey("s") && Input.GetKey("d"));
			bool up = Input.GetKey("w") && !upLeft && !upRight;
			bool down = Input.GetKey("s") && !downLeft && !downRight;
			bool left = Input.GetKey("a") && !upLeft && !downLeft;
			bool right = Input.GetKey("d") && !upRight && !downRight;
			int pX = 0, pY = 0;
			if (upLeft)
			{
				pX = actorTile.x - 1;
				pY = actorTile.y + 1;
			}
			if (upRight)
			{
				pY = actorTile.y + 1;
				pX = actorTile.x + 1;
			}
			if (downLeft)
			{
				pY = actorTile.y - 1;
				pX = actorTile.x - 1;
			}
			if (downRight)
			{
				pY = actorTile.y - 1;
				pX = actorTile.x + 1;
			}
			//END DIAGONAL
			//STRAIGHT DIRECTIONS
			if (up)
			{
				pY = actorTile.y + 1;
				pX = actorTile.x;
			}
			if (left)
			{
				pX = actorTile.x - 1;
				pY = actorTile.y;
			}
			if (down)
			{
				pX = actorTile.x;
				pY = actorTile.y - 1;
			}
			if (right)
			{
				pX = actorTile.x + 1;
				pY = actorTile.y;
			}
			bool isMoving = (pX != 0) && (pY != 0);
			if (isMoving)
			{
				unit.goTo(MapBox.instance.GetTile(pX, pY), true, true);
			}
            else
            {
				unit.cancelAllBeh(null);
				unit.stopMovement();
			}
		}

		public static void SendTroops(List<Actor> troops, WorldTile targetTile)
        {
			if(targetTile == null)
            {
				return;
            }
			foreach(var i in troops)
            {
				i.goTo(targetTile, true, true);
            }
        }

		private void TroopSelector(WorldTile mouseTile)
        {
            if (mouseTile != null)
            {
                foreach (var i in mouseTile.units)
                {
                    Actor newTroop = i;
                    if (newTroop != null)
                    {
                        bool troopRepeated = false;
                        foreach (var j in WorldBoxMod.troops)
                        {
                            if (j == newTroop)
                            {
                                troopRepeated = true;
                            }
                        }
                        if (!troopRepeated)
                        {
                            ++WorldBoxMod.troopCount;
                            WorldBoxMod.troops.Add(newTroop);
                        }
                        else
                        {

                        }
                    }
                }
            }
        }

		public void SetUIFalse()
        {
			WorldBoxMod.isUIon = false;
			if(WorldBoxMod.isSelectingTroops)
            {
				WorldBoxMod.isSelectingTroops = false;
				Config.lockGameControls = false;
			}
			WorldBoxMod.isShowingTroops = false;
			WorldBoxMod.toControl = false;
			WorldBoxMod.releasingUnit = false;
        }

    } // End MortalBox

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
	} // End Reflection
}
