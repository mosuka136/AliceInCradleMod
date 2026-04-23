using BetterExperience.HTranslatorSpace;
using System.Collections;

namespace BetterExperience.Test.HTranslatorSpace
{
    public class TranslatorTests
    {
        // -----------------------------------------------------------------------
        // Constructor - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_WithDefaultParameters_InitializesEmptyStrings()
        {
            // Arrange & Act
            var translator = new Translator();

            // Assert
            Assert.Equal("", translator.Chinese);
            Assert.Equal("", translator.English);
        }

        [Fact]
        public void Constructor_WithChineseAndEnglish_InitializesProperties()
        {
            // Arrange
            var chinese = "中文";
            var english = "English";

            // Act
            var translator = new Translator(chinese, english);

            // Assert
            Assert.Equal(chinese, translator.Chinese);
            Assert.Equal(english, translator.English);
        }

        [Fact]
        public void Constructor_WithNullValues_InitializesWithNull()
        {
            // Arrange & Act
            var translator = new Translator(null, null);

            // Assert
            Assert.Null(translator.Chinese);
            Assert.Null(translator.English);
        }

        [Fact]
        public void Constructor_WithMixedValues_InitializesCorrectly()
        {
            // Arrange
            var chinese = "测试";

            // Act
            var translator = new Translator(chinese);

            // Assert
            Assert.Equal(chinese, translator.Chinese);
            Assert.Equal("", translator.English);
        }

        // -----------------------------------------------------------------------
        // Default Property - LanguageType tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Default_WithChineseLanguageType_ReturnsChineseText()
        {
            // Arrange
            var chinese = "中文文本";
            var english = "English text";
            var translator = new Translator(chinese, english)
            {
                LanguageType = LanguageType.Chinese
            };

            // Act
            var result = translator.Default;

            // Assert
            Assert.Equal(chinese, result);
        }

        [Fact]
        public void Default_WithEnglishLanguageType_ReturnsEnglishText()
        {
            // Arrange
            var chinese = "中文文本";
            var english = "English text";
            var translator = new Translator(chinese, english)
            {
                LanguageType = LanguageType.English
            };

            // Act
            var result = translator.Default;

            // Assert
            Assert.Equal(english, result);
        }

        [Fact]
        public void Default_WithDefaultLanguageTypeAndEnglishDefault_ReturnsEnglishText()
        {
            // Arrange
            var chinese = "中文文本";
            var english = "English text";
            var originalDefault = Translator.DefaultLanguage;
            Translator.DefaultLanguage = LanguageType.English;
            var translator = new Translator(chinese, english)
            {
                LanguageType = LanguageType.Default
            };

            // Act
            var result = translator.Default;

            // Assert
            Assert.Equal(english, result);
            
            // Cleanup
            Translator.DefaultLanguage = originalDefault;
        }

        [Fact]
        public void Default_WithDefaultLanguageTypeAndChineseDefault_ReturnsChineseText()
        {
            // Arrange
            var chinese = "中文文本";
            var english = "English text";
            var originalDefault = Translator.DefaultLanguage;
            Translator.DefaultLanguage = LanguageType.Chinese;
            var translator = new Translator(chinese, english)
            {
                LanguageType = LanguageType.Default
            };

            // Act
            var result = translator.Default;

            // Assert
            Assert.Equal(chinese, result);
            
            // Cleanup
            Translator.DefaultLanguage = originalDefault;
        }

        [Fact]
        public void Default_WithNoneLanguageType_ReturnsEnglishText()
        {
            // Arrange
            var chinese = "中文文本";
            var english = "English text";
            var translator = new Translator(chinese, english)
            {
                LanguageType = LanguageType.None
            };

            // Act
            var result = translator.Default;

            // Assert
            Assert.Equal(english, result);
        }

        [Fact]
        public void Default_WithDefaultLanguageTypeAndNoneDefault_ReturnsEnglishText()
        {
            // Arrange
            var chinese = "中文文本";
            var english = "English text";
            var originalDefault = Translator.DefaultLanguage;
            Translator.DefaultLanguage = LanguageType.None;
            var translator = new Translator(chinese, english)
            {
                LanguageType = LanguageType.Default
            };

            // Act
            var result = translator.Default;

            // Assert
            Assert.Equal(english, result);
            
            // Cleanup
            Translator.DefaultLanguage = originalDefault;
        }

        [Fact]
        public void Default_WithDefaultLanguageTypeAndDefaultDefault_ReturnsEnglishText()
        {
            // Arrange
            var chinese = "中文文本";
            var english = "English text";
            var originalDefault = Translator.DefaultLanguage;
            Translator.DefaultLanguage = LanguageType.Default;
            var translator = new Translator(chinese, english)
            {
                LanguageType = LanguageType.Default
            };

            // Act
            var result = translator.Default;

            // Assert
            Assert.Equal(english, result);
            
            // Cleanup
            Translator.DefaultLanguage = originalDefault;
        }

        // -----------------------------------------------------------------------
        // GetEnumerator - IEnumerable tests
        // -----------------------------------------------------------------------

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsChineseAndEnglish()
        {
            // Arrange
            var chinese = "中文";
            var english = "English";
            var translator = new Translator(chinese, english);
            IEnumerable enumerable = translator;

            // Act
            var enumerator = enumerable.GetEnumerator();
            var results = new System.Collections.Generic.List<string>();
            while (enumerator.MoveNext())
            {
                results.Add(enumerator.Current as string);
            }

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Equal(chinese, results[0]);
            Assert.Equal(english, results[1]);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_WithNullValues_ReturnsNulls()
        {
            // Arrange
            var translator = new Translator(null, null);
            IEnumerable enumerable = translator;

            // Act
            var enumerator = enumerable.GetEnumerator();
            var results = new System.Collections.Generic.List<string>();
            while (enumerator.MoveNext())
            {
                results.Add(enumerator.Current as string);
            }

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Null(results[0]);
            Assert.Null(results[1]);
        }

        [Fact]
        public void GetEnumerator_Generic_ReturnsChineseAndEnglish()
        {
            // Arrange
            var chinese = "中文";
            var english = "English";
            var translator = new Translator(chinese, english);

            // Act
            var results = translator.ToList();

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Equal(chinese, results[0]);
            Assert.Equal(english, results[1]);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_WithEmptyStrings_ReturnsEmptyStrings()
        {
            // Arrange
            var translator = new Translator("", "");
            IEnumerable enumerable = translator;

            // Act
            var enumerator = enumerable.GetEnumerator();
            var results = new System.Collections.Generic.List<string>();
            while (enumerator.MoveNext())
            {
                results.Add(enumerator.Current as string);
            }

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Equal("", results[0]);
            Assert.Equal("", results[1]);
        }
    }
}
