using nel.mgm.dojo;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 道场节奏猜拳的判定规则。不触碰 Unity 或 DjGM 实例。
    /// 原版 <see cref="DjGM.hitHand"/>：t_input_alloc ≥ 14 过快、&lt; -14 过慢；
    /// 猜拳胜负由 <see cref="DjRPC.checkWin"/> 判定（0 平、1 胜、-1 负）。
    /// </summary>
    internal static class DojoAssistLogic
    {
        internal const float VanillaFastWindow = DjGM.hand_fast_time;
        internal const float VanillaSlowWindow = DjGM.hand_slow_time;
        internal const float WideWindowMul = 3f;
        internal const float AppearOrLoseThreshold = 1000f;
        internal const uint HandNextCalc = DjGM.HAND__NEXT_CALC;
        internal const uint HandBeatBits = DjGM.HAND__BEAT_BITS;

        internal static float WideFastWindow => VanillaFastWindow * WideWindowMul;
        internal static float WideSlowWindow => VanillaSlowWindow * WideWindowMul;

        /// <summary>
        /// 克制手：布克石头、石头克剪刀、剪刀克布。
        /// </summary>
        internal static int WinningHand(int enemyHand)
        {
            int hand = ((enemyHand % 3) + 3) % 3;
            return (hand + 2) % 3;
        }

        internal static int ResolvePlayerHand(int pressed, int enemyHand, bool correctHand)
        {
            return correctHand ? WinningHand(enemyHand) : pressed;
        }

        /// <summary>
        /// 出现动画（t≥1000）和失败倒计时不放宽。宽窗口内的快/慢边缘夹进原版窗口，
        /// 这样 hitHand 不会打上 FAST/SLOW 位。
        /// </summary>
        internal static float AdjustTiming(float tInputAlloc, bool wider)
        {
            if (!wider || tInputAlloc >= AppearOrLoseThreshold)
                return tInputAlloc;

            if (!IsInWindow(tInputAlloc, WideFastWindow, WideSlowWindow))
                return tInputAlloc;

            if (tInputAlloc >= VanillaFastWindow)
                return VanillaFastWindow - 0.01f;
            if (tInputAlloc < -VanillaSlowWindow)
                return -VanillaSlowWindow + 0.01f;
            return tInputAlloc;
        }

        internal static bool IsInWindow(float tInputAlloc, float fastWindow, float slowWindow)
        {
            return tInputAlloc < fastWindow && tInputAlloc >= -slowWindow;
        }

        /// <summary>
        /// GO 拍（NEXT_CALC）之后、尚未出拳、且仍在原版慢窗口内，才自动出拳。
        /// 等 t≤0 避免抢在 GO 前打出 FAST。
        /// </summary>
        internal static bool ShouldAutoHit(float tInputAlloc, uint handTypeBits)
        {
            if ((handTypeBits & ~HandBeatBits) != 0)
                return false;
            if ((handTypeBits & HandNextCalc) == 0)
                return false;
            if (tInputAlloc >= AppearOrLoseThreshold)
                return false;
            return tInputAlloc <= 0f && tInputAlloc >= -VanillaSlowWindow;
        }
    }
}
