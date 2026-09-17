using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using nel.mgm.farm;
using System;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 免疫敌怪拘束：拦截 <see cref="PR.initAbsorb"/>，并拆掉已经挂上的敌怪拘束。
        /// 牧场挤奶、剧情 QTE 和自慰小游戏不拦截。
        /// </summary>
        internal static class ImmuneEnemyAbsorb
        {
            private static bool _logged;

            internal static bool ImmuneOn => IsOn(ConfigManager.EnableImmuneEnemyAbsorb);

            internal static bool PrefixInitAbsorb(NelM2Attacker absorbBy, ref bool result)
            {
                if (!ImmuneEnemyAbsorbLogic.ShouldBlockEnemyAbsorb(
                        ImmuneOn,
                        absorbBy is NelEnemy,
                        absorbBy is NelNMgmFarmAnimal))
                    return true;

                result = false;
                LogOnce();
                return false;
            }

            internal static bool TryRelease(AbsorbManagerContainer con)
            {
                if (con == null || !ImmuneOn || con.Length <= 0)
                    return false;

                int length = con.Length;
                bool releasedAny = false;
                for (int i = length - 1; i >= 0; i--)
                {
                    var absorb = con.GetManagerItem(i);
                    if (absorb == null || !absorb.isActive())
                        continue;

                    var publisher = absorb.getPublishMover();
                    if (!ImmuneEnemyAbsorbLogic.ShouldBlockEnemyAbsorb(
                            ImmuneOn,
                            publisher is NelEnemy,
                            publisher is NelNMgmFarmAnimal))
                        continue;

                    absorb.kirimomi_release = false;
                    absorb.destruct();
                    releasedAny = true;
                }

                bool anyActive = false;
                for (int i = 0; i < con.Length; i++)
                {
                    var absorb = con.GetManagerItem(i);
                    if (absorb != null && absorb.isActive())
                    {
                        anyActive = true;
                        break;
                    }
                }

                if (!ImmuneEnemyAbsorbLogic.ShouldFinishContainerRelease(ImmuneOn, releasedAny, anyActive))
                    return false;

                con.release_type |= AbsorbManagerContainer.RELEASE_TYPE.GACHA;
                con.clear();
                LogOnce();
                return true;
            }

            private static bool IsOn(ConfigEntry<bool> entry)
            {
                return ConfigManager.EnableBetterExperience?.Value == true && entry?.Value == true;
            }

            private static void LogOnce()
            {
                if (_logged)
                    return;
                BLog.Debug($"{nameof(ImmuneEnemyAbsorb)} applied: block enemy absorb.");
                _logged = true;
            }
        }

        [HarmonyPatch]
        public class ImmuneEnemyAbsorbPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), nameof(PR.initAbsorb))]
            public static bool Prefix(NelM2Attacker AbsorbBy, ref bool __result)
            {
                try
                {
                    return ImmuneEnemyAbsorb.PrefixInitAbsorb(AbsorbBy, ref __result);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(ImmuneEnemyAbsorbPatch)} prefix", ex);
                    return true;
                }
            }
        }

        /// <summary>
        /// 开关打开时拆掉已经挂上的敌怪拘束，走原版 GACHA 释放。
        /// </summary>
        [HarmonyPatch]
        public class ImmuneEnemyAbsorbReleasePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(AbsorbManagerContainer), nameof(AbsorbManagerContainer.runAbsorbPr))]
            public static bool Prefix(AbsorbManagerContainer __instance, ref bool __result)
            {
                try
                {
                    if (!ImmuneEnemyAbsorb.TryRelease(__instance))
                        return true;

                    __result = false;
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(ImmuneEnemyAbsorbReleasePatch)} prefix", ex);
                    return true;
                }
            }
        }
    }
}
