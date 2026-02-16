using HarmonyLib;
using nel;

namespace BetterExperience
{
    internal partial class Patchs
    {
        [HarmonyPatch]
        private class BetterSaveSitePatch
        {
            [HarmonyPatch(typeof(NelM2DBase), "canSaveInCurMap")]
            private static bool Prefix(NelM2DBase __instance, ref bool __result)
            {
                if(!ConfigManager.EnableBetterSaveSite.Value)
                    return true;

                if (__instance == null)
                    return true;

                if(__instance.curMap == null)
                    return true;

                __result = true;
                return false;
            }
        }
    }
}
