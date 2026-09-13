using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using BetterExperience.BPatchGUI;
using evt;
using HarmonyLib;
using m2d;
using nel;
using nel.mgm.farm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 挤奶小游戏（MGFarm 奶牛挤奶）全自动代打。
        /// 游戏流程：与奶牛对话触发事件 → 每轮 3 槽“按下-按住蓄力-松开结算” → 结束回到场地换下一头。
        /// 奶量与蓄力进度(pushdown_level)成正比，进度在 Ef.z == pushdown_maxt + 10 时到达 1.0，
        /// 按住超过 pushdown_maxt + 10 + alloc_over_t(至少 2 帧) 判“按过头”失败并触怒奶牛；
        /// 结算还要求奶牛奶量 mp_ratio 大于 0.25。
        /// 本类分两部分：
        /// 输入垫片——替换 UiMgmFarmSuck.run 内的 IN.isBP/IN.isBO 两处调用，自动按下并按住，
        /// 蓄力进度第一次到达 1.0 的那一帧松开，得到满级奶量且不触怒奶牛（替换只作用于该方法内部）；
        /// 场地驱动——每帧选择奶量最高的奶牛，把玩家滑行带到牛旁后调用
        /// <see cref="M2EventItem.execute"/> 触发对话事件，之后由游戏事件脚本完成对齐与挤奶，循环到计时结束。
        /// </summary>
        internal static class AutoMilk
        {
            internal static readonly object StatusKey = new object();

            private static readonly FieldInfo EfField = AccessTools.Field(typeof(UiMgmFarmSuck), "Ef");
            private static readonly FieldInfo CowsField = AccessTools.Field(typeof(M2LpMgmFarm), "AEnCow");

            // 开关状态。
            private static bool _enabled;
            private static bool _statusShown;
            private static bool _loggedActivation;

            // QTE 虚拟按键状态；仅由游戏主线程经 initSuck/quitSuck/run 访问。
            private static UiMgmFarmSuck _activeUi;
            private static bool _holding;

            // 反射或状态异常时置位，后续帧直接透传原始输入，避免每帧重复抛错刷日志；
            // 下一次 initSuck 会清零重试。
            private static bool _broken;

            // 场地驱动状态。
            private static M2LpMgmFarm _farm;
            private static NelNMgmFarmAnimal _target;
            private static float _retryCooldown;

            // 奶量低于结算失败线(0.25)的牛不挤；留少量余量避免边界失败。
            private const float MilkMpRatioMin = 0.3f;
            // 距奶牛中心的停止距离（地图单位）；事件脚本会把玩家对齐到最终位置。
            private const float ApproachStopDist = 1.5f;
            // 每帧滑行速度（地图单位/帧），与玩家奔跑速度相当。
            private const float GlideSpeed = 0.32f;
            // 触发对话后的冷却帧：既等待事件结束，也避免对话被拒（牛忙）时连续刷事件。
            private const float TriggerCooldown = 60f;

            internal static bool GetAutoMilk() => _enabled;

            internal static void SetAutoMilk(bool enabled)
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
                        enabled ? TranslatorResource.AutoMilkEnabled : TranslatorResource.AutoMilkDisabled,
                        owner: StatusKey);
                    SyncStatus();
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetAutoMilk)}", ex);
                }
            }

            internal static bool IsEnabled()
            {
                return ConfigManager.EnableBetterExperience?.Value == true && _enabled;
            }

            internal static void ResetDriver()
            {
                _farm = null;
                _target = null;
                _retryCooldown = 0f;
                _activeUi = null;
                _holding = false;
                _broken = false;
            }

            // ===== QTE 输入垫片 =====

            internal static void CaptureUi(UiMgmFarmSuck ui)
            {
                // initSuck 每一轮槽位都会被事件脚本调用（Ef.time 递增），此处同步重置虚拟按键。
                _activeUi = ui;
                _holding = false;
                _broken = false;
            }

            internal static void ReleaseUi()
            {
                _activeUi = null;
                _holding = false;
            }

            internal static bool AutoIsBP(int press_max)
            {
                try
                {
                    if (!_broken && IsEnabled() && _activeUi != null && _activeUi.isActive())
                    {
                        var ef = (EffectItem)EfField.GetValue(_activeUi);
                        // 与原调用点相同的前置：开场动画结束(af >= 8)、尚未下压(z <= 0)、本槽未结算。
                        if (ef != null && ef.z <= 0f && ef.af >= 8f && !_activeUi.decided_this_phase)
                        {
                            _holding = true;
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AutoMilk)}.{nameof(AutoIsBP)}", ex);
                    _holding = false;
                    _broken = true;
                }
                return IN.isBP(press_max);
            }

            internal static bool AutoIsBO(int press)
            {
                try
                {
                    if (!_broken && IsEnabled() && _holding && _activeUi != null && _activeUi.isActive())
                    {
                        var ef = (EffectItem)EfField.GetValue(_activeUi);
                        if (ef == null || _activeUi.decided_this_phase)
                        {
                            // 本槽已结算或效果对象已失效，停止介入。
                            _holding = false;
                        }
                        else if (ef.z >= _activeUi.pushdown_maxt + 10f)
                        {
                            // 蓄力进度恰好到达 1.0 时松开；距“按过头”失败阈值仍余 alloc_over_t(>= 2 帧)。
                            BLog.Debug($"{nameof(AutoMilk)} releasing slot {ef.time} at pushdown_level 1.0 (z={ef.z}, maxt={_activeUi.pushdown_maxt}).");
                            _holding = false;
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AutoMilk)}.{nameof(AutoIsBO)}", ex);
                    _holding = false;
                    _broken = true;
                }
                return IN.isBO(press);
            }

            // ===== 场地驱动 =====

            internal static void TickFarm(M2LpMgmFarm farm, float fcnt)
            {
                if (!IsEnabled())
                {
                    if (_farm != null)
                        ResetDriver();
                    return;
                }
                if (_farm != farm)
                {
                    // 换图或重新进入小游戏后农场实例会变化，丢弃旧目标。
                    _farm = farm;
                    _target = null;
                }
                if (!farm.isMainGame())
                {
                    _target = null;
                    return;
                }

                // 挤奶会话或任意事件执行中：等待（QTE 由输入垫片代打）。
                if (farm.suck_target >= 0 || EV.isActive())
                {
                    _target = null;
                    return;
                }

                var map = farm.Ui.Mp;
                var pr = map?.getKeyPr() as PR;
                if (map == null || pr == null || pr.Mp != map || map.M2D == null || map.M2D.curMap != map ||
                    !pr.is_alive || !pr.isNormalState() || pr.isMoveScriptActive())
                {
                    _target = null;
                    return;
                }

                if (_retryCooldown > 0f)
                {
                    _retryCooldown -= Math.Max(fcnt, 0f);
                    return;
                }

                var cows = CowsField?.GetValue(farm) as NelNMgmFarmAnimal[];
                if (cows == null)
                    return;
                if (_target != null && !IsValidCow(_target, cows))
                    _target = null;
                if (_target == null)
                    _target = PickCow(cows, pr);
                if (_target == null)
                    return;

                float destX = _target.x + Math.Sign(pr.x - _target.x) * ApproachStopDist;
                float dx = destX - pr.x;
                if (Math.Abs(dx) > 0.1f)
                {
                    Glide(pr, dx, fcnt);
                    return;
                }

                // 到位：面向奶牛并触发对话事件；事件脚本自带玩家对齐、挤奶流程与结算。
                pr.setAim(_target.x >= pr.x ? AIM.R : AIM.L, false);
                LogActivationOnce();
                BLog.Debug($"{nameof(AutoMilk)} triggering talk with cow {_target.cow_index} (mp_ratio={_target.mp_ratio:0.00}).");
                _target.AttachEvent.execute(M2EventItem.CMD.TALK, pr);
                _target = null;
                _retryCooldown = TriggerCooldown;
            }

            private static bool IsValidCow(NelNMgmFarmAnimal cow, NelNMgmFarmAnimal[] cows)
            {
                // 目标仍在牛列表且奶量未跌破下限时保持不变，避免来回切换目标。
                return cows.Contains(cow) && cow.get_mp() > 0 && cow.mp_ratio >= MilkMpRatioMin * 0.9f;
            }

            private static NelNMgmFarmAnimal PickCow(NelNMgmFarmAnimal[] cows, PR pr)
            {
                // 奶量优先（奶量决定单次收益），并列时取最近；低奶量的牛留给它们吃草恢复。
                NelNMgmFarmAnimal best = null;
                float bestMp = float.MinValue;
                float bestDist = float.MaxValue;
                foreach (var cow in cows)
                {
                    if (cow == null || cow.get_mp() <= 0 || cow.mp_ratio < MilkMpRatioMin)
                        continue;
                    float dist = Math.Abs(cow.x - pr.x);
                    if (cow.mp_ratio > bestMp || (cow.mp_ratio == bestMp && dist < bestDist))
                    {
                        best = cow;
                        bestMp = cow.mp_ratio;
                        bestDist = dist;
                    }
                }
                return best;
            }

            private static void Glide(PR pr, float dx, float fcnt)
            {
                // 与穿墙一致的清速+直移地滑：每帧清掉输入速度，按固定步长平移。
                pr.jumpRaisingQuit(true);
                pr.getFootManager()?.initJump(false, true, false);
                var physics = pr.getPhysic();
                physics.walk_xspeed = 0f;
                physics.killSpeedForce(kill_phy_translate_stack: true);
                float step = Math.Min(Math.Abs(dx), GlideSpeed * Math.Max(fcnt, 1f));
                pr.setTo(pr.x + Math.Sign(dx) * step, pr.y);
            }

            private static void LogActivationOnce()
            {
                if (_loggedActivation)
                    return;
                BLog.Debug($"{nameof(AutoMilk)} applied.");
                _loggedActivation = true;
            }

            private static void SyncStatus()
            {
                if (IsEnabled())
                {
                    if (_statusShown)
                        return;
                    NoticeGUI.SetStatus(StatusKey, TranslatorResource.AutoMilkActive);
                    _statusShown = true;
                }
                else
                {
                    HideStatus();
                }
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
        /// 挤奶 QTE 输入代打：把 UiMgmFarmSuck.run 内的按键检测替换为自动挤奶垫片，
        /// 并跟踪当前挤奶 UI 的生命周期。
        /// </summary>
        [HarmonyPatch]
        public class AutoMilkInputPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiMgmFarmSuck), nameof(UiMgmFarmSuck.initSuck))]
            public static void InitSuckPostfix(UiMgmFarmSuck __instance)
            {
                AutoMilk.CaptureUi(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiMgmFarmSuck), nameof(UiMgmFarmSuck.quitSuck))]
            public static void QuitSuckPostfix()
            {
                AutoMilk.ReleaseUi();
            }

            // 按调用点的操作数精确匹配两处输入调用；数量与预期不符时抛出异常，
            // 由插件入口跳过本补丁类，不影响其他补丁。
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(UiMgmFarmSuck), nameof(UiMgmFarmSuck.run))]
            public static IEnumerable<CodeInstruction> ReplaceInputCalls(IEnumerable<CodeInstruction> instructions)
            {
                var isBP = AccessTools.Method(typeof(IN), nameof(IN.isBP));
                var isBO = AccessTools.Method(typeof(IN), nameof(IN.isBO));
                var autoIsBP = typeof(AutoMilk).GetMethod(nameof(AutoMilk.AutoIsBP), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                var autoIsBO = typeof(AutoMilk).GetMethod(nameof(AutoMilk.AutoIsBO), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                var result = instructions.Select(instruction => new CodeInstruction(instruction)).ToList();
                int replacedBP = 0;
                int replacedBO = 0;
                foreach (var instruction in result)
                {
                    if (instruction.opcode != OpCodes.Call)
                        continue;
                    if (Equals(instruction.operand, isBP))
                    {
                        instruction.operand = autoIsBP;
                        replacedBP++;
                    }
                    else if (Equals(instruction.operand, isBO))
                    {
                        instruction.operand = autoIsBO;
                        replacedBO++;
                    }
                }
                if (replacedBP != 1 || replacedBO != 1)
                    throw new InvalidOperationException(
                        $"Expected one isBP/isBO call in {nameof(UiMgmFarmSuck.run)}, found {replacedBP}/{replacedBO}.");
                BLog.Debug($"{nameof(AutoMilkInputPatch)} transpiler replaced input calls: isBP={replacedBP}, isBO={replacedBO}.");
                return result;
            }
        }

        /// <summary>
        /// 挤奶小游戏场地驱动：挂在农场层 runner 的帧更新上，
        /// 自动选牛、接近并触发对话，循环到计时结束。
        /// </summary>
        [HarmonyPatch]
        public class AutoMilkDriverPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(M2LpMgmFarm), nameof(M2LpMgmFarm.run))]
            public static void RunPostfix(M2LpMgmFarm __instance, float fcnt)
            {
                try
                {
                    AutoMilk.TickFarm(__instance, fcnt);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AutoMilkDriverPatch)}", ex);
                    AutoMilk.ResetDriver();
                }
            }
        }
    }
}
