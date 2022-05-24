using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Incarnate.UnitControl;

namespace Incarnate
{
	public class WorldBoxMod : MonoBehaviour
	{
		public static bool ShowHideMaster = false;
		public static bool ShowHide = false;
		public static UnitController playerController;

		void Start()
        {
			IncarnateTraitBoot();
			playerController = new UnitController();
		}

		void Update()
		{
			bool isOn = Config.gameLoaded;
			if (isOn)
			{
				playerController.Update();
			}
            else Clean();
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

		public static void IncarnateGUI()
        {
			Rect buttonSize = new Rect(Screen.width - 100, 60, 5f, 5f);//100f, 20f);
			WorldTile mousePos = MapBox.instance.getMouseTilePos();

			if (WorldBoxMod.ShowHideMaster)
			{
				if (GUI.Button(buttonSize, "IncarnateUI"))
				{
					ShowHide = !ShowHide;
				}

				if (ShowHide)
                {

				}// SHOWHIDE END
				
			}// SHOWHIDE MASTER END

			if (playerController.isControlling)
            {
				BaseStats playerStats = Reflection.GetField(playerController.playerActor.GetType(), playerController.playerActor, "curStats") as BaseStats;
				GUI.backgroundColor = Color.white;
				GUI.contentColor = Color.black;
				string presentedHealth = playerController.playerActor.base_data.health + "/" + playerStats.health;
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