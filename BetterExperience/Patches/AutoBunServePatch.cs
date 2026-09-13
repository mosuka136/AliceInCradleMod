using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using BetterExperience.BPatchGUI;
using evt;
using HarmonyLib;
using m2d;
using nel;
using nel.mgm.bun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static BetterExperience.Patches.AutoBunServeLogic;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 兔女郎酒吧小游戏自动配送：注入 <see cref="BunPrRunner"/> 的左右走输入，
        /// 用原版走路速度和托盘动画走到吧台取酒、再走到对应客人面前送出。
        /// 到位后直接调用 <see cref="BUN.Pickup"/> / <see cref="BUN.ServeToMob"/> / <see cref="BUN.fineBunTrash"/>，
        /// 不伪造 Check 键，避免误触对话。体力低时腾空双手并按住道具键吃零食回血。
        /// 忙碌（递酒动画、受伤、被摸）时松手，以免打断送酒动作。
        /// </summary>
        internal static class AutoBunServe
        {
            internal static readonly object StatusKey = new object();

            private static readonly FieldInfo PoolServedField = AccessTools.Field(typeof(BUN), "PoolServedCocktail");
            private static readonly FieldInfo AMobAppearField = AccessTools.Field(typeof(BUN), "AMobAppear");
            private static readonly FieldInfo BunTrashField = AccessTools.Field(typeof(BUN), "BunTrash");
            private static readonly FieldInfo BunField = AccessTools.Field(typeof(BunPrRunner), "Bun");
            private static readonly MethodInfo GetStatUi = AccessTools.Method(typeof(BUN), "getStatUi");
            private static readonly FieldInfo BartenBunField = AccessTools.Field(typeof(MvNelNNEAListener_Barten), "Bun");
            private static readonly FieldInfo BartenStateField = AccessTools.Field(typeof(MvNelNNEAListener_Barten), "state");
            private static readonly FieldInfo BartenPosWalkField = AccessTools.Field(typeof(MvNelNNEAListener_Barten), "PosWalk");
            private static readonly FieldInfo BartenAssignedField = AccessTools.Field(typeof(MvNelNNEAListener_Barten), "AssignedB")
                ?? AccessTools.Field(typeof(MvNelNNEAListener), "Assigned");

            private static int _bartenderIdleWalkFrames;
            private static int _bartenderShakeWaitFrames;
            private static int _framesSinceProduce;
            private static int _lastNotServed = -1;
            private static float _lastBartenderX;
            private static readonly PropertyInfo PoolCountProperty = PoolServedField == null
                ? null
                : AccessTools.Property(PoolServedField.FieldType, "Count");
            private static readonly MethodInfo PoolGetUsing = PoolServedField == null
                ? null
                : AccessTools.Method(PoolServedField.FieldType, "GetUsing", new[] { typeof(int) });
            private static readonly MethodInfo GetMoveKeyFlag = AccessTools.Method(typeof(M2MoverPr), "getMoveKey", new[] { typeof(bool) });

            private static readonly List<int> Carrying = new List<int>(2);
            private static readonly List<AutoBunDrink> Bar = new List<AutoBunDrink>(8);
            private static readonly List<AutoBunCustomer> Customers = new List<AutoBunCustomer>(8);
            private static readonly List<BUN.ServedCocktail> BarObjects = new List<BUN.ServedCocktail>(8);
            private static readonly List<M2LpMgmBunny.MobEntry> Mobs = new List<M2LpMgmBunny.MobEntry>(8);

            private static bool _enabled;
            private static bool _statusShown;
            private static bool _loggedActivation;
            private static bool _broken;

            private static BUN _bun;
            private static AutoBunAction _pending;
            private static BUN.ServedCocktail _stickyPickup;
            private static M2BunEventItem _stickyServe;

            internal static bool GetAutoBunServe() => _enabled;

            internal static void SetAutoBunServe(bool enabled)
            {
                try
                {
                    if (_enabled == enabled)
                    {
                        SyncStatus();
                        return;
                    }

                    _enabled = enabled;
                    ResetDriver();
                    NoticeGUI.Show(
                        enabled ? TranslatorResource.AutoBunServeEnabled : TranslatorResource.AutoBunServeDisabled,
                        owner: StatusKey);
                    SyncStatus();
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetAutoBunServe)}", ex);
                }
            }

            internal static bool IsEnabled()
            {
                return ConfigManager.EnableBetterExperience?.Value == true && _enabled;
            }

            internal static void ThrowIfUnresolved()
            {
                if (PoolServedField == null || AMobAppearField == null || PoolCountProperty == null || PoolGetUsing == null)
                    throw new InvalidOperationException("BUN served-cocktail pool or mob list was not found.");
            }

            internal static void CaptureGame(BUN bun)
            {
                _bun = bun;
                ResetSticky();
                _broken = false;
                _pending = Wait();
                ResetBartenderWatch();
            }

            internal static void ReleaseGame(BUN bun)
            {
                if (_bun != bun && bun != null)
                    return;
                _bun = null;
                ResetSticky();
                _pending = Wait();
            }

            internal static void ResetDriver()
            {
                ResetSticky();
                _pending = Wait();
                _broken = false;
                ResetBartenderWatch();
            }

            internal static void ResetBartenderWatch()
            {
                _bartenderIdleWalkFrames = 0;
                _bartenderShakeWaitFrames = 0;
                _framesSinceProduce = 0;
                _lastNotServed = -1;
                _lastBartenderX = 0f;
            }

            public static M2MoverPr.MOVEK AutoGetMoveKey(M2MoverPr mover)
            {
                if (TryGetAutoMove(mover, out var key))
                    return key;
                return mover is PR pr ? pr.getMoveKey() : mover.getMoveKey();
            }

            public static M2MoverPr.MOVEK AutoGetMoveKey(M2MoverPr mover, bool flag)
            {
                if (TryGetAutoMove(mover, out var key))
                    return key;
                if (GetMoveKeyFlag != null)
                    return (M2MoverPr.MOVEK)GetMoveKeyFlag.Invoke(mover, new object[] { flag });
                return mover is PR pr ? pr.getMoveKey() : mover.getMoveKey();
            }

            public static bool AutoIsItmPD(M2MoverPr mover)
            {
                if (WantsItemHold())
                    return !IsEating(mover);
                return mover.isItmPD();
            }

            public static bool AutoIsItmO(M2MoverPr mover)
            {
                if (WantsItemHold())
                    return true;
                return mover.isItmO();
            }

            public static bool AutoIsItmO(M2MoverPr mover, int press)
            {
                if (WantsItemHold())
                    return true;
                return mover.isItmO(press);
            }

            internal static void AfterRunnerTick(BunPrRunner runner, PR pr)
            {
                try
                {
                    if (!IsEnabled() || _broken || runner == null || pr == null)
                        return;
                    if (pr.isMoveScriptActive() || runner.isBusy() || EV.isActive())
                        return;

                    var bun = GetBun(runner);
                    if (!IsGameReady(bun) || bun.omorashi || bun.restroom_lock)
                        return;

                    if (!_pending.ShouldInteract)
                        return;

                    Perform(_pending, bun, pr);
                    LogActivationOnce();
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AutoBunServe)}.{nameof(AfterRunnerTick)}", ex);
                    _broken = true;
                    ResetSticky();
                }
            }

            private static bool TryGetAutoMove(M2MoverPr mover, out M2MoverPr.MOVEK key)
            {
                key = default;
                try
                {
                    _pending = Wait();
                    if (!IsEnabled() || _broken || !(mover is PR pr))
                        return false;

                    var runner = pr.SpRunner as BunPrRunner;
                    var bun = GetBun(runner);
                    if (!IsGameReady(bun) || bun.omorashi || bun.restroom_lock || EV.isActive())
                        return false;
                    if (pr.isMoveScriptActive())
                        return false;

                    bool eating = runner.isEating() || runner.isEatingInputting();
                    if (runner.isBusy() && !eating)
                    {
                        _pending = Wait(pr.x);
                        key = default;
                        return true;
                    }

                    _pending = DecideFrame(bun, pr, eating);
                    UpdateSticky();
                    key = ToMoveKey(_pending.Walk);
                    if (_pending.ShouldWalk)
                        LogActivationOnce();
                    return true;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AutoBunServe)}.{nameof(TryGetAutoMove)}", ex);
                    _broken = true;
                    ResetSticky();
                    return false;
                }
            }

            private static AutoBunAction DecideFrame(BUN bun, PR pr, bool eating)
            {
                Snapshot(bun);
                int stickyPickup = _stickyPickup == null ? -1 : BarObjects.IndexOf(_stickyPickup);
                int stickyServe = -1;
                if (_stickyServe != null)
                {
                    for (int i = 0; i < Mobs.Count; i++)
                    {
                        if (Mobs[i] != null && (object)Mobs[i].EvC == _stickyServe)
                        {
                            stickyServe = i;
                            break;
                        }
                    }
                }

                var trash = BunTrashField?.GetValue(bun) as M2EventItem;
                float trashX = trash != null ? trash.x : float.NaN;
                bool trashExists = trash != null;
                float idleX = bun.NBarten != null ? bun.NBarten.x : pr.x;
                var stat = GetStatUi == null ? null : GetStatUi.Invoke(bun, null) as UiBunStatus;
                float hp = stat == null ? 100f : stat.hp_ratio * UiBunStatus.max_hp;
                return Decide(
                    pr.x,
                    Carrying,
                    Bar,
                    Customers,
                    trashX,
                    trashExists,
                    idleX,
                    false,
                    stickyPickup,
                    stickyServe,
                    hp,
                    eating);
            }

            private static bool WantsItemHold()
            {
                return IsEnabled() && !_broken && _pending.Act == AutoBunAct.Eat;
            }

            private static bool IsEating(M2MoverPr mover)
            {
                var runner = (mover as PR)?.SpRunner as BunPrRunner;
                return runner != null && (runner.isEating() || runner.isEatingInputting());
            }

            internal static bool TryHandlePendulumKey(AbsorbManager absorb, out bool press)
            {
                press = false;
                if (!IsEnabled() || _broken || absorb == null)
                    return false;
                if (!(absorb.Listener is BunPrRunner))
                    return false;
                var gacha = absorb.get_Gacha();
                if (gacha == null)
                    return false;
                if (gacha.type != PrGachaItem.TYPE.PENDULUM && gacha.type != PrGachaItem.TYPE.PENDULUM_ONNIE)
                    return false;
                var pendulum = gacha.getPendulumDrawer();
                press = ShouldPressPendulum(pendulum != null && pendulum.isAccepting());
                return true;
            }

            internal static bool ShouldSkipFrozenBartenderWalk(MvNelNNEAListener_Barten bartender)
            {
                if (!IsEnabled() || _broken || bartender == null)
                    return false;
                var bun = BartenBunField?.GetValue(bartender) as BUN;
                return ShouldFinishFrozenWalk(bartender.isMoveScriptActive(false), bun != null && bun.isGameProgressing());
            }

            internal static void FinishBartenderWalk(MvNelNNEAListener_Barten bartender)
            {
                if (BartenPosWalkField == null)
                    return;
                var pos = (Vector3)BartenPosWalkField.GetValue(bartender);
                pos.y = 0f;
                BartenPosWalkField.SetValue(bartender, pos);
                var assigned = BartenAssignedField?.GetValue(bartender) as M2Mover;
                assigned?.getPhysic()?.setWalkXSpeed(0f);
            }

            internal static void KickBartender(MvNelNNEAListener_Barten bartender)
            {
                if (!IsEnabled() || _broken || bartender == null || BartenBunField == null)
                    return;
                var bun = BartenBunField.GetValue(bartender) as BUN;
                if (bun == null || !bun.isGameProgressing())
                {
                    ResetBartenderWatch();
                    return;
                }

                int notServed = bun.getNotServedCount();
                if (notServed < _lastNotServed || notServed <= 0)
                    _framesSinceProduce = 0;
                else
                    _framesSinceProduce++;
                _lastNotServed = notServed;

                var pos = BartenPosWalkField == null
                    ? Vector3.zero
                    : (Vector3)BartenPosWalkField.GetValue(bartender);
                bool walking = pos.y > 0f;
                float moved = Math.Abs(bartender.x - _lastBartenderX);
                if (walking && moved < 0.01f)
                    _bartenderIdleWalkFrames++;
                else
                    _bartenderIdleWalkFrames = 0;
                _lastBartenderX = bartender.x;
                if (ShouldUnstickIdleWalk(walking, moved, _bartenderIdleWalkFrames))
                {
                    pos.y = 0f;
                    BartenPosWalkField?.SetValue(bartender, pos);
                    _bartenderIdleWalkFrames = 0;
                    BLog.Debug($"{nameof(AutoBunServe)} unstuck bartender walk.");
                }

                var state = BartenStateField == null
                    ? MvNelNNEAListener_Barten.CSTATE.OFFLINE
                    : (MvNelNNEAListener_Barten.CSTATE)BartenStateField.GetValue(bartender);
                if (state == MvNelNNEAListener_Barten.CSTATE.SHAKE && pos.y == 0f)
                    _bartenderShakeWaitFrames++;
                else
                    _bartenderShakeWaitFrames = 0;
                if (ShouldForceShakeStand(pos.y, _bartenderShakeWaitFrames))
                {
                    var assigned = BartenAssignedField?.GetValue(bartender) as M2Mover;
                    assigned?.SpSetPose("bar_stand", -1, null, false);
                    _bartenderShakeWaitFrames = 0;
                    BLog.Debug($"{nameof(AutoBunServe)} forced bartender stand for shake.");
                }

                if (ShouldForceProduce(notServed, _framesSinceProduce))
                {
                    bun.ServeOnTable(bartender.x);
                    _framesSinceProduce = 0;
                    BLog.Debug($"{nameof(AutoBunServe)} forced bartender to place a drink.");
                }
            }

            private static void UpdateSticky()
            {
                if (_pending.Act == AutoBunAct.Pickup &&
                    _pending.TargetIndex >= 0 && _pending.TargetIndex < BarObjects.Count)
                {
                    _stickyPickup = BarObjects[_pending.TargetIndex];
                    _stickyServe = null;
                    return;
                }
                if (_pending.Act == AutoBunAct.Serve &&
                    _pending.TargetIndex >= 0 && _pending.TargetIndex < Mobs.Count)
                {
                    var mob = Mobs[_pending.TargetIndex];
                    _stickyServe = mob == null ? null : mob.EvC;
                    _stickyPickup = null;
                    return;
                }
                ResetSticky();
            }

            private static void Perform(AutoBunAction action, BUN bun, PR pr)
            {
                switch (action.Act)
                {
                    case AutoBunAct.Pickup:
                        if (action.TargetIndex < 0 || action.TargetIndex >= BarObjects.Count)
                            return;
                        var cocktail = BarObjects[action.TargetIndex];
                        if (cocktail == null || !cocktail.valid)
                            return;
                        bun.Pickup(cocktail, pr);
                        _stickyPickup = null;
                        break;
                    case AutoBunAct.Serve:
                        if (action.TargetIndex < 0 || action.TargetIndex >= Mobs.Count)
                            return;
                        var mob = Mobs[action.TargetIndex];
                        if (mob?.EvC == null || !mob.Order.valid)
                            return;
                        bun.ServeToMob(mob.EvC);
                        _stickyServe = null;
                        break;
                    case AutoBunAct.Trash:
                        bun.fineBunTrash(true);
                        break;
                }
            }

            private static void Snapshot(BUN bun)
            {
                Carrying.Clear();
                Bar.Clear();
                Customers.Clear();
                BarObjects.Clear();
                Mobs.Clear();

                var orders = bun.getCarryingOrders();
                if (orders != null)
                {
                    for (int i = 0; i < orders.Length; i++)
                    {
                        if (orders[i].valid)
                            Carrying.Add(orders[i].cocktail_index);
                    }
                }

                var pool = PoolServedField?.GetValue(bun);
                if (pool != null && PoolCountProperty != null && PoolGetUsing != null)
                {
                    int count = (int)PoolCountProperty.GetValue(pool);
                    for (int i = 0; i < count; i++)
                    {
                        var served = PoolGetUsing.Invoke(pool, new object[] { i }) as BUN.ServedCocktail;
                        if (served == null || !served.valid)
                            continue;
                        Bar.Add(new AutoBunDrink(served.x, served.Order.cocktail_index));
                        BarObjects.Add(served);
                    }
                }

                if (!(AMobAppearField?.GetValue(bun) is IList list))
                    return;
                foreach (M2LpMgmBunny.MobEntry mob in list)
                {
                    Mobs.Add(mob);
                    bool canServe = mob != null
                        && mob.mbstate == M2LpMgmBunny.MBSTATE.WAIT
                        && mob.Order.valid
                        && mob.EvC != null
                        && !mob.EvC.isAttackMovingExecuting();
                    float x = mob == null ? 0f : mob.x;
                    int cocktail = mob != null && mob.Order.valid ? mob.Order.cocktail_index : -1;
                    float time = mob == null ? 0f : mob.t;
                    int orderId = mob != null && mob.Order.valid ? mob.Order.id : int.MaxValue;
                    Customers.Add(new AutoBunCustomer(x, cocktail, time, canServe, orderId));
                }
            }

            private static BUN GetBun(BunPrRunner runner)
            {
                if (runner == null)
                    return _bun;
                if (BunField != null)
                {
                    var bun = BunField.GetValue(runner) as BUN;
                    if (bun != null)
                        _bun = bun;
                }
                return _bun;
            }

            private static bool IsGameReady(BUN bun)
            {
                return bun != null && bun.isGameProgressing();
            }

            private static M2MoverPr.MOVEK ToMoveKey(int walk)
            {
                if (walk < 0)
                    return M2MoverPr.MOVEK.TO_L2;
                if (walk > 0)
                    return M2MoverPr.MOVEK.TO_R2;
                return default;
            }

            private static void ResetSticky()
            {
                _stickyPickup = null;
                _stickyServe = null;
            }

            private static void LogActivationOnce()
            {
                if (_loggedActivation)
                    return;
                BLog.Debug($"{nameof(AutoBunServe)} applied.");
                _loggedActivation = true;
            }

            private static void SyncStatus()
            {
                if (IsEnabled())
                {
                    if (_statusShown)
                        return;
                    NoticeGUI.SetStatus(StatusKey, TranslatorResource.AutoBunServeActive);
                    _statusShown = true;
                }
                else
                    HideStatus();
            }

            private static void HideStatus()
            {
                if (!_statusShown)
                    return;
                NoticeGUI.RemoveStatus(StatusKey);
                _statusShown = false;
            }
        }

        /// <summary>
        /// 注入酒吧玩家 runner 的左右走，并在到位后执行取酒/送酒/丢弃。
        /// </summary>
        [HarmonyPatch]
        public class AutoBunServeMovePatch
        {
            public static MethodBase TargetMethod()
            {
                MethodBase method = null;
                var map = typeof(BunPrRunner).GetInterfaceMap(typeof(ISpecialPrRunner));
                for (int i = 0; i < map.InterfaceMethods.Length; i++)
                {
                    if (map.InterfaceMethods[i].Name == nameof(ISpecialPrRunner.runPreSPPR))
                    {
                        method = map.TargetMethods[i];
                        break;
                    }
                }
                if (method == null)
                    method = AccessTools.Method(typeof(BunPrRunner), "nel.ISpecialPrRunner.runPreSPPR")
                        ?? AccessTools.DeclaredMethod(typeof(BunPrRunner), nameof(ISpecialPrRunner.runPreSPPR));
                if (method == null)
                    throw new InvalidOperationException("BunPrRunner.runPreSPPR was not found.");
                AutoBunServe.ThrowIfUnresolved();
                return method;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var autoMove = AccessTools.Method(typeof(AutoBunServe), nameof(AutoBunServe.AutoGetMoveKey), new[] { typeof(M2MoverPr) });
                var autoMoveFlag = AccessTools.Method(typeof(AutoBunServe), nameof(AutoBunServe.AutoGetMoveKey), new[] { typeof(M2MoverPr), typeof(bool) });
                var autoItmPD = AccessTools.Method(typeof(AutoBunServe), nameof(AutoBunServe.AutoIsItmPD), new[] { typeof(M2MoverPr) });
                var autoItmO = AccessTools.Method(typeof(AutoBunServe), nameof(AutoBunServe.AutoIsItmO), new[] { typeof(M2MoverPr) });
                var autoItmOFlag = AccessTools.Method(typeof(AutoBunServe), nameof(AutoBunServe.AutoIsItmO), new[] { typeof(M2MoverPr), typeof(int) });

                var result = new List<CodeInstruction>(instructions);
                int replacedMove = 0;
                int replacedPd = 0;
                int replacedO = 0;
                foreach (var instruction in result)
                {
                    if (instruction.opcode != OpCodes.Call && instruction.opcode != OpCodes.Callvirt)
                        continue;
                    if (!(instruction.operand is MethodInfo method) || !IsMoverMethod(method))
                        continue;
                    if (method.Name == "getMoveKey")
                    {
                        instruction.operand = method.GetParameters().Length == 0 ? autoMove : autoMoveFlag;
                        replacedMove++;
                    }
                    else if (method.Name == "isItmPD")
                    {
                        instruction.operand = autoItmPD;
                        replacedPd++;
                    }
                    else if (method.Name == "isItmO")
                    {
                        instruction.operand = method.GetParameters().Length == 0 ? autoItmO : autoItmOFlag;
                        replacedO++;
                    }
                }
                if (replacedMove != 1)
                    throw new InvalidOperationException(
                        $"Expected one getMoveKey call in BunPrRunner.runPreSPPR, found {replacedMove}.");
                if (replacedPd != 1 || replacedO != 1)
                    throw new InvalidOperationException(
                        $"Expected one isItmPD/isItmO call in BunPrRunner.runPreSPPR, found {replacedPd}/{replacedO}.");
                BLog.Debug($"{nameof(AutoBunServeMovePatch)} transpiler replaced input calls: getMoveKey={replacedMove}, isItmPD={replacedPd}, isItmO={replacedO}.");
                return result;
            }

            public static void Postfix(BunPrRunner __instance, PR Pr)
            {
                AutoBunServe.AfterRunnerTick(__instance, Pr);
            }

            private static bool IsMoverMethod(MethodInfo method)
            {
                if (method == null || method.DeclaringType == null)
                    return false;
                return method.DeclaringType == typeof(M2MoverPr) ||
                    method.DeclaringType == typeof(PR) ||
                    typeof(M2MoverPr).IsAssignableFrom(method.DeclaringType);
            }
        }

        /// <summary>
        /// 跟踪酒吧小游戏生命周期，换场时丢掉粘滞目标。
        /// </summary>
        [HarmonyPatch]
        public class AutoBunServeGamePatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(BUN), nameof(BUN.activate))]
            public static void ActivatePostfix(BUN __instance)
            {
                AutoBunServe.CaptureGame(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(BUN), nameof(BUN.deactivate))]
            public static void DeactivatePostfix(BUN __instance)
            {
                AutoBunServe.ReleaseGame(__instance);
            }
        }

        /// <summary>
        /// 兴奋度过高触发的钟摆 QTE：只在接受窗口内视为按下目标键，避免窗外误触失败。
        /// </summary>
        [HarmonyPatch]
        public class AutoBunServeGachaPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(AbsorbManager), nameof(AbsorbManager.isKeyPD))]
            public static bool IsKeyPDPrefix(AbsorbManager __instance, ref bool __result)
            {
                try
                {
                    if (!AutoBunServe.TryHandlePendulumKey(__instance, out bool press))
                        return true;
                    __result = press;
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AutoBunServeGachaPatch)}", ex);
                    return true;
                }
            }
        }

        /// <summary>
        /// QTE/演出会让酒保 isMoveScriptActive，走路状态机停在 PosWalk.y&gt;0，不再出酒。
        /// 冻结时直接结束走路，并在摇酒姿态卡住或长时间不出酒时把状态机推下去。
        /// </summary>
        [HarmonyPatch]
        public class AutoBunServeBartenderPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(MvNelNNEAListener_Barten), "runPosWalk")]
            public static bool RunPosWalkPrefix(MvNelNNEAListener_Barten __instance, ref bool __result)
            {
                try
                {
                    if (!AutoBunServe.ShouldSkipFrozenBartenderWalk(__instance))
                        return true;
                    AutoBunServe.FinishBartenderWalk(__instance);
                    __result = false;
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AutoBunServeBartenderPatch)}.{nameof(RunPosWalkPrefix)}", ex);
                    return true;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(MvNelNNEAListener_Barten), nameof(MvNelNNEAListener_Barten.runPre))]
            public static void RunPrePostfix(MvNelNNEAListener_Barten __instance)
            {
                try
                {
                    AutoBunServe.KickBartender(__instance);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AutoBunServeBartenderPatch)}.{nameof(RunPrePostfix)}", ex);
                }
            }
        }
    }
}
