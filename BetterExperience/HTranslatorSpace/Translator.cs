using System.Collections;
using System.Collections.Generic;

namespace BetterExperience.HTranslatorSpace
{
    public class Translator : IEnumerable<string>
    {
        public static LanguageType DefaultLanguage { get; set; } = LanguageType.English;

        public LanguageType LanguageType { get; set; } = LanguageType.Default;
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
