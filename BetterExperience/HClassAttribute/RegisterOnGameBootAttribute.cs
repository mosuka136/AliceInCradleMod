using System;

namespace BetterExperience.HClassAttribute
{
    /// <summary>
    /// 标记一个应在游戏首次场景加载完成后创建的 Unity 组件类型。
    /// 被标记类型必须继承 <see cref="UnityEngine.Component"/>，否则注册阶段只会记录警告。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RegisterOnGameBootAttribute : Attribute
    {
    }
}
