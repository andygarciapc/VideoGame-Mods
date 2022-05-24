using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using ai;
using ai.behaviours;

namespace Incarnate.UnitControl
{
    public class UnitController
    {
		public Actor playerActor;
		public static bool aiOn = false;
		public static bool camOff = false;
		public static bool runOnce = false;
		public static bool hadPeaceful = false;
		public bool isControlling = false;

		public UnitController()
        {
			playerActor = null;
        }

		private void ControlButtonInitialize()
		{
			bool hPress = Input.GetKeyDown("h");
			bool cPress = Input.GetKeyDown("c");
			bool uPress = Input.GetKeyDown("u");
			GodPower spawnHuman = AssetManager.powers.get("humans");
			WorldTile mousePos = MapBox.instance.getMouseTilePos();

			if (!isControlling)//CHECK C/H KEY TO CONTROL
			{
				if (hPress)
				{
					playerActor = MapBox.instance.createNewUnit(spawnHuman.actorStatsId, mousePos, "", spawnHuman.actorSpawnHeight, null);
					if (playerActor != null)
					{
						//Config.setControllableCreature(WorldBoxMod.playerActor);
						isControlling = true;
						playerActor.addTrait("incarnation");
						WorldTip.showNow("Incarnated as " + playerActor.coloredName, true, "top", 3f);
						ActorManager(playerActor);
					}

				}
				if (cPress)
				{

					playerActor = MapBox.instance.getActorNearCursor();

					if (playerActor != null)
					{
						//Config.setControllableCreature(WorldBoxMod.playerActor);
						isControlling = true;
						WorldTip.showNow("Now controlling " + playerActor.coloredName, true, "top", 3f);
						ActorManager(playerActor);
					}
				}
			}
		}

		public void CameraController()
		{
			bool lPress = Input.GetKeyDown("l");
			if (lPress)
			{
				camOff = !camOff;
			}
			if (camOff)
			{
				if (Config.controllableUnit != null)
				{
					Config.setControllableCreature(null);
				}
			}
			else
			{
				if (isControlling)
				{
					if (Config.controllableUnit == null)
					{
						Config.setControllableCreature(playerActor);
					}
					Camera.main.transform.position = new Vector2(playerActor.transform.position.x, playerActor.transform.position.y);
				}
			}
		}

		public void ActorController()
		{
			WorldTile actorTile = playerActor.currentTile;
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
				isControlling = false;
				runOnce = false;
				if (!hadPeaceful)
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
				if (attackBuilding != null)
				{
					bool attackPerform = (bool)playerActor.CallMethod(
						"tryToAttack", new object[] { attackBuilding }
						);
				}
			}
			if (shiftPress)
			{
				playerActor.addTrait("sprinting");
				//playerActor.addForce(cX/1.5f, cY/1.5f, 0f);
				//improve speed
			}
			if (shiftUp)
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

		public static void ActorManager(Actor playerActor)
		{
			ActorStatus playerStatus = Reflection.GetField(playerActor.GetType(), playerActor, "data") as ActorStatus;

			//ActorStats playerStat = Reflection.GetField(playerActor.GetType(), playerActor, "stats") as ActorStats;
			if (!playerStatus.alive)
			{
				//Config.setControllableCreature(null);
				if (!hadPeaceful)
				{
					playerActor.removeTrait("peaceful");
				}
				isControlling = false;
				WorldTip.showNow("RIP to " + playerActor.coloredName, true, "top", 3f);
			}
			else
			{
			}
			if (!runOnce)
			{
				if (playerActor.haveTrait("peaceful"))
				{
					hadPeaceful = true;
				}
				else
				{
					playerActor.addTrait("peaceful");
				}
				runOnce = true;
			}
		}

		public void AiController(Actor controlledActor)
		{
			AiSystemActor reAi = Reflection.GetField(controlledActor.GetType(), controlledActor, "ai") as AiSystemActor;
			bool semiPress = Input.GetKeyDown(KeyCode.Semicolon);
			if (semiPress)
			{
				aiOn = !aiOn;
				if (aiOn)
				{
					WorldTip.showNow("AI ON", true, "top", 2f);

				}
				else
				{
					WorldTip.showNow("AI OFF", true, "top", 2f);

				}
			}
			if (aiOn)
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
				if (currentType.IsType(blockedTiles[i]))
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
			if (isImpassable) controlledActor.goTo(targetTile, true, true);
			else controlledActor.goTo(targetTile);
		}

		public void Update()
        {

        }
	}
}
