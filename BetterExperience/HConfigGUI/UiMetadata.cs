using System;

namespace BetterExperience.HConfigGUI
{
    /// <summary>
    /// 配置项 GUI 元数据接口。
    /// 元数据只影响控件选择和展示方式，不改变配置值的文件格式。
    /// </summary>
    public interface IUiMetadata
    {
        Type MetadataType { get; }
    }

    /// <summary>
    /// 数值配置项的滑条元数据。
    /// </summary>
    public class UiSliderMetadata : IUiMetadata
    {
        public Type MetadataType => typeof(UiSliderMetadata);
        /// <summary>
        /// 滑条最小显示值。
        /// </summary>
        public float Min { get; }
        /// <summary>
        /// 滑条最大显示值。
        /// </summary>
        public float Max { get; }
        /// <summary>
        /// 滑条步进；小于等于 0 时不吸附。
        /// </summary>
        public float Step { get; }
        public UiSliderMetadata(float min, float max, float step)
        {
            Min = min;
            Max = max;
            Step = step;
        }
    }
}
