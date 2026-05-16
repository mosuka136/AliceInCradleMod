using BetterExperience.BConfigManager;
using HarmonyLib;
using m2d;
using nel;
using System;
using System.Collections.Generic;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 阻止玩家获得指定异常状态。
        /// 读取存档时会临时放行，避免已有存档状态因加载流程被误过滤。
        /// </summary>
        [HarmonyPatch]
        public class ImmuneAbnormalitiesPatch
        {
            private static bool _isLoading = false;

            // 分项免疫开关映射到游戏 SER 枚举，便于 Add 前缀统一判断。
            private static readonly IDictionary<SER, Func<bool>> ImmuneAbnormalityConfigMap = new Dictionary<SER, Func<bool>>
            {
                { SER.MP_REDUCE, () => ConfigManager.EnableImmuneAbnormalityMpReduce.Value },
                { SER.BURST_TIRED, () => ConfigManager.EnableImmuneAbnormalityBurstTired.Value },
                { SER.CLT_BROKEN, () => ConfigManager.EnableImmuneAbnormalityClothesBroken.Value },
                { SER.OVERRUN_TIRED, () => ConfigManager.EnableImmuneAbnormalityOverRunTired.Value },
                { SER.SHIELD_BREAK, () => ConfigManager.EnableImmuneAbnormalityShieldBreak.Value },
                { SER.SLEEP, () => ConfigManager.EnableImmuneAbnormalitySleep.Value },
                { SER.BURNED, () => ConfigManager.EnableImmuneAbnormalityBurned.Value },
                { SER.FROZEN, () => ConfigManager.EnableImmuneAbnormalityFrozen.Value },
                { SER.PARALYSIS, () => ConfigManager.EnableImmuneAbnormalityParalysis.Value },
                { SER.CONFUSE, () => ConfigManager.EnableImmuneAbnormalityConfuse.Value },
                { SER.JAMMING, () => ConfigManager.EnableImmuneAbnormalityJamming.Value },
                { SER.PARASITISED, () => ConfigManager.EnableImmuneAbnormalityParasitised.Value },
                { SER.SHAMED, () => ConfigManager.EnableImmuneAbnormalityShamed.Value },
                { SER.SHAMED_SPLIT, () => ConfigManager.EnableImmuneAbnormalityShamedSplit.Value },
                { SER.SHAMED_WET, () => ConfigManager.EnableImmuneAbnormalityShamedWet.Value },
                { SER.SHAMED_EP, () => ConfigManager.EnableImmuneAbnormalityShamedEp.Value },
                { SER.SEXERCISE, () => ConfigManager.EnableImmuneAbnormalitySexercise.Value },
                { SER.FRUSTRATED, () => ConfigManager.EnableImmuneAbnormalityFrustrated.Value },
                { SER.ORGASM_AFTER, () => ConfigManager.EnableImmuneAbnormalityOrgasmAfter.Value },
                { SER.EGGED, () => ConfigManager.EnableImmuneAbnormalityEgged.Value },
                { SER.LAYING_EGG, () => ConfigManager.EnableImmuneAbnormalityLayingEgg.Value },
                { SER.DO_NOT_LAY_EGG, () => ConfigManager.EnableImmuneAbnormalityDoNotLayEgg.Value },
                { SER.NEAR_PEE, () => ConfigManager.EnableImmuneAbnormalityNearPee.Value },
                { SER.DRUNK, () => ConfigManager.EnableImmuneAbnormalityDrunk.Value },
                { SER.WEB_TRAPPED, () => ConfigManager.EnableImmuneAbnormalityWebTrapped.Value },
                { SER.STONE, () => ConfigManager.EnableImmuneAbnormalityStone.Value },
                { SER.ATK_DOWN, () => ConfigManager.EnableImmuneAbnormalityAtkDown.Value }
            };

            [HarmonyPrefix]
            [HarmonyPatch(typeof(M2Ser), nameof(M2Ser.Add))]
            public static bool AddPrefix(M2Ser __instance, SER ser)
            {
                try
                {
                    if (!_isLoading && (ConfigManager.EnableImmuneAbnormalities.Value || (ImmuneAbnormalityConfigMap.TryGetValue(ser, out var isEnabled) && isEnabled())))
                    {
                        HLog.Debug($"{nameof(ImmuneAbnormalitiesPatch)} applied for {ser}.");
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(ImmuneAbnormalitiesPatch)}", ex);
                    return true;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(M2Ser), nameof(M2Ser.readBinaryFrom))]
            public static void ReadBinaryFromPrefix()
            {
                _isLoading = true;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(M2Ser), nameof(M2Ser.readBinaryFrom))]
            public static void ReadBinaryFromPostfix()
            {
                _isLoading = false;
            }
        }
    }
}
