using System;

namespace BetterExperience.HEnumHelper
{
    /// <summary>
    /// 控制枚举值是否在配置界面中显示。
    /// 被隐藏的值仍可被配置文件解析，只是不作为 GUI 选项展示。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DisplayEnumAttribute : Attribute
    {
        /// <summary>
        /// 是否在枚举选择控件中显示该值。
        /// </summary>
        public bool IsDisplay { get; set; }
        public DisplayEnumAttribute(bool isDisplay = true)
        {
            IsDisplay = isDisplay;
        }
    }
}
