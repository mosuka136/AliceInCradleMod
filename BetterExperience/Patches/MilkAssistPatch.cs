using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
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
        /// 挤奶小游戏的手动辅助（非全自动），四个配置开关相互独立、持久化保存：
        /// 满级松手——玩家按住蓄力时，在进度恰好满级的那一帧自动松开，
        /// 保证满级奶量且永不“按过头”（复用自动挤奶在 UiMgmFarmSuck.run 内的输入垫片）；
        /// 放宽判定——把“按过头”宽限量(alloc_over_t，原为 2~14 帧随牛烦躁缩短)放大到固定 30 帧；
        /// 奶量不减——跳过挤奶结束时的奶量扣除(applyMpDamage)，同一头牛可反复挤
        /// （开场牛仍从 0 奶量开始，需先吃草积累；结算的奶量检查不变）；
        /// 奔跑不扰牛——玩家奔跑或跳跃靠近奶牛时不再打上 INJECTED 标记，避免打扰与触怒。
        /// 各开关均为调用时读取的纯配置判断，不持有游戏状态，加载存档或切换场景不影响。
        /// </summary>
        internal static class MilkAssist
        {
            internal static bool ReleaseAssistOn => ConfigManager.EnableMilkReleaseAssist?.Value == true;
            internal static bool WideToleranceOn => ConfigManager.EnableMilkTolerance?.Value == true;
            internal static bool NoDrainOn => ConfigManager.EnableMilkNoDrain?.Value == true;
            internal static bool NoRunAngerOn => ConfigManager.EnableMilkNoRunAnger?.Value == true;
            internal static bool NoTiredLimitOn => ConfigManager.EnableMilkNoTiredLimit?.Value == true;

            // 原版用 GFC 计数 DFMT（“牛の疲労ゲージ”，标志位 82）限制连续开局：
            // 结算按成绩 +1~+4，累计到 7 后与奶农对话被“牛累了”拒绝；平时偶发小幅自然回落。
            internal const string TiredGfcKey = "DFMT";
            private static int _tiredGfcIndex = -1;

            /// <summary>
            /// 从游戏名字表解析 DFMT 的 GFC 索引（由 __INITG 脚本的 DEFINE_GFC_NAME 登记），
            /// 不硬编码编号以适应版本变动；未登记过（-1）时不拦截，下次调用继续尝试。
            /// </summary>
            internal static int GetTiredGfcIndex()
            {
                if (_tiredGfcIndex < 0 && GF.Onamed_c != null)
                {
                    int idx;
                    if (GF.Onamed_c.TryGetValue(TiredGfcKey, out idx))
                        _tiredGfcIndex = idx;
                }
                return _tiredGfcIndex;
            }

            /// <summary>
            /// 替换 quitSuck 内的 applyMpDamage 调用（静态方法首参为实例，栈布局与 callvirt 一致）。
            /// 原方法返回扣除后的奶量，调用处不使用返回值，这里原样透传返回值。
            /// </summary>
            internal static int ApplyDrainAfterSuck(NelNMgmFarmAnimal cow, int val, bool force, AttackInfo atk)
            {
                if (NoDrainOn)
                {
                    BLog.Debug($"{nameof(MilkAssist)} skipped milk drain for cow {cow.cow_index}.");
                    return (int)cow.get_mp();
                }
                return cow.applyMpDamage(val, force, atk);
            }

            /// <summary>
            /// 替换 runPre 内对奔跑/跳跃打扰标记的 AddF 调用；关闭辅助时保持原行为。
            /// 参数必须与调用点入栈一致（实例 + FLAG + float），否则栈不平衡会产生非法 IL。
            /// </summary>
            internal static NAI AddInjectedFlag(NAI nai, NAI.FLAG flag, float maxt)
            {
                if (NoRunAngerOn)
                    return nai;
                return nai.AddF(flag, maxt);
            }
        }

        /// <summary>
        /// 放宽挤奶“按过头”判定：宽限量不足 30 帧时抬高到 30 帧。
        /// alloc_over_t 只被蓄力结算的失败阈值与空奶提示音使用，不影响其他系统。
        /// </summary>
        [HarmonyPatch]
        public class MilkTolerancePatch
        {
            // alloc_over_t 是表达式体属性，需显式指定 Getter 才能解析到目标方法。
            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiMgmFarmSuck), nameof(UiMgmFarmSuck.alloc_over_t), MethodType.Getter)]
            public static void AllocOverTPostfix(ref float __result)
            {
                if (MilkAssist.WideToleranceOn)
                    __result = Math.Max(__result, 30f);
            }
        }

        /// <summary>
        /// 挤奶奶量不减：把 quitSuck 中唯一的 applyMpDamage 调用替换为可运行时开关的垫片。
        /// 调用点数量与预期不符时抛出异常，由插件入口跳过本补丁类，不影响其他补丁。
        /// </summary>
        [HarmonyPatch]
        public class MilkNoDrainPatch
        {
            // quitSuck 为游戏 internal 方法，无法 nameof，用字符串指定。
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(NelNMgmFarmAnimal), "quitSuck")]
            public static IEnumerable<CodeInstruction> ReplaceDrainCall(IEnumerable<CodeInstruction> instructions)
            {
                var shim = typeof(MilkAssist).GetMethod(nameof(MilkAssist.ApplyDrainAfterSuck),
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                // 按方法名匹配：virtual 调用的操作数可能绑定到基类声明而非本类重写。
                var result = instructions.Select(instruction => new CodeInstruction(instruction)).ToList();
                int replaced = 0;
                foreach (var instruction in result)
                {
                    if (instruction.opcode != OpCodes.Callvirt ||
                        !(instruction.operand is MethodInfo method) || method.Name != "applyMpDamage")
                        continue;
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = shim;
                    replaced++;
                }
                if (replaced != 1)
                    throw new InvalidOperationException(
                        $"Expected one applyMpDamage call in quitSuck, found {replaced}.");
                BLog.Debug($"{nameof(MilkNoDrainPatch)} transpiler replaced drain call.");
                return result;
            }
        }

        /// <summary>
        /// 奔跑不扰牛：把 runPre 中唯一的 INJECTED 标记 AddF 调用替换为可运行时开关的垫片。
        /// </summary>
        [HarmonyPatch]
        public class MilkNoRunAngerPatch
        {
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(NelNMgmFarmAnimal), nameof(NelNMgmFarmAnimal.runPre))]
            public static IEnumerable<CodeInstruction> ReplaceInjectedFlagCall(IEnumerable<CodeInstruction> instructions)
            {
                var shim = typeof(MilkAssist).GetMethod(nameof(MilkAssist.AddInjectedFlag),
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                // 按方法名匹配 NAI::AddF（runPre 内仅 INJECTED 一处调用）。
                var result = instructions.Select(instruction => new CodeInstruction(instruction)).ToList();
                int replaced = 0;
                foreach (var instruction in result)
                {
                    if (instruction.opcode != OpCodes.Callvirt ||
                        !(instruction.operand is MethodInfo method) || method.Name != nameof(NAI.AddF))
                        continue;
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = shim;
                    replaced++;
                }
                if (replaced != 1)
                    throw new InvalidOperationException(
                        $"Expected one NAI.AddF call in {nameof(NelNMgmFarmAnimal.runPre)}, found {replaced}.");
                BLog.Debug($"{nameof(MilkNoRunAngerPatch)} transpiler replaced injected-flag call.");
                return result;
            }
        }

        /// <summary>
        /// 解除牛疲劳开局限制：读写两端同时拦截 GFC 计数 DFMT。
        /// 事件表达式 GFC[DFMT] 经名字表解析出索引后调用 <see cref="GF.getC(int)"/>，
        /// 读取端把该索引恒返回 0，已累计到上限的存档也立即解除封锁；
        /// 写入端（<see cref="GF.commandGfcSet"/>）把对 DFMT 的任何赋值改写为 0，
        /// 避免关闭本配置后残留远超原版的累计值。
        /// </summary>
        [HarmonyPatch]
        public class MilkNoTiredLimitPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(GF), nameof(GF.getC), new[] { typeof(int) })]
            public static bool GetCIntPrefix(int i, ref uint __result)
            {
                if (!MilkAssist.NoTiredLimitOn || i != MilkAssist.GetTiredGfcIndex())
                    return true;
                __result = 0u;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(GF), nameof(GF.getC), new[] { typeof(string) })]
            public static bool GetCStringPrefix(string key, ref uint __result)
            {
                if (!MilkAssist.NoTiredLimitOn || key != MilkAssist.TiredGfcKey)
                    return true;
                __result = 0u;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(GF), nameof(GF.commandGfcSet))]
            public static bool CommandGfcSetPrefix(string key, ref string val)
            {
                if (!MilkAssist.NoTiredLimitOn || key != MilkAssist.TiredGfcKey)
                    return true;
                // 改写为绝对赋值 0（commandGfcSet 对纯数字按绝对值处理）。
                val = "0";
                return true;
            }
        }
    }
}
