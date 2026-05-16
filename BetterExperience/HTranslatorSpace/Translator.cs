using System.Collections;
using System.Collections.Generic;

namespace BetterExperience.HTranslatorSpace
{
    /// <summary>
    /// 简单的双语文本容器。
    /// 该类型用于配置文件注释和 GUI 文案；它只按当前默认语言返回中文或英文，不负责资源文件加载或运行时本地化回退链。
    /// </summary>
    public class Translator : IEnumerable<string>
    {
        /// <summary>
        /// 当实例语言设置为 <see cref="LanguageType.Default"/> 时使用的全局语言。
        /// </summary>
        public static LanguageType DefaultLanguage { get; set; } = LanguageType.English;

        /// <summary>
        /// 当前实例的语言选择。Default 表示跟随 <see cref="DefaultLanguage"/>。
        /// </summary>
        public LanguageType LanguageType { get; set; } = LanguageType.Default;
        /// <summary>
        /// 按当前语言解析后的文本。
        /// </summary>
        public string Default
        {
            get
            {
                var language = LanguageType == LanguageType.Default ? DefaultLanguage : LanguageType;
                switch (language)
                {
                    case LanguageType.Chinese:
                        return Chinese;
                    case LanguageType.English:
                        return English;
                    case LanguageType.None:
                    case LanguageType.Default:
                    default:
                        return English;
                }
            }
        }
        public string Chinese { get; set; }
        public string English { get; set; }

        public Translator(string chinese = "", string english = "")
        {
            Chinese = chinese;
            English = english;
        }

        public static implicit operator string(Translator translator)
        {
            return translator.Default;
        }

        public override string ToString()
        {
            return Default;
        }

        public IEnumerator<string> GetEnumerator()
        {
            yield return Chinese;
            yield return English;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
