using BetterExperience.HEnumHelper;
using System.ComponentModel;

namespace BetterExperience.HTranslatorSpace
{
    /// <summary>
    /// 插件 GUI 和配置注释使用的语言枚举。
    /// None 与 Default 是内部控制值，不在配置界面中作为可选语言展示。
    /// </summary>
    public enum LanguageType
    {
        [DisplayEnum(false)]
        None,
        [DisplayEnum(false)]
        Default,
        [Description("简体中文")]
        Chinese,
        [Description("English")]
        English,
    }
}
