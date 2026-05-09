using HarmonyLib;
using m2d;
using nel;
using nel.smnp;
using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class BattleStatisticsPatch
        {
            public static Dictionary<(ENEMYID, ENATTR), int> PlayerInjuryHpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static Dictionary<(ENEMYID, ENATTR), int> PlayerDamageHpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static Dictionary<(ENEMYID, ENATTR), int> EnemyInjuryHpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static Dictionary<(ENEMYID, ENATTR), int> EnemyDamageHpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static int TotalPlayerInjuryHpCounter { get; private set; } = 0;
            public static int TotalPlayerDamageHpCounter { get; private set; } = 0;
            public static int TotalEnemyInjuryHpCounter { get; private set; } = 0;
            public static int TotalEnemyDamageHpCounter { get; private set; } = 0;

            public static Dictionary<(ENEMYID, ENATTR), int> PlayerInjurySingleBattleHpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static Dictionary<(ENEMYID, ENATTR), int> PlayerDamageSingleBattleHpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static Dictionary<(ENEMYID, ENATTR), int> EnemyInjurySingleBattleHpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static Dictionary<(ENEMYID, ENATTR), int> EnemyDamageSingleBattleHpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static int TotalPlayerInjurySingleBattleHpCounter { get; private set; } = 0;
            public static int TotalPlayerDamageSingleBattleHpCounter { get; private set; } = 0;
            public static int TotalEnemyInjurySingleBattleHpCounter { get; private set; } = 0;
            public static int TotalEnemyDamageSingleBattleHpCounter { get; private set; } = 0;

            public static Dictionary<(ENEMYID, ENATTR), int> PlayerInjuryMpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static Dictionary<(ENEMYID, ENATTR), int> PlayerDamageMpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static Dictionary<(ENEMYID, ENATTR), int> EnemyInjuryMpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static Dictionary<(ENEMYID, ENATTR), int> EnemyDamageMpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static int TotalPlayerInjuryMpCounter { get; private set; } = 0;
            public static int TotalPlayerDamageMpCounter { get; private set; } = 0;
            public static int TotalEnemyInjuryMpCounter { get; private set; } = 0;
            public static int TotalEnemyDamageMpCounter { get; private set; } = 0;

            public static Dictionary<(ENEMYID, ENATTR), int> PlayerInjurySingleBattleMpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static Dictionary<(ENEMYID, ENATTR), int> PlayerDamageSingleBattleMpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static Dictionary<(ENEMYID, ENATTR), int> EnemyInjurySingleBattleMpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static Dictionary<(ENEMYID, ENATTR), int> EnemyDamageSingleBattleMpCounter { get; private set; } = new Dictionary<(ENEMYID, ENATTR), int>();
            public static int TotalPlayerInjurySingleBattleMpCounter { get; private set; } = 0;
            public static int TotalPlayerDamageSingleBattleMpCounter { get; private set; } = 0;
            public static int TotalEnemyInjurySingleBattleMpCounter { get; private set; } = 0;
            public static int TotalEnemyDamageSingleBattleMpCounter { get; private set; } = 0;

            public static bool IsInBattle { get; private set; } = false;
            public static float BattleStartTime { get; private set; } = 0f;
            public static float BattleEndTime { get; private set; } = 0f;

            private static object _objectAttackEnemy = null;
            private static object _objectAttackPlayer = null;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SummonerPlayer))]
            [HarmonyPatch(MethodType.Constructor)]
            [HarmonyPatch(
                new Type[] { typeof(EnemySummoner), typeof(EfParticleFuncCalc), typeof(CsvReaderA), typeof(bool), typeof(bool) },
                new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
            public static void SummonerPlayerConstructorPostfix()
            {
                try
                {
                    StartBattle();
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(BattleStatisticsPatch)}", ex);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(EnemySummoner), nameof(EnemySummoner.close))]
            public static void EnemySummonerClosePrefix()
            {
                try
                {
                    EndBattle();
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(BattleStatisticsPatch)}", ex);
                }
            }

            public static void StartBattle()
            {
                if (!IsInBattle)
                {
                    TotalPlayerDamageSingleBattleHpCounter = 0;
                    TotalPlayerDamageSingleBattleMpCounter = 0;
                    TotalPlayerInjurySingleBattleHpCounter = 0;
                    TotalPlayerInjurySingleBattleMpCounter = 0;
                    TotalEnemyDamageSingleBattleHpCounter = 0;
                    TotalEnemyDamageSingleBattleMpCounter = 0;
                    TotalEnemyInjurySingleBattleHpCounter = 0;
                    TotalEnemyInjurySingleBattleMpCounter = 0;
                    PlayerDamageSingleBattleHpCounter.Clear();
                    PlayerDamageSingleBattleMpCounter.Clear();
                    PlayerInjurySingleBattleHpCounter.Clear();
                    PlayerInjurySingleBattleMpCounter.Clear();
                    EnemyDamageSingleBattleHpCounter.Clear();
                    EnemyDamageSingleBattleMpCounter.Clear();
                    EnemyInjurySingleBattleHpCounter.Clear();
                    EnemyInjurySingleBattleMpCounter.Clear();

                    IsInBattle = true;
                    BattleStartTime = Time.time;
                    HLog.Debug("Battle Started");
                }
            }

            public static void EndBattle()
            {
                if (IsInBattle)
                {
                    IsInBattle = false;
                    BattleEndTime = Time.time;
                    HLog.Debug("Battle Ended");
                }
            }

            [HarmonyPatch]
            public class PlayerInjuryCounterPatch
            {
                [HarmonyPrefix]
                [HarmonyPatch(typeof(PR), nameof(PR.applyDamage),
                    new Type[] { typeof(NelAttackInfo), typeof(HITTYPE), typeof(bool) },
                    new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
                public static void ApplyDamagePrefix(NelAttackInfo Atk)
                {
                    try
                    {
                        var enemy = GetSource(Atk) as NelEnemy;

                        if (enemy != null)
                        {
                            _objectAttackPlayer = enemy;
                            HLog.Debug($"Player Injured by Enemy: {GetEnemyKey(enemy)}");
                        }
                        else
                        {
                            _objectAttackPlayer = null;
                            HLog.Debug($"Player Injured by {GetSource(Atk)?.GetType().Name ?? "Unknown Source"}");
                        }
                    }
                    catch (Exception ex)
                    {
                        HLog.Error($"Unexpected error in {nameof(PlayerInjuryCounterPatch)}", ex);
                    }
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(PR), nameof(PR.applyDamage),
                    new Type[] { typeof(NelAttackInfo), typeof(HITTYPE), typeof(bool) },
                    new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
                public static void ApplyDamagePostfix()
                {
                    _objectAttackPlayer = null;
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(PR), nameof(PR.setDamageCounter))]
                public static void SetDamageCounterPostfix(int delta_hp, int delta_mp)
                {
                    try
                    {
                        TotalPlayerInjuryHpCounter += -delta_hp;
                        TotalPlayerInjuryMpCounter += -delta_mp;

                        var enemy = _objectAttackPlayer as NelEnemy;

                        if (enemy != null)
                        {
                            var key = GetEnemyKey(enemy);

                            if (!PlayerInjuryHpCounter.ContainsKey(key))
                                PlayerInjuryHpCounter[key] = 0;

                            if (!PlayerInjuryMpCounter.ContainsKey(key))
                                PlayerInjuryMpCounter[key] = 0;

                            PlayerInjuryHpCounter[key] += -delta_hp;
                            PlayerInjuryMpCounter[key] += -delta_mp;

                            if (!EnemyDamageHpCounter.ContainsKey(key))
                                EnemyDamageHpCounter[key] = 0;

                            if (!EnemyDamageMpCounter.ContainsKey(key))
                                EnemyDamageMpCounter[key] = 0;

                            EnemyDamageHpCounter[key] += -delta_hp;
                            EnemyDamageMpCounter[key] += -delta_mp;
                        }

                        if (IsInBattle)
                        {
                            TotalPlayerInjurySingleBattleHpCounter += -delta_hp;
                            TotalPlayerInjurySingleBattleMpCounter += -delta_mp;

                            if (enemy != null)
                            {
                                var key = GetEnemyKey(enemy);

                                if (!PlayerInjurySingleBattleHpCounter.ContainsKey(key))
                                    PlayerInjurySingleBattleHpCounter[key] = 0;

                                if (!PlayerInjurySingleBattleMpCounter.ContainsKey(key))
                                    PlayerInjurySingleBattleMpCounter[key] = 0;

                                PlayerInjurySingleBattleHpCounter[key] += -delta_hp;
                                PlayerInjurySingleBattleMpCounter[key] += -delta_mp;

                                if (!EnemyDamageSingleBattleHpCounter.ContainsKey(key))
                                    EnemyDamageSingleBattleHpCounter[key] = 0;

                                if (!EnemyDamageSingleBattleMpCounter.ContainsKey(key))
                                    EnemyDamageSingleBattleMpCounter[key] = 0;

                                EnemyDamageSingleBattleHpCounter[key] += -delta_hp;
                                EnemyDamageSingleBattleMpCounter[key] += -delta_mp;
                            }
                        }

                        HLog.Debug($"Player Injury Hp Counter: {delta_hp}, Mp Counter: {delta_mp}");
                    }
                    catch (Exception ex)
                    {
                        HLog.Error($"Unexpected error in {nameof(PlayerInjuryCounterPatch)}", ex);
                    }
                }
            }

            [HarmonyPatch]
            public class EnemyInjuryCounterPatch
            {
                [HarmonyPrefix]
                [HarmonyPatch(typeof(NelEnemy), nameof(NelEnemy.applyDamage),
                    new Type[] { typeof(NelAttackInfo), typeof(HITTYPE), typeof(bool) },
                    new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
                public static void ApplyDamagePrefix(NelAttackInfo Atk)
                {
                    try
                    {
                        var source = GetSource(Atk);
                        if (source == null)
                        {
                            _objectAttackEnemy = null;
                            HLog.Debug($"Enemy Injured by Unknown Source");
                            return;
                        }

                        if (source is NelEnemy enemy)
                        {
                            _objectAttackEnemy = enemy;
                            HLog.Debug($"Enemy Injured by Enemy: {GetEnemyKey(enemy)}");
                        }
                        else if (source is PR)
                        {
                            _objectAttackEnemy = source;
                            HLog.Debug($"Enemy Injured by Player");
                        }
                        else
                        {
                            _objectAttackEnemy = null;
                            HLog.Debug($"Enemy Injured by {source.GetType().Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        HLog.Error($"Unexpected error in {nameof(EnemyInjuryCounterPatch)}", ex);
                    }
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(NelEnemy), nameof(NelEnemy.applyDamage),
                    new Type[] { typeof(NelAttackInfo), typeof(HITTYPE), typeof(bool) },
                    new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
                public static void ApplyDamagePostfix()
                {
                    _objectAttackEnemy = null;
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(NelEnemy), nameof(NelEnemy.setDamageCounter))]
                public static void SetDamageCounterPostfix(NelEnemy __instance, int delta_hp, int delta_mp)
                {
                    try
                    {
                        TotalEnemyInjuryHpCounter += -delta_hp;
                        TotalEnemyInjuryMpCounter += -delta_mp;

                        var attackedKey = GetEnemyKey(__instance);

                        if (!EnemyInjuryHpCounter.ContainsKey(attackedKey))
                            EnemyInjuryHpCounter[attackedKey] = 0;

                        if (!EnemyInjuryMpCounter.ContainsKey(attackedKey))
                            EnemyInjuryMpCounter[attackedKey] = 0;

                        EnemyInjuryHpCounter[attackedKey] += -delta_hp;
                        EnemyInjuryMpCounter[attackedKey] += -delta_mp;

                        if (_objectAttackEnemy is PR)
                        {
                            TotalPlayerDamageHpCounter += -delta_hp;
                            TotalPlayerDamageMpCounter += -delta_mp;

                            if (!PlayerDamageHpCounter.ContainsKey(attackedKey))
                                PlayerDamageHpCounter[attackedKey] = 0;

                            if (!PlayerDamageMpCounter.ContainsKey(attackedKey))
                                PlayerDamageMpCounter[attackedKey] = 0;

                            PlayerDamageHpCounter[attackedKey] += -delta_hp;
                            PlayerDamageMpCounter[attackedKey] += -delta_mp;
                        }
                        else if (_objectAttackEnemy is NelEnemy enemy)
                        {
                            TotalEnemyDamageHpCounter += -delta_hp;
                            TotalEnemyDamageMpCounter += -delta_mp;

                            var attackingKey = GetEnemyKey(enemy);

                            if (!EnemyDamageHpCounter.ContainsKey(attackingKey))
                                EnemyDamageHpCounter[attackingKey] = 0;

                            if (!EnemyDamageMpCounter.ContainsKey(attackingKey))
                                EnemyDamageMpCounter[attackingKey] = 0;

                            EnemyDamageHpCounter[attackingKey] += -delta_hp;
                            EnemyDamageMpCounter[attackingKey] += -delta_mp;
                        }

                        if (IsInBattle)
                        {
                            TotalEnemyInjurySingleBattleHpCounter += -delta_hp;
                            TotalEnemyInjurySingleBattleMpCounter += -delta_mp;

                            if (!EnemyInjurySingleBattleHpCounter.ContainsKey(attackedKey))
                                EnemyInjurySingleBattleHpCounter[attackedKey] = 0;

                            if (!EnemyInjurySingleBattleMpCounter.ContainsKey(attackedKey))
                                EnemyInjurySingleBattleMpCounter[attackedKey] = 0;

                            EnemyInjurySingleBattleHpCounter[attackedKey] += -delta_hp;
                            EnemyInjurySingleBattleMpCounter[attackedKey] += -delta_mp;

                            if (_objectAttackEnemy is PR)
                            {
                                TotalPlayerDamageSingleBattleHpCounter += -delta_hp;
                                TotalPlayerDamageSingleBattleMpCounter += -delta_mp;

                                if (!PlayerDamageSingleBattleHpCounter.ContainsKey(attackedKey))
                                    PlayerDamageSingleBattleHpCounter[attackedKey] = 0;

                                if (!PlayerDamageSingleBattleMpCounter.ContainsKey(attackedKey))
                                    PlayerDamageSingleBattleMpCounter[attackedKey] = 0;

                                PlayerDamageSingleBattleHpCounter[attackedKey] += -delta_hp;
                                PlayerDamageSingleBattleMpCounter[attackedKey] += -delta_mp;
                            }
                            else if (_objectAttackEnemy is NelEnemy enemy)
                            {
                                TotalEnemyDamageSingleBattleHpCounter += -delta_hp;
                                TotalEnemyDamageSingleBattleMpCounter += -delta_mp;

                                var attackingKey = GetEnemyKey(enemy);

                                if (!EnemyDamageSingleBattleHpCounter.ContainsKey(attackingKey))
                                    EnemyDamageSingleBattleHpCounter[attackingKey] = 0;

                                if (!EnemyDamageSingleBattleMpCounter.ContainsKey(attackingKey))
                                    EnemyDamageSingleBattleMpCounter[attackingKey] = 0;

                                EnemyDamageSingleBattleHpCounter[attackingKey] += -delta_hp;
                                EnemyDamageSingleBattleMpCounter[attackingKey] += -delta_mp;
                            }
                        }

                        HLog.Debug($"Enemy Injury Hp Counter: {delta_hp}, Mp Counter: {delta_mp}");
                    }
                    catch (Exception ex)
                    {
                        HLog.Error($"Unexpected error in {nameof(EnemyInjuryCounterPatch)}", ex);
                    }
                }
            }

            public static object GetSource(NelAttackInfo attack_info)
            {
                if (attack_info == null)
                    return null;

                if (attack_info.Caster != null)
                    return attack_info.Caster;

                if (attack_info.AttackFrom != null)
                    return attack_info.AttackFrom;

                if (attack_info.PublishMagic?.Caster != null)
                    return attack_info.PublishMagic.Caster;

                return null;
            }

            private static (ENEMYID, ENATTR) GetEnemyKey(NelEnemy enemy)
            {
                var enemyId = enemy.id & ~ENEMYID._OVERDRIVE_FLAG;
                if (enemy.isOverDrive())
                    enemyId |= ENEMYID._OVERDRIVE_FLAG;

                return (enemyId, enemy.nattr & ~ENATTR.__OPTIONAL);
            }
        }
    }
}
