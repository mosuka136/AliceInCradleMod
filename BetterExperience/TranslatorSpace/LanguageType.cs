using BetterExperience.HEnumHelper;
using System.ComponentModel;

namespace BetterExperience.TranslatorSpace
{
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
