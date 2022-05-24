using System;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using HarmonyLib;
using SandBox;
using MCM.Abstractions.Settings.Base.PerSave;
using deMountandSprint;
using deMountandSprint.Behaviors;

namespace deMountandSprint.Patches
{

    [HarmonyPatch(typeof(SandboxAgentStatCalculateModel), "UpdateAgentStats")]
    public class StatPatch
    {
        static void Postfix(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            if (!Settings.Instance.deMountandSprint) return;
            if (agent.IsAIControlled) return;
            UpdateAgentDrivenProperties(agent, agentDrivenProperties);
        }

        public static void UpdateAgentDrivenProperties(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            if (!Settings.Instance.deMountandSprint) return;
            if (agent.IsAIControlled) return;
            if (!agent.IsMainAgent) return;
            if(Settings.Instance.AthleticSprint)
            {
                Settings.Instance.SpeedMod = StatPatch.CalculateMaxSpeed(agent) + Settings.Instance.AthleticBonus;
                Settings.Instance.PlayerSpeed = (6.2f * StatPatch.CalculateMaxSpeed(agent));
                agentDrivenProperties.MaxSpeedMultiplier = Settings.Instance.SpeedMod;
            }
            else
            {
                agentDrivenProperties.MaxSpeedMultiplier = Settings.Instance.SpeedMod;
            }
          /*Traverse.Create(agent.Monster).Property<float>("JumpAcceleration").Value = 50f;
            Traverse.Create(agent.Monster).Property<float>("JumpSpeedLimit").Value = 50f;
            agentDrivenProperties.TopSpeedReachDuration = Settings.Instance.SpeedMod;*/
        }

        public static float CalculateMaxSpeed(Agent agent)
        {
            return SandboxAgentStatCalculateModel.CalculateMaximumSpeedMultiplier(agent.Character.GetSkillValue(DefaultSkills.Athletics), agent.Monster.Weight, agent.AgentDrivenProperties.ArmorEncumbrance + agent.AgentDrivenProperties.WeaponsEncumbrance);
        }
    }

    [HarmonyPatch(typeof(SandboxAgentStatCalculateModel), "InitializeAgentStats")]
    public class InitializeAgentPatch
    {

        static void Postfix(Agent agent, Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData)
        {
            if (!Settings.Instance.deMountandSprint) return;
            StatPatch.UpdateAgentDrivenProperties(agent, agentDrivenProperties);
            if (Settings.Instance.AthleticSprint && agent.IsMainAgent)
            {
                Settings.Instance.SpeedMod = StatPatch.CalculateMaxSpeed(agent) + Settings.Instance.AthleticBonus;
                Settings.Instance.PlayerSpeed = (6.2f * StatPatch.CalculateMaxSpeed(agent));
                Settings.Instance.HasPlayerMultiplier = false;
                Settings.Instance.HasSpeedModMultiplier = true;
            }
        }

    }
}
