using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using ai;
using ai.behaviours;

namespace Incarnate
{
	public class WorldBoxMod : MonoBehaviour
	{
		//UI Bools
		public static bool ShowHideMaster = false;
		public static bool ShowHide = false;
		//Control Bools
		private static bool isOn = false;
		private static bool isControlling = false;
		private static Actor playerActor;
		private static bool aiOn = false;
		private static bool camOff = false;
		private static bool runOnce = false;
		private static bool hadPeaceful = false;
		//private static Dictionary<string, ScrollWindow> wboxWindows;


		void Awake()
        {
			
		}	

		void Start()
        {
			IncarnateTraitBoot();
		}
		void Update()
		{
			isOn = Config.gameLoaded;
			if (isOn)
			{
				//TestMod();
				ControlButtonInitialize();
				//IncarnateTraitBoot();
				if (WorldBoxMod.isControlling)
                {
					ActorController();
					CameraController();
                }
			}
            else
            {
				Clean();
            }
		}

		void OnGUI()
        {
			WorldBoxMod.IncarnateGUI();
        }

		public void IncarnateTraitBoot()
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
		void TestMod()
        {	

			bool cPress = Input.GetKeyDown("c");
			MapBox instance = MapBox.instance;
			WorldTile mouseTile = instance.getMouseTilePos();

			if(cPress)
            {
				WorldBoxMod.playerActor = instance.getActorNearCursor();
				if (WorldBoxMod.playerActor != null)
				{
					
				}
			}
        }

		void ControlButtonInitialize()
		{
			bool hPress = Input.GetKeyDown("h");
			bool cPress = Input.GetKeyDown("c");
			bool uPress = Input.GetKeyDown("u");
			GodPower spawnHuman = AssetManager.powers.get("humans");
			WorldTile mousePos = MapBox.instance.getMouseTilePos();



			if (!WorldBoxMod.isControlling)//CHECK C/H KEY TO CONTROL
			{
				if (hPress)
				{
					WorldBoxMod.playerActor = MapBox.instance.createNewUnit(spawnHuman.actorStatsId, mousePos, "", spawnHuman.actorSpawnHeight, null);
					if (WorldBoxMod.playerActor != null)
					{
						//Config.setControllableCreature(WorldBoxMod.playerActor);
						WorldBoxMod.isControlling = true;
						WorldBoxMod.playerActor.addTrait("incarnation");
						WorldTip.showNow("Incarnated as " + playerActor.coloredName, true, "top", 3f);
						ActorManager(WorldBoxMod.playerActor);
					}

				}
				if (cPress)
				{

					WorldBoxMod.playerActor = MapBox.instance.getActorNearCursor();

					if (WorldBoxMod.playerActor != null)
					{
						//Config.setControllableCreature(WorldBoxMod.playerActor);
						WorldBoxMod.isControlling = true;
						WorldTip.showNow("Now controlling " + playerActor.coloredName, true, "top", 3f);
						ActorManager(WorldBoxMod.playerActor);
					}
				}
				if (uPress)
                {
					WorldBoxMod.ShowHideMaster = !WorldBoxMod.ShowHideMaster;
                }
			}
		} 

		public void CameraController()
        {
			bool lPress = Input.GetKeyDown("l");
			if (lPress)
            {
				WorldBoxMod.camOff = !WorldBoxMod.camOff;
            }
			if(WorldBoxMod.camOff)
            {
				if (Config.controllableUnit != null)
				{
					Config.setControllableCreature(null);
				}
			}
			else
            {
				if (WorldBoxMod.isControlling)
				{
					if (Config.controllableUnit == null)
					{
						Config.setControllableCreature(WorldBoxMod.playerActor);
					}
					Camera.main.transform.position = new Vector2(WorldBoxMod.playerActor.transform.position.x, WorldBoxMod.playerActor.transform.position.y);
				}
			}
        }

		public void ActorController()
		{
			WorldTile actorTile = WorldBoxMod.playerActor.currentTile;
			//actorTile.setBurned(-1);						// BURN TRAIL ON PLAYER

			//----------------------------------------------------------------------------
			bool upLeft = (Input.GetKey("w") && Input.GetKey("a"));
			bool upRight = (Input.GetKey("w") && Input.GetKey("d"));
			bool downLeft = (Input.GetKey("s") && Input.GetKey("a"));
			bool downRight = (Input.GetKey("s") && Input.GetKey("d"));
			bool up = Input.GetKey("w") && !upLeft && !upRight;
			bool down = Input.GetKey("s") && !downLeft && !downRight;
			bool left = Input.GetKey("a") && !upLeft && !downLeft;
			bool right = Input.GetKey("d") && !upRight && !downRight;
			bool escPress = Input.GetKeyDown(KeyCode.Escape);
			int pX = 0, pY = 0;
			int cX = 0, cY = 0;

			//DIAGONAL DIRECTIONS
			if (upLeft)
			{
				cX = cX - 1;
				cY = cY + 1;
				pX = actorTile.x - 1;
				pY = actorTile.y + 1;
			}
			if (upRight)
			{
				cX = cX + 1;
				cY = cY + 1;
				pY = actorTile.y + 1;
				pX = actorTile.x + 1;
			}
			if (downLeft)
			{
				cX = cX - 1;
				cY = cY - 1;
				pY = actorTile.y - 1;
				pX = actorTile.x - 1;
			}
			if (downRight)
			{
				cX = cX + 1;
				cY = cY - 1;
				pY = actorTile.y - 1;
				pX = actorTile.x + 1;
			}
			//END DIAGONAL
			//STRAIGHT DIRECTIONS
			if (up)
			{
				cY = cY + 1;
				pY = actorTile.y + 1;
				pX = actorTile.x;
			}
			if (left)
			{
				cX = cX - 1;
				pX = actorTile.x - 1;
				pY = actorTile.y;
			}
			if (down)
			{
				cY = cY - 1;
				pX = actorTile.x;
				pY = actorTile.y - 1;
			}
			if (right)
			{
				cX = cX + 1;
				pX = actorTile.x + 1;
				pY = actorTile.y;
			}
			//END DIRECTIONS------------------------------------------------------------------------------------------------------------------
			bool isMoving = (pX != 0) && (pY != 0);

			if (CanNotGoTo(actorTile.main_type)) // WATER BOOTS!
			{
				MapBox.instance.CallMethod(
						"removeWater", new object[] { playerActor.currentTile }
						);
				if ((pX != 0) && (pY != 0))
				{
					MapBox.instance.CallMethod(
							"removeWater", new object[] { MapBox.instance.GetTile(pX, pY) }
							);
				}
			}

			if (isMoving)
			{
				MovetheActor(WorldBoxMod.playerActor, MapBox.instance.GetTile(pX, pY));
			}
			else
            {
				AiController(playerActor);
            }

			PlayerActionController(playerActor, cX, cY);
			ActorManager(playerActor);

			if (escPress)
			{
				WorldTip.showNow("Released Control Of " + playerActor.coloredName, true, "top", 3f);
				//if (Config.controllableUnit != null)
                //{
					//playerActor = null;
					//Config.setControllableCreature(null);
                //}
				WorldBoxMod.isControlling = false;
				WorldBoxMod.runOnce = false;
				if(!WorldBoxMod.hadPeaceful)
                {
					playerActor.removeTrait("peaceful");
                }
				Config.setControllableCreature(null);
			}
		}	

		public void PlayerActionController(Actor playerActor, int cX, int cY)
        {
			bool uPress = Input.GetKeyDown("u");
			bool fPress = Input.GetKeyDown("f");
			bool shiftPress = Input.GetKey(KeyCode.LeftShift);
			bool shiftUp = Input.GetKeyUp(KeyCode.LeftShift);
			bool spacePress = Input.GetKeyDown(KeyCode.Space);
			WorldTile mouseTile = MapBox.instance.getMouseTilePos();
			//----------------------
			if (fPress)
            {
				Actor attackActor = MapBox.instance.getActorNearCursor();
				Building attackBuilding = mouseTile.building;
				if (attackActor != null && attackActor != playerActor)
                {
					bool attackPerform = (bool)playerActor.CallMethod(
						"tryToAttack", new object[] { attackActor }
						);
					if (attackPerform)
                    {

                    }
                }
				if (attackBuilding != null	)
                {
					bool attackPerform = (bool)playerActor.CallMethod(
						"tryToAttack", new object[] { attackBuilding }
						);
                }
            }
			if (uPress)
            {
				//MapBox.spawnLightning(mouseTile, 0.10f);
				//MapBox.instance.console.Show();
				WorldBoxMod.ShowHideMaster = !WorldBoxMod.ShowHideMaster;
				//insert ability
            }
			if (shiftPress)
            {
				playerActor.addTrait("sprinting");
				//playerActor.addForce(cX/1.5f, cY/1.5f, 0f);
				//improve speed
            }
			if(shiftUp)
            {
				playerActor.removeTrait("sprinting");
            }
			if (spacePress)
			{
				playerActor.addForce(cX, cY, 0.80f);
				playerActor.cancelAllBeh(null);
				//jump mod?
			}
        }	 

		public void ActorManager(Actor playerActor)
        {
			ActorStatus playerStatus = Reflection.GetField(playerActor.GetType(), playerActor, "data") as ActorStatus;
			
			//ActorStats playerStat = Reflection.GetField(playerActor.GetType(), playerActor, "stats") as ActorStats;
			if(!playerStatus.alive)
            {
				//Config.setControllableCreature(null);
				if(!WorldBoxMod.hadPeaceful)
                {
					playerActor.removeTrait("peaceful");
                }
				WorldBoxMod.isControlling = false;
				WorldTip.showNow("RIP to " + playerActor.coloredName, true, "top", 3f);
			}
			else
            {
            }
			if(!WorldBoxMod.runOnce)
            {
				if(playerActor.haveTrait("peaceful"))
                {
					WorldBoxMod.hadPeaceful = true;
                }
				else
                {
					playerActor.addTrait("peaceful");
                }
				WorldBoxMod.runOnce = true;
            }
        }
			
		public void AiController(Actor controlledActor)
        {
			AiSystemActor reAi = Reflection.GetField(controlledActor.GetType(), controlledActor, "ai") as AiSystemActor;
			bool semiPress = Input.GetKeyDown(KeyCode.Semicolon);
			if (semiPress)
            {
				WorldBoxMod.aiOn = !WorldBoxMod.aiOn;
				if (WorldBoxMod.aiOn)
                {
					WorldTip.showNow("AI ON", true, "top", 2f);

				}
                else
                {
					WorldTip.showNow("AI OFF", true, "top", 2f);

				}
            }
			if(WorldBoxMod.aiOn)
            {
				if (controlledActor.haveTrait("peaceful"))
                {
					controlledActor.removeTrait("peaceful");
                }

            }
            else
            {
				if (!controlledActor.haveTrait("peaceful"))
				{
					controlledActor.addTrait("peaceful");
				}
				reAi.CallMethod(
					"clearBeh", new object[] { }
					);
				reAi.CallMethod(
					"setTaskBehFinished", new object[] { }
					);
				reAi.setJob(null);
				reAi.clearAction();
				controlledActor.stopMovement();
			}
		}

		public bool CanNotGoTo(TileType currentType)
        {
			List<string> blockedTiles = new List<string> { "deep_ocean", "close_ocean", "shallow_waters", "mountains" };
			for (int i = 0; i < blockedTiles.Count; ++i)
            {
				if(currentType.IsType(blockedTiles[i]) )
                {
					return true;
                }
            }
			return false;
        }

		public void MovetheActor(Actor controlledActor, WorldTile targetTile)
        {
			AiSystemActor reAi = Reflection.GetField(controlledActor.GetType(), controlledActor, "ai") as AiSystemActor;
			bool isImpassable = CanNotGoTo(targetTile.main_type);
			if (isImpassable)
			{	
				controlledActor.goTo(targetTile, true, true);
			}
			else
			{
				controlledActor.goTo(targetTile);
			}
		}

		public static void IncarnateGUI()
        {
			Rect buttonSize = new Rect(Screen.width - 100, 60, 5f, 5f);//100f, 20f);
			WorldTile mousePos = MapBox.instance.getMouseTilePos();

			if (WorldBoxMod.ShowHideMaster)
			{
				if (GUI.Button(buttonSize, "IncarnateUI"))
				{
					WorldBoxMod.ShowHide = !WorldBoxMod.ShowHide;
				}

				if (WorldBoxMod.ShowHide)
                {

				}// SHOWHIDE END
				
			}// SHOWHIDE MASTER END

			if(WorldBoxMod.isControlling)
            {
				BaseStats playerStats = Reflection.GetField(playerActor.GetType(), playerActor, "curStats") as BaseStats;
				GUI.backgroundColor = Color.white;
				GUI.contentColor = Color.black;
				string presentedHealth = WorldBoxMod.playerActor.base_data.health + "/" + playerStats.health;
				GUI.Box(new Rect(Screen.width/2, 0, 100f, 20f), presentedHealth);
            }
        }

		public void Clean()
        {
			isOn = false;
			isControlling = false;
			playerActor = null;
        }

	} // End WorldBoxMod-----------------------------------------------
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