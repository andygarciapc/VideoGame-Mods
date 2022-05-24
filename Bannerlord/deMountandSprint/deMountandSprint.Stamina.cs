using System;
using System.Reflection;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using MCM.Abstractions.Settings.Base.PerSave;
using RealisticBattleAiModule;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.View.Screen;
using RealisticBattleAiModule.AiModule.Posture;

namespace deMountandSprint.Stamina
{
    class SprintLogicRBM : MissionView
    {
        Mission mission;
        static Agent player;
        bool isTired;
        GameEntity tpFlag;
        static bool isSprinting = false;
        Posture posture = null;
        Vec3 lookTeleport = Vec3.Zero;
        float staminaLoss = Settings.Instance.StaminaLoss;
        bool hasSpeedModMultiplier = Settings.Instance.HasSpeedModMultiplier;
        bool hasPlayerMultiplier = Settings.Instance.HasPlayerMultiplier;
        bool canTeleport = Settings.Instance.CanTeleport;
        bool memeTeleport = Settings.Instance.CanMemeTeleport;
        float teleportDistance = Settings.Instance.TeleportDistance;
        bool staminaTeleportEnabled = Settings.Instance.StaminaTeleportEnabled;
        bool canLookTp = Settings.Instance.CanLookTeleport;
        float closeTpStamina = Settings.Instance.CloseTeleportStaminaCost;
        float lookTpStamina = Settings.Instance.LookTeleportStaminaCost;

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
        public SprintLogicRBM(Mission _mission)
        {
            mission = _mission;
            tpFlag = GameEntity.CreateEmpty(mission.Scene, true);
            tpFlag.EntityFlags |= EntityFlags.NotAffectedBySeason;
            tpFlag.AddComponent(MetaMesh.GetCopy("human_shadow_mesh", true, false));
            tpFlag.SetContourColor(new uint?(4294967295U), true);
            tpFlag.SetColor(4284900966U, 4284900966U, "human_shadow_mesh");
            tpFlag.SetFactorColor(4284900966U);
            MatrixFrame frame = tpFlag.GetFrame();
            float num = MathF.Sin(MBCommon.GetApplicationTime() * 2f) + 1f;
            num *= 0.25f;
            frame.origin.z = num;
            tpFlag.SetFrame(ref frame);
        }

        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            if(agent.IsMainAgent)
            {
                player = agent;
                Posture temp = null;
                AgentPostures.values.TryGetValue(agent, out temp);
                if (temp != null)
                {
                    posture = temp;
                }
            }
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);
            if (agent.IsMainAgent)
            {
                player = agent;
                Posture temp = null;
                AgentPostures.values.TryGetValue(agent, out temp);
                if (temp != null)
                {
                    posture = temp;
                }
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            bool isValid = (mission.MainAgent != null);
            if (!isValid) return;
            if (player != mission.MainAgent) player = mission.MainAgent;
            if (player.HasMount) return;

            InputKey key = (InputKey)Settings.Instance.SprintKey;

            if (Settings.Instance.HasToggle)
            {
                if (Input.IsKeyPressed(key))
                {
                    SprintLogicRBM.isSprinting = !SprintLogicRBM.isSprinting;
                }
            }
            else
            {
                SprintLogicRBM.isSprinting = Input.IsKeyDown(key);
            }

            //GET INPUT
            bool wKey = Input.IsKeyDown(InputKey.W);
            bool aKey = Input.IsKeyDown(InputKey.A);
            bool dKey = Input.IsKeyDown(InputKey.D);
            bool sKey = Input.IsKeyDown(InputKey.S);
            bool isMoving = wKey || aKey || dKey || sKey;

            bool postureValid = posture != null;
            if(postureValid)
            {
                Posture temp = null;
                AgentPostures.values.TryGetValue(mission.MainAgent, out temp);
                if(temp != null)
                {
                    if(temp == posture) { }
                    else posture = temp;
                }
            } 
            else
            {
                AgentPostures.values.TryGetValue(mission.MainAgent, out posture);
                return;
            }
            
            if(posture.posture < 1f) isTired = true;                                        //Posture less than 1, player is now tired.
            else if(posture.posture > posture.maxPosture/3f) isTired = false;               //No longer tired when posture 1/3 of max.
            if(isTired) isSprinting = false;

            float speedMod = Settings.Instance.SpeedMod;
            float playerSpeed = Settings.Instance.PlayerSpeed;

            if (isSprinting)
            {
                if (wKey)
                {
                    player.SetMaximumSpeedLimit(speedMod, hasSpeedModMultiplier);
                }
                if (aKey)
                {
                    player.SetMaximumSpeedLimit(speedMod * 0.5f, hasSpeedModMultiplier);
                }
                if (dKey)
                {
                    player.SetMaximumSpeedLimit(speedMod * 0.5f, hasSpeedModMultiplier);
                }
                if (sKey)
                {
                    player.SetMaximumSpeedLimit(speedMod * 0.4f, hasSpeedModMultiplier);
                }
                else
                {
                    player.SetMaximumSpeedLimit(speedMod * 0.6f, hasSpeedModMultiplier);
                }
                if(isMoving)
                {
                    posture.posture -= posture.regenPerTick * staminaLoss;
                    AgentPostures.postureVisual._dataSource.PlayerPosture = (int)posture.posture;
                    AgentPostures.postureVisual._dataSource.PlayerPostureMax = (int)posture.maxPosture;
                }
            }
            else
            {
                if (wKey)
                {
                    if (isTired) player.SetMaximumSpeedLimit(playerSpeed*0.5f, hasPlayerMultiplier);
                    else player.SetMaximumSpeedLimit(playerSpeed, hasPlayerMultiplier);
                }
                if (aKey)
                {
                    player.SetMaximumSpeedLimit(playerSpeed * 0.5f, hasPlayerMultiplier);
                }
                if (dKey)
                {
                    player.SetMaximumSpeedLimit(playerSpeed * 0.5f, hasPlayerMultiplier);
                }
                if (sKey)
                {
                    player.SetMaximumSpeedLimit(playerSpeed * 0.4f, hasPlayerMultiplier);
                }
                else
                {
                    player.SetMaximumSpeedLimit(playerSpeed * 0.6f, hasPlayerMultiplier);
                }
            }

            if (canTeleport) TeleportManager(wKey, aKey, dKey, sKey);
        }

