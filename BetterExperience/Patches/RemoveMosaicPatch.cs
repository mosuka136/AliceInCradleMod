using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch(typeof(MosaicShower), "FnDrawMosaic")]
        public class RemoveMosaicPatch
        {
            private static bool _hasLoggedActivation = false;

            public static void Postfix(ref bool __result)
            {
                try
                {
                    if (ConfigManager.EnableMosaic.Value)
                        return;

                    __result = false;

                    if (!_hasLoggedActivation)
                    {
                        HLog.Debug($"{nameof(RemoveMosaicPatch)} applied.");
                        _hasLoggedActivation = true;
                    }
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveMosaicPatch)}.", ex);
                }
            }
        }
    }
}
