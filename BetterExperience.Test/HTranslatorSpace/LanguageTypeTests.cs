using BetterExperience.HEnumHelper;
using BetterExperience.HTranslatorSpace;
using System.ComponentModel;

namespace BetterExperience.Test.HTranslatorSpace
{
    public class LanguageTypeTests
    {
        [Theory]
        [InlineData(LanguageType.None)]
        [InlineData(LanguageType.Default)]
        public void HiddenLanguageControlValues_HaveDisplayEnumFalse(LanguageType languageType)
        {
            // Act
            var attribute = EnumHelper.GetAttribute<LanguageType, DisplayEnumAttribute>(languageType);

            // Assert
            Assert.NotNull(attribute);
            Assert.False(attribute.IsDisplay);
        }

        [Theory]
        [InlineData(LanguageType.Chinese, "简体中文")]
        [InlineData(LanguageType.English, "English")]
        public void VisibleLanguageValues_HaveExpectedDescriptions(LanguageType languageType, string expectedDescription)
        {
            // Act
            var description = EnumHelper.GetAttribute<LanguageType, DescriptionAttribute>(languageType);
            var display = EnumHelper.GetAttribute<LanguageType, DisplayEnumAttribute>(languageType);

            // Assert
            Assert.NotNull(description);
            Assert.Equal(expectedDescription, description.Description);
            Assert.Null(display);
        }
    }
}
