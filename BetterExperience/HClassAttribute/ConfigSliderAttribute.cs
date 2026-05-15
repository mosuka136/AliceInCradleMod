using System;

namespace BetterExperience.HClassAttribute
{
    /// <summary>
    /// 为数值配置项声明 GUI 滑条范围。
    /// 该特性只影响配置界面的展示和输入方式，不会自动限制配置文件中已存在的值。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ConfigSliderAttribute : Attribute
    {
        /// <summary>
        /// 滑条最小显示值。
        /// </summary>
        public float Min { get; set; }
        /// <summary>
        /// 滑条最大显示值。
        /// </summary>
        public float Max { get; set; }
        /// <summary>
        /// 滑条步进；小于等于 0 时表示不做步进吸附。
        /// </summary>
        public float Step { get; set; }

        public ConfigSliderAttribute(float min, float max, float step = -1f)
        {
            Min = min;
            Max = max;
            Step = step;
        }
    }
}
