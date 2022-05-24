using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v1;
using MCM.Abstractions.Settings.Base.PerSave;
using MCM.Abstractions.Settings.Base;
using deMountandSprint.Behaviors;
using deMountandSprint.Stamina;
using HarmonyLib;

namespace deMountandSprint
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("Bannerlord.deMountandSprint").PatchAll();
            InformationManager.DisplayMessage(new InformationMessage("Sprint loaded"));
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            if (!Settings.Instance.deMountandSprint) return;
            if (Settings.Instance.HasStamina) mission.AddMissionBehavior(new SprintLogicRBM(mission));
            else mission.AddMissionBehavior(new SprintLogic(mission));
        }
    }

    public class Settings : AttributePerSaveSettings<Settings>
    {
        public override string Id => "deMountandSprintSettings";
        
        public override string DisplayName => "deMountandSprint";

        //GROUP 1
        [SettingProperty("Activate Mod", RequireRestart = false, HintText = "Turn the mod on/off")]
        [SettingPropertyGroup("Mod Config", GroupOrder = 1)]    
        public bool deMountandSprint { get; set; } = true;

        [SettingProperty("Sprint Speed Multiplier On", RequireRestart = false, HintText = " Determines whether sprint speed is multiplier\nON: Sprint speed will be 6.2 * (THIS_VALUE)\nOFF: Sprint speed will be flat speed value(Vanilla max player speed: 6.2).")]
        [SettingPropertyGroup("Mod Config", GroupOrder = 1)]
        public bool HasSpeedModMultiplier { get; set; } = true;

        [SettingProperty("Base Speed Multiplier On", RequireRestart = false, HintText = " Determines whether base speed is multiplier.\nON: Base speed will be 6.2 * (THIS_VALUE)\nOFF: Base speed will be flat speed value(Vanilla max player speed: 6.2).")]
        [SettingPropertyGroup("Mod Config", GroupOrder = 1)]
        public bool HasPlayerMultiplier { get; set; } = false;

        [SettingProperty("Toggle Sprint", RequireRestart = false, HintText = "\nON: Press to sprint\nOFF: Hold to sprint")]
        [SettingPropertyGroup("Mod Config", GroupOrder = 1)]
        public bool HasToggle { get; set; } = false;

        [SettingProperty("Sprint Key", minValue: 0, maxValue: 256, RequireRestart = false, HintText = " Key to Sprint\nCommon KeyCodes:\nLAlt:56       |       LShift:42")]
        [SettingPropertyGroup("Keyboard", GroupOrder = 5)]
        public int SprintKey { get; set; } = 47;
        //END GROUP 1:

        //GROUP 2:
        [SettingProperty("Base Speed", minValue: 0f, maxValue: 100f, RequireRestart = false, HintText = "\nBase Speed Value/Multiplier of player when not holding V.\n")]
        [SettingPropertyGroup("Speed Values", GroupOrder = 2)]
        public float PlayerSpeed { get; set; } = 5f;

        [SettingProperty("Sprint Speed", minValue: 0f, maxValue: 100f, RequireRestart = false, HintText = "\nSpeed Value/Multiplier of player when holding V.")]
        [SettingPropertyGroup("Speed Values", GroupOrder = 2)]
        public float SpeedMod { get; set; } = 1.5f;
        //END GROUP 2

        //GROUP 3
        [SettingProperty("Athletics Activate", RequireRestart = false, HintText = "\nBase speed will be based off of player athletics, like vanilla.\nON: Use Athletics Bonus: OVERRIDES \"Sprint Speed\" and \"Base Speed\" Multipliers.")]
        [SettingPropertyGroup("Athletics & Stamina", GroupOrder = 3)]
        public bool AthleticSprint { get; set; } = false;

        [SettingProperty("Athletics Sprint Bonus", minValue: 0f, maxValue: 100f, RequireRestart = false, HintText = "\nSprint Bonus when Athletics is on, will be added to your regular speed multiplier(Vanilla Bonus is kept under 1).")]
        [SettingPropertyGroup("Athletics & Stamina", GroupOrder = 3)]
        public float AthleticBonus { get; set; } = 1.5f;

        [SettingProperty("Stamina/Posture Activate", RequireRestart = false, HintText = "\nSprinting requires stamina/posture\nWARNING: Realistic Battle AI Module is required for this.")]
        [SettingPropertyGroup("Athletics & Stamina", GroupOrder = 3)]
        public bool HasStamina { get; set; } = false;

        [SettingProperty("Stamina/Posture Loss Multiplier", minValue: 0f, maxValue: 5f, RequireRestart = false, HintText = "\nThe amount of stamina lost when sprinting(Multiplied by your own posture regeneration rate).\nWARNING: Realistic Battle AI Module is required for this.")]
        [SettingPropertyGroup("Athletics & Stamina", GroupOrder = 3)]
        public float StaminaLoss { get; set; } = 1.3f;
        //END GROUP 3

        //GROUP 4
        [SettingProperty("Activate Teleport", RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Teleportation", GroupOrder = 4)]
        public bool CanTeleport { get; set; } = false;

        [SettingProperty("Distance to Close Teleport", minValue: 0f, maxValue: 20f, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Teleportation", GroupOrder = 4)]
        public float TeleportDistance { get; set; } = 1.5f;

        [SettingProperty("Nothing personal, kid.", RequireRestart = false, HintText = "\nTeleport behind nearest enemy.")]
        [SettingPropertyGroup("Teleportation", GroupOrder = 4)]
        public bool CanMemeTeleport { get; set; } = false;

        [SettingProperty("Teleport where you're looking.", RequireRestart = false, HintText = "\nTeleport behind nearest enemy.")]
        [SettingPropertyGroup("Teleportation", GroupOrder = 4)]
        public bool CanLookTeleport { get; set; } = false;

        [SettingProperty("Stamina cost for close teleport.", minValue: 0f, maxValue: 100f, RequireRestart = false, HintText = "\nStamina cost is % of total stamina.")]
        [SettingPropertyGroup("Teleportation", GroupOrder = 4)]
        public float CloseTeleportStaminaCost { get; set; } = 15f;

        [SettingProperty("Stamina cost for look teleport.", minValue: 0f, maxValue: 100f, RequireRestart = false, HintText = "\nStamina cost for teleport is multiplied by distance.")]
        [SettingPropertyGroup("Teleportation", GroupOrder = 4)]
        public float LookTeleportStaminaCost { get; set; } = 5f;

        [SettingProperty("Stamina Enabled", RequireRestart = false, HintText = "\nTeleport costs stamina.\nSprint Stamina On Required.")]
        [SettingPropertyGroup("Teleportation", GroupOrder = 4)]
        public bool StaminaTeleportEnabled { get; set; } = false;

        [SettingProperty("Teleport Key", minValue: 0, maxValue: 256, RequireRestart = false, HintText = "\nKey to Teleport")]
        [SettingPropertyGroup("Keyboard", GroupOrder = 5)]
        public int TeleportKey { get; set; } = 35;

        [SettingProperty("Nothing personal, kid Key", minValue: 0, maxValue: 256, RequireRestart = false, HintText = "\nKey to teleport behind nearest enemy.")]
        [SettingPropertyGroup("Keyboard", GroupOrder = 5)]
        public int MemeKey { get; set; } = 29;

        public override IDictionary<string, Func<BaseSettings>> GetAvailablePresets()
        {
            var basePresets = base.GetAvailablePresets();
            basePresets.Add("The Flash", () => new Settings()
            {
                deMountandSprint = true,
                HasSpeedModMultiplier = true,
                HasPlayerMultiplier = true,
                HasToggle = false,
                AthleticSprint = false,
                HasStamina = false,
                AthleticBonus = 0f,
                PlayerSpeed = 0.07f,
                SpeedMod = 10f,
            });

            basePresets.Add("Vanilla Sprint", () => new Settings()
            {
                deMountandSprint = true,
                HasSpeedModMultiplier = true,
                HasPlayerMultiplier = false,
                HasToggle = false,
                AthleticSprint = true,
                HasStamina = false,
                StaminaLoss = 0f,
                AthleticBonus = 0.3f,
                PlayerSpeed = 6.2f,
                SpeedMod = 1.3f
            });

            basePresets.Add("Vanilla Sprint(With Stamina)", () => new Settings()
            {
                deMountandSprint = true,
                HasSpeedModMultiplier = true,
                HasPlayerMultiplier = false,
                HasToggle = false,
                AthleticSprint = true,
                HasStamina = true,
                StaminaLoss = 1.2f,
                AthleticBonus = 0.3f,
                PlayerSpeed = 6.2f,
                SpeedMod = 1.3f
            });

            return basePresets;
        }
    }
}