using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch(typeof(MosaicShower), "FnDrawMosaic")]
        public class RemoveMosaicPatch
        {
            static void Postfix(ref bool __result)
            {
                if (ConfigManager.EnableMosaic.Value)
                    return;

                __result = false;
            }
        }
    }
}
