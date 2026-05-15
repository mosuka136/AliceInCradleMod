using System;

namespace BetterExperience.HClassAttribute
{
    /// <summary>
    /// 标记一个应在游戏首次场景加载完成后执行的一次性初始化方法。
    /// 被标记的方法需要是无参方法；当前注册流程会以静态方法方式调用。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class InitializeOnGameBootAttribute : Attribute
    {
    }
}
