using nel;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 污渍与浸湿拦截规则。不触碰 Unity 或角色实例。
    /// 冻结/石化/蛛网是异常贴图，不算普通污渍。
    /// </summary>
    internal static class DirtWetLogic
    {
        internal static bool ShouldSkipDirt(bool noDirt, BetoInfo.TYPE type)
        {
            return noDirt && !BetobetoManager.is_special_ser_type(type);
        }

        internal static bool ShouldSkipWetten(bool noWet, bool becomingWet)
        {
            return noWet && becomingWet;
        }
    }
}
