using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace ThrDtoActTwo
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.inscryption.thrdtoact2";
        private const string PluginName = "ThrDtoActTwo";
        private const string PluginVersion = "1.0.0";

        private Harmony harmony;

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginName} is loaded!");
            
            harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
            
            Logger.LogInfo("Harmony patches applied");
        }

        private void OnDestroy()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }
    }
}
