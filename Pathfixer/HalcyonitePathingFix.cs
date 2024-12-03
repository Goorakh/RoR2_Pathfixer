using BepInEx.Configuration;
using UnityEngine.AddressableAssets;
using UnityEngine;
using System.Collections.Generic;
using RoR2.CharacterAI;
using Pathfixer.Utilities.Extensions;

namespace Pathfixer
{
    static class HalcyonitePathingFix
    {
        public static ConfigEntry<bool> EnableHalcyonitePathingChanges { get; private set; }

        static AISkillDriver[] _enableNodeGraphSkillDriverPrefabs = [];

        public static void Init(ConfigFile configFile)
        {
            EnableHalcyonitePathingChanges = configFile.Bind(new ConfigDefinition("Tweaks", "Enable Halcyonite Pathing Changes"), true, new ConfigDescription("Enables or disables tweaks to Halcyonite AI to make it use pathfinding"));
            EnableHalcyonitePathingChanges.SettingChanged += onEnableHalcyonitePathingChangesChanged;

            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Halcyonite/HalcyoniteMaster.prefab").CallOnSuccess(halcyoniteMaster =>
            {
                AISkillDriver[] skillDrivers = halcyoniteMaster.GetComponents<AISkillDriver>();
                List<AISkillDriver> enableNodeGraphSkillDrivers = new List<AISkillDriver>(skillDrivers.Length);

                foreach (AISkillDriver skillDriver in skillDrivers)
                {
                    if (skillDriver.customName == "Follow Target" || skillDriver.customName == "WhirlwindRush")
                    {
                        if (!skillDriver.ignoreNodeGraph)
                        {
                            Log.Warning($"Halcyonite SkillDriver {skillDriver.customName} is already using NodeGraph");
                            continue;
                        }

                        enableNodeGraphSkillDrivers.Add(skillDriver);
                    }
                }

                _enableNodeGraphSkillDriverPrefabs = [.. enableNodeGraphSkillDrivers];
                refreshIgnoreNodeGraph();
            });
        }

        static void onEnableHalcyonitePathingChangesChanged(object sender, System.EventArgs e)
        {
            refreshIgnoreNodeGraph();
        }

        static void refreshIgnoreNodeGraph()
        {
            foreach (AISkillDriver skillDriver in _enableNodeGraphSkillDriverPrefabs)
            {
                skillDriver.ignoreNodeGraph = !EnableHalcyonitePathingChanges.Value;
            }
        }
    }
}