        private void TeleportManager(bool wKey, bool aKey, bool dKey, bool sKey)
        {
            InputKey teleportKey = (InputKey)Settings.Instance.TeleportKey;
            InputKey memeKey = (InputKey)Settings.Instance.MemeKey;
            InputKey sprintKey = (InputKey)Settings.Instance.SprintKey;
            Vec3 telePos = player.Position;

            bool willdodgeTeleport = (Input.IsKeyDown(sprintKey) && Input.IsKeyPressed(teleportKey));
            bool willMemeTeleport = Input.IsKeyPressed(memeKey);
            if (willdodgeTeleport || willMemeTeleport)
            {
                float currTpCost = posture.maxPosture * (closeTpStamina / 100);
                if (currTpCost > posture.posture) return;

                Vec3 playerLook = player.LookDirection;
                if (wKey)
                {
                    telePos.x += playerLook.x * teleportDistance;
                    telePos.y += playerLook.y * teleportDistance;
                }
                if (aKey)
                {
                    telePos.x += (-playerLook.y) * teleportDistance;
                    telePos.y += playerLook.x * teleportDistance;
                }
                if (dKey)
                {
                    telePos.x += playerLook.y * teleportDistance;
                    telePos.y += (-playerLook.x) * teleportDistance;
                }
                if (sKey)
                {
                    telePos.x -= playerLook.x * teleportDistance;
                    telePos.y -= playerLook.y * teleportDistance;
                }

                if (memeTeleport)
                {
                    if (willMemeTeleport)
                    {
                        Agent nothingPersonalKid = null;
                        nothingPersonalKid = mission.GetClosestEnemyAgent(player.Team, player.Position, 1.0e20f);
                        if (nothingPersonalKid == null)
                        {
                            nothingPersonalKid = player.ImmediateEnemy;
                            if (nothingPersonalKid == null) return;
                        }

                        Vec3 enemyLook = nothingPersonalKid.LookDirection;
                        telePos = nothingPersonalKid.Position;
                        telePos.x -= enemyLook.x * 1.3f;
                        telePos.y -= enemyLook.y * 1.3f;
                        player.LookDirection = enemyLook;
                        FieldInfo cameraSpecialCurrentAddedElevation = Utility.CameraSpecialCurrentAddedElevation;
                        if (cameraSpecialCurrentAddedElevation != null)
                        {
                            cameraSpecialCurrentAddedElevation.SetValue(MissionScreen, MissionScreen.CameraElevation);
                        }
                        FieldInfo cameraSpecialCurrentAddedBearing = Utility.CameraSpecialCurrentAddedBearing;
                        if (cameraSpecialCurrentAddedBearing != null)
                        {
                            cameraSpecialCurrentAddedBearing.SetValue(MissionScreen, MBMath.WrapAngle(MissionScreen.CameraBearing - nothingPersonalKid.LookDirectionAsAngle));
                        }
                        MethodInfo setCameraElevation = Utility.SetCameraElevation;
                        if (setCameraElevation != null)
                        {
                            setCameraElevation.Invoke(MissionScreen, new object[]
                            {
                                        0f
                            });
                        }
                        MethodInfo setCameraBearing = Utility.SetCameraBearing;
                        if (setCameraBearing != null)
                        {
                            setCameraBearing.Invoke(MissionScreen, new object[]
                            {
                                        nothingPersonalKid.LookDirectionAsAngle
                            });
                        }

                    }
                }
                else if (willMemeTeleport) return;

                if (staminaTeleportEnabled)
                {
                    currTpCost = posture.maxPosture * (closeTpStamina / 100);
                    if (currTpCost < posture.posture)
                    {
                        posture.posture -= posture.maxPosture * (closeTpStamina / 100);
                        AgentPostures.postureVisual._dataSource.PlayerPosture = (int)posture.posture;
                        AgentPostures.postureVisual._dataSource.PlayerPostureMax = (int)posture.maxPosture;
                        player.TeleportToPosition(telePos);
                    }
                }
                else
                {
                    player.TeleportToPosition(telePos);
                }
            }

            if (!canLookTp) return;

            if (Input.IsKeyDown(teleportKey) && !Input.IsKeyDown(sprintKey))
            {
                tpFlag.SetVisibilityExcludeParents(true);
                Vec3 position;
                Vec3 vec;
                bool isValidGround = false;
                if (this.MissionScreen.GetProjectedMousePositionOnGround(out position, out vec, true))
                {
                    WorldPosition worldPosition = new WorldPosition(mission.Scene, UIntPtr.Zero, position, false);
                    isValidGround = (OrderFlag.IsPositionOnValidGround(worldPosition));
                }
                if (isValidGround)
                {
                    lookTeleport = position;
                }
                MatrixFrame frame = tpFlag.GetFrame();
                frame.origin = lookTeleport;
                frame.rotation = player.LookRotation;
                tpFlag.SetFrame(ref frame);
            }
            if (Input.IsKeyReleased(teleportKey) && lookTeleport != Vec3.Zero)
            {
                if (staminaTeleportEnabled)
                {
                    double eqn = Math.Pow(lookTeleport.x - player.Position.x, 2) + Math.Pow(lookTeleport.y - player.Position.y, 2) + Math.Pow(lookTeleport.z - player.Position.z, 2);
                    float distance = MathF.Sqrt((float)eqn);
                    float lookTpCost = distance * lookTpStamina;
                    if (lookTpCost < posture.posture)
                    {
                        posture.posture -= distance * lookTpStamina;
                        AgentPostures.postureVisual._dataSource.PlayerPosture = (int)posture.posture;
                        AgentPostures.postureVisual._dataSource.PlayerPostureMax = (int)posture.maxPosture;
                        player.TeleportToPosition(lookTeleport);
                    }
                }
                else
                {
                    player.TeleportToPosition(lookTeleport);
                }
                tpFlag.SetVisibilityExcludeParents(false);
                lookTeleport = Vec3.Zero;
            }
        }

