using System;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screen;
using TaleWorlds.MountAndBlade.View.Missions;
using MCM.Abstractions.Settings.Base.PerSave;

namespace deMountandSprint.Behaviors
{
    public class SprintLogic : MissionView
    {
        Mission mission;
        static bool isSprinting = false;
        GameEntity tpFlag;
        Vec3 lookTeleport = Vec3.Zero;
        static Agent player;
        bool canTp = Settings.Instance.CanTeleport;
        bool hasSpeedModMultiplier = Settings.Instance.HasSpeedModMultiplier;
        bool hasPlayerMultiplier = Settings.Instance.HasPlayerMultiplier;
        bool canMemeTp = Settings.Instance.CanMemeTeleport;
        float tpDistance = Settings.Instance.TeleportDistance;
        bool canLookTp = Settings.Instance.CanLookTeleport;
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public SprintLogic(Mission _mission)
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

        public override void OnMissionTick(float dt)
        {
           base.OnMissionTick(dt);

            bool isValid = (mission.MainAgent != null);
            if (!isValid)
            {
                return;
            }
            if (player != Mission.MainAgent)
            {
                player = Mission.MainAgent;
            }
            if (player.HasMount) return;
            InputKey key = (InputKey)Settings.Instance.SprintKey;
            if (Settings.Instance.HasToggle)
            {
                if(Input.IsKeyPressed(key))
                {
                    SprintLogic.isSprinting = !SprintLogic.isSprinting;
                }
            }
            else
            {
                SprintLogic.isSprinting = Input.IsKeyDown(key);
            }
            bool wKey = Input.IsKeyDown(InputKey.W);
            bool aKey = Input.IsKeyDown(InputKey.A);
            bool dKey = Input.IsKeyDown(InputKey.D);
            bool sKey = Input.IsKeyDown(InputKey.S);
            bool adKey = aKey || dKey;

            float speedMod = Settings.Instance.SpeedMod;
            float playerSpeed = Settings.Instance.PlayerSpeed;

            if (isSprinting)
            {
                if (wKey) player.SetMaximumSpeedLimit(speedMod, hasSpeedModMultiplier);
                if(adKey) player.SetMaximumSpeedLimit(speedMod * 0.55f, hasSpeedModMultiplier);
                if (sKey) player.SetMaximumSpeedLimit(speedMod * 0.43f, hasSpeedModMultiplier);
                else player.SetMaximumSpeedLimit(speedMod * 0.55f, hasSpeedModMultiplier);
            }
            else
            {
                if (wKey) player.SetMaximumSpeedLimit(playerSpeed, hasPlayerMultiplier);
                if (adKey) player.SetMaximumSpeedLimit(playerSpeed * 0.55f, hasPlayerMultiplier);
                if (sKey) player.SetMaximumSpeedLimit(playerSpeed * 0.43f, hasPlayerMultiplier);
                else player.SetMaximumSpeedLimit(playerSpeed * 0.55f, hasPlayerMultiplier);
            }

            if(canTp) TeleportManager(wKey, aKey, dKey, sKey);
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
                Vec3 playerLook = player.LookDirection;
                if (wKey)
                {
                    telePos.x += playerLook.x * tpDistance;
                    telePos.y += playerLook.y * tpDistance;
                }
                if (aKey)
                {
                    telePos.x += (-playerLook.y) * tpDistance;
                    telePos.y += playerLook.x * tpDistance;
                }
                if (dKey)
                {
                    telePos.x += playerLook.y * tpDistance;
                    telePos.y += (-playerLook.x) * tpDistance;
                }
                if (sKey)
                {
                    telePos.x -= playerLook.x * tpDistance;
                    telePos.y -= playerLook.y * tpDistance;
                }

                if (canMemeTp)
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
                player.TeleportToPosition(telePos);
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
                player.TeleportToPosition(lookTeleport);
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
    /*public class StaminaUI : MissionView
    {
        GauntletLayer layer;
        IGauntletMovie movie;
        StaminaVM staminaVM;
        static Settings instance = PerSaveSettings<Settings>.Instance;
        Mission mission;
        bool useStamina;
        static Agent player;

        public StaminaUI(Mission _mission, bool UseStamina)
        {
            mission = _mission;
            useStamina = UseStamina;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            staminaVM = new StaminaVM();
            layer = new GauntletLayer(1);
            movie = layer.LoadMovie("StaminaUI", staminaVM);
            MissionScreen.AddLayer(layer);
        }

        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            layer = null;
            movie = null;
            staminaVM = null;
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            bool isValid = (mission != null && mission.MainAgent != null);
            if (!isValid)
            {
                return;
            }
            if (player != mission.MainAgent)
            {
                player = mission.MainAgent;
            }
            if (player.HasMount)
            {
                return;
            }

            bool isSprinting = Input.IsKeyDown((InputKey)instance.sprintKey);
            if (isSprinting)
            {
                if(useStamina)
                {
                    bool hasStamina = staminaVM.Stamina > 0;
                    if(hasStamina)
                    {
                        player.SetMaximumSpeedLimit(instance.speedMod, true);
                        staminaVM.Stamina -= 0.5f;
                    }
                    else
                    {
                        player.SetMaximumSpeedLimit(instance.playerSpeed * 0.5f, true);
                    }
                }
                else player.SetMaximumSpeedLimit(instance.speedMod, true);
            }
            else
            {
                player.SetMaximumSpeedLimit(instance.playerSpeed, true);
            }
        }
    }

    public class StaminaVM : ViewModel
    {
        float stamina;
        float staminaMax;
        bool showStamina;
        string staminaLabel;

        public StaminaVM()
        {
            stamina = 100f;
            staminaMax = 100f;
            showStamina = true;
            staminaLabel = "StaminaLabel";
        }

        [DataSourceProperty]
        public float Stamina
        {
            get
            {
                return this.stamina;
            }
            set
            {
                if(value != this.stamina)
                {
                    this.stamina = value;
                    base.OnPropertyChangedWithValue(value, "Stamina");
                }
            }
        }

        [DataSourceProperty]
        public float StaminaMax
        {
            get
            {
                return this.staminaMax;
            }
            set
            {
                if (value != this.staminaMax)
                {
                    this.staminaMax = value;
                    base.OnPropertyChangedWithValue(value, "StaminaMax");
                }
            }
        }

        [DataSourceProperty]
        public bool ShowStaminaStatus
        {
            get
            {
                return this.showStamina;
            }
            set
            {
                if (value != this.showStamina)
                {
                    this.showStamina = value;
                    base.OnPropertyChanged("ShowStaminaStatus");
                }
            }
        }

        [DataSourceProperty]
        public string StaminaLabel
        {
            get
            {
                return this.staminaLabel;
            }
            set
            {
                if (value != this.staminaLabel)
                {
                    this.staminaLabel = value;
                    base.OnPropertyChanged("StaminaLabel");
                }
            }
        }
    }*/
}