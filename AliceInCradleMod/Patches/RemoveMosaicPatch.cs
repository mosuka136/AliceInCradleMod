using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch(typeof(MosaicShower), "FnDrawMosaic")]
        private class RemoveMosaicPatch
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
