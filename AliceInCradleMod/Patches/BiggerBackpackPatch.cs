using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    internal partial class Patchs
    {
        [HarmonyPatch(typeof(NelItemManager), "readBinaryFrom")]
        private class BiggerBackpackPatch
        {
            private static ItemStorage inventory;
            private static int row_max;
            static void Postfix(NelItemManager __instance)
            {
                if (!ConfigManager.EnableBiggerBackpack.Value)
                    return;

                if (__instance == null)
                {
                    HLog.Error("__instance is null.");
                    return;
                }
                inventory = Traverse.Create(__instance).Field("StInventory").GetValue<ItemStorage>();
                if (inventory == null)
                {
                    HLog.Error("inventory is null.");
                    return;
                }

                row_max = inventory.row_max;
                OnSiteProtectionManager.Instance.OnSiteProtectionActivated += RecoverRowMax;

                inventory.row_max = ConfigManager.BackpackCapacity.Value;
            }

            private static void RecoverRowMax()
            {
                inventory.row_max = row_max;
            }
        }
    }
}
