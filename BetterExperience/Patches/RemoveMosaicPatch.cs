using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch(typeof(MosaicShower), "FnDrawMosaic")]
        public class RemoveMosaicPatch
        {
            private static bool _hasLoggedActivation = false;

            static void Postfix(ref bool __result)
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
        }
    }
}
