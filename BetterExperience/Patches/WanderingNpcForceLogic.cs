using System.ComponentModel;
using nel;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 可传唤的流浪商人种类，枚举值与 <see cref="WanderingManager.TYPE"/> 一一对应。
    /// 控制页下拉条目与测试方法都需要公开此枚举。
    /// </summary>
    public enum SummonWanderingNpcKind
    {
        [Description("南丁格尔")]
        Nightingale = 0,
        [Description("咖啡师")]
        CoffeeMaker = 1,
        [Description("提尔德")]
        Tilde = 2,
        [Description("木偶商人")]
        Puppet = 3
    }

    /// <summary>
    /// 流浪商人传唤的映射规则，不访问游戏对象。
    /// </summary>
    internal static class WanderingNpcForceLogic
    {
        /// <summary>
        /// 把控制页的选择转换为游戏的流浪商人类型；越界值回退为夜莺。
        /// </summary>
        internal static WanderingManager.TYPE ToGameType(SummonWanderingNpcKind kind)
        {
            int value = (int)kind;
            if (value < 0 || value >= (int)WanderingManager.TYPE._MAX)
                return WanderingManager.TYPE.NIG;

            return (WanderingManager.TYPE)value;
        }

        /// <summary>
        /// 世界未就绪（未进入游戏或未读档）时不执行传唤。
        /// </summary>
        internal static bool ShouldSummon(bool worldReady)
        {
            return worldReady;
        }
    }
}
