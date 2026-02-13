using HarmonyLib;
using nel;

namespace BetterExperience
{
    internal partial class Patchs
    {
        [HarmonyPatch(typeof(NelItemManager), "readBinaryFrom")]
        private class BiggerBackpackPatch
        {
            static void Postfix(NelItemManager __instance)
            {
                if (!ConfigManager.EnableBiggerBackpack.Value)
                    return;

                if (__instance == null)
                {
                    HLog.Error("__instance is null.");
                    return;
                }
                var inventory = Traverse.Create(__instance).Field("StInventory").GetValue<ItemStorage>();
                if (inventory == null)
                {
                    HLog.Error("inventory is null.");
                    return;
                }
                inventory.row_max = ConfigManager.BackpackCapacity.Value;
            }
        }
    }
}
