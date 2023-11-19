﻿using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;

namespace DynBoneWrangler
{
    public class DynBoneWrangler : ResoniteMod
    {
        public override string Name => "DynBoneWrangler";
        public override string Author => "isotach";
        public override string Version => "1.0.1";
        public override string Link => "https://github.com/isovel/DynBoneWrangler";

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> enabled =
            new ModConfigurationKey<bool>("enabled",
                "Should the mod be enabled", () => true);

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> halfRateUpdates =
            new ModConfigurationKey<bool>("halfRateUpdates",
                "Reduce DynamicBoneChain updates", () => true);

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<float> disableThreshold =
            new ModConfigurationKey<float>("disableThreshold",
                "Disable DynamicBoneChain updates when your FPS is below this value", () => 17.0f);

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<float> enableThreshold =
            new ModConfigurationKey<float>("enableThreshold",
                "Re-enable DynamicBoneChain updates when your FPS is above this value", () => 22.0f);

        private static ModConfiguration Config;

        public override void OnEngineInit() {
            Harmony harmony = new Harmony("ch.isota.DynBoneWrangler");
            Config = GetConfiguration();
            Config.Save(true);
            harmony.PatchAll();
        
            Msg("Initialized!");
        }

        [HarmonyPatch(typeof(DynamicBoneChainManager))]
        class DynamicBoneChainManager_Patch
        {
            private static bool _shouldUpdate = true;

            private static bool _throttleVar = false;
            
            [HarmonyPatch("Update")]
            static bool Prefix(Worker __instance)
            {
                if (!Config.GetValue(enabled)) return true;

                if (Config.GetValue(halfRateUpdates) && _throttleVar == true)
                {
                    _throttleVar = false;

                    return false;
                }
                
                CheckShouldUpdate(__instance);

                if (_shouldUpdate) return true;
                
                return false;
            }

            private static void CheckShouldUpdate(Worker worker)
            {
                float localUserFPS = worker.Engine.SystemInfo.ImmediateFPS;
                
                if (_shouldUpdate)
                {
                    if (localUserFPS < Config.GetValue(disableThreshold)) _shouldUpdate = false;
                }
                else
                {
                    if (localUserFPS > Config.GetValue(enableThreshold)) _shouldUpdate = true;
                    
                    if (Config.GetValue(halfRateUpdates) == true) _throttleVar = true;
                }
            }
        }
    }
}