        public static class Utility
        {
            static Utility()
            {
                PropertyInfo property = typeof(MissionScreen).GetProperty("LastFollowedAgent", BindingFlags.Instance | BindingFlags.Public);
                Utility.SetLastFollowedAgent = ((property != null) ? property.GetSetMethod(true) : null);
                Utility.CameraSpecialCurrentAddedElevation = typeof(MissionScreen).GetField("_cameraSpecialCurrentAddedElevation", BindingFlags.Instance | BindingFlags.NonPublic);
                Utility.CameraSpecialCurrentAddedBearing = typeof(MissionScreen).GetField("_cameraSpecialCurrentAddedBearing", BindingFlags.Instance | BindingFlags.NonPublic);
                Utility.CameraSpecialCurrentPositionToAdd = typeof(MissionScreen).GetField("_cameraSpecialCurrentPositionToAdd", BindingFlags.Instance | BindingFlags.NonPublic);
                PropertyInfo property2 = typeof(MissionScreen).GetProperty("CameraElevation", BindingFlags.Instance | BindingFlags.Public);
                Utility.SetCameraElevation = ((property2 != null) ? property2.GetSetMethod(true) : null);
                PropertyInfo property3 = typeof(MissionScreen).GetProperty("CameraBearing", BindingFlags.Instance | BindingFlags.Public);
                Utility.SetCameraBearing = ((property3 != null) ? property3.GetSetMethod(true) : null);
                Utility.IsPlayerAgentAdded = typeof(MissionScreen).GetField("_isPlayerAgentAdded", BindingFlags.Instance | BindingFlags.NonPublic);
                Utility.ShouldSmoothMoveToAgent = true;
                Utility.HasPlayer = typeof(Formation).GetProperty("HasPlayer", BindingFlags.Instance | BindingFlags.NonPublic);
                PropertyInfo hasPlayer = Utility.HasPlayer;
                Utility.SetHasPlayerMethod = ((hasPlayer != null) ? hasPlayer.GetSetMethod(true) : null);
            }

            public static readonly MethodInfo SetLastFollowedAgent;
            public static readonly FieldInfo CameraSpecialCurrentAddedElevation;
            public static readonly FieldInfo CameraSpecialCurrentAddedBearing;
            public static readonly FieldInfo CameraSpecialCurrentPositionToAdd;
            public static readonly MethodInfo SetCameraElevation;
            public static readonly MethodInfo SetCameraBearing;
            public static readonly FieldInfo IsPlayerAgentAdded;
            public static bool ShouldSmoothMoveToAgent;
            private static readonly PropertyInfo HasPlayer;
            public static readonly MethodInfo SetHasPlayerMethod;

        }
    }


}
