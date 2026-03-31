using System.Collections.Concurrent;
using System.ComponentModel;

namespace BetterExperience.TranslatorSpace
{
    public enum LanguageType
    {
        None,
        Default,
        [Description("简体中文")]
        Chinese,
        [Description("English")]
        English,
    }

    public static class EnumExtensions
    {
        private static readonly ConcurrentDictionary<LanguageType, string> _descriptions = new ConcurrentDictionary<LanguageType, string>();
        public static string GetDescription(this LanguageType languageType)
        {
            return _descriptions.GetOrAdd(languageType, v =>
            {
                var fieldInfo = v.GetType().GetField(v.ToString());
                var descriptionAttribute = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
                return descriptionAttribute != null && descriptionAttribute.Length > 0 ? descriptionAttribute[0].Description : v.ToString();
            });
        }
    }
}
