using BetterExperience.BepConfigManager;
using HarmonyLib;
using m2d;
using nel;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class ImmuneAbnormalitiesPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(M2Ser), nameof(M2Ser.Add))]
            private static bool Prefix(SER ser)
            {
                if (ConfigManager.EnableImmuneAbnormalities.Value)
                    return false;

                switch (ser)
                {
                    case SER.MP_REDUCE:
                        return !ConfigManager.EnableImmuneAbnormalityMpReduce.Value;
                    case SER.BURST_TIRED:
                        return !ConfigManager.EnableImmuneAbnormalityBurstTired.Value;
                    case SER.CLT_BROKEN:
                        return !ConfigManager.EnableImmuneAbnormalityClothesBroken.Value;
                    case SER.OVERRUN_TIRED:
                        return !ConfigManager.EnableImmuneAbnormalityOverRunTired.Value;
                    case SER.SHIELD_BREAK:
                        return !ConfigManager.EnableImmuneAbnormalityShieldBreak.Value;
                    case SER.SLEEP:
                        return !ConfigManager.EnableImmuneAbnormalitySleep.Value;
                    case SER.BURNED:
                        return !ConfigManager.EnableImmuneAbnormalityBurned.Value;
                    case SER.FROZEN:
                        return !ConfigManager.EnableImmuneAbnormalityFrozen.Value;
                    case SER.PARALYSIS:
                        return !ConfigManager.EnableImmuneAbnormalityParalysis.Value;
                    case SER.CONFUSE:
                        return !ConfigManager.EnableImmuneAbnormalityConfuse.Value;
                    case SER.JAMMING:
                        return !ConfigManager.EnableImmuneAbnormalityJamming.Value;
                    case SER.PARASITISED:
                        return !ConfigManager.EnableImmuneAbnormalityParasitised.Value;
                    case SER.SHAMED:
                        return !ConfigManager.EnableImmuneAbnormalityShamed.Value;
                    case SER.SHAMED_SPLIT:
                        return !ConfigManager.EnableImmuneAbnormalityShamedSplit.Value;
                    case SER.SHAMED_WET:
                        return !ConfigManager.EnableImmuneAbnormalityShamedWet.Value;
                    case SER.SHAMED_EP:
                        return !ConfigManager.EnableImmuneAbnormalityShamedEp.Value;
                    case SER.SEXERCISE:
                        return !ConfigManager.EnableImmuneAbnormalitySexercise.Value;
                    case SER.FRUSTRATED:
                        return !ConfigManager.EnableImmuneAbnormalityFrustrated.Value;
                    case SER.ORGASM_AFTER:
                        return !ConfigManager.EnableImmuneAbnormalityOrgasmAfter.Value;
                    case SER.EGGED:
                        return !ConfigManager.EnableImmuneAbnormalityEgged.Value;
                    case SER.LAYING_EGG:
                        return !ConfigManager.EnableImmuneAbnormalityLayingEgg.Value;
                    case SER.DO_NOT_LAY_EGG:
                        return !ConfigManager.EnableImmuneAbnormalityDoNotLayEgg.Value;
                    case SER.NEAR_PEE:
                        return !ConfigManager.EnableImmuneAbnormalityNearPee.Value;
                    case SER.DRUNK:
                        return !ConfigManager.EnableImmuneAbnormalityDrunk.Value;
                    case SER.WEB_TRAPPED:
                        return !ConfigManager.EnableImmuneAbnormalityWebTrapped.Value;
                    case SER.STONE:
                        return !ConfigManager.EnableImmuneAbnormalityStone.Value;
                    default:
                        return true;
                }
            }
        }
    }
}
