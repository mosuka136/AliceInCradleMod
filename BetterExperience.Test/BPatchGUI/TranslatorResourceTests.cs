using BetterExperience.BPatchGUI;
using BetterExperience.HTranslatorSpace;
using nel;

namespace BetterExperience.Test.BPatchGUI
{
    public class TranslatorResourceTests
    {
        [Fact]
        public void GetEnemyName_WithKnownEnemyId_ReturnsTranslatedName()
        {
            // Arrange
            var originalDefaultLanguage = Translator.DefaultLanguage;
            Translator.DefaultLanguage = LanguageType.English;

            try
            {
                // Act
                var result = TranslatorResource.GetEnemyName(ENEMYID.SLIME_0);

                // Assert
                Assert.Equal("Slime", result);
            }
            finally
            {
                Translator.DefaultLanguage = originalDefaultLanguage;
            }
        }

        [Fact]
        public void GetEnemyName_WithUnknownEnemyId_ReturnsEnumValueString()
        {
            // Arrange
            var originalDefaultLanguage = Translator.DefaultLanguage;
            var enemyId = (ENEMYID)123456;
            Translator.DefaultLanguage = LanguageType.English;

            try
            {
                // Act
                var result = TranslatorResource.GetEnemyName(enemyId);

                // Assert
                Assert.Equal("123456", result);
            }
            finally
            {
                Translator.DefaultLanguage = originalDefaultLanguage;
            }
        }

        [Fact]
        public void GetEnemyName_WithContaminatedKnownEnemyId_AppendsContaminatedSuffix()
        {
            // Arrange
            var originalDefaultLanguage = Translator.DefaultLanguage;
            var enemyId = ENEMYID.SLIME_0 | ENEMYID._OVERDRIVE_FLAG;
            Translator.DefaultLanguage = LanguageType.English;

            try
            {
                // Act
                var result = TranslatorResource.GetEnemyName(enemyId);

                // Assert
                Assert.Equal("Slime(Contaminated)", result);
            }
            finally
            {
                Translator.DefaultLanguage = originalDefaultLanguage;
            }
        }

        [Fact]
        public void GetEnemyAttributeName_WithNormalAttributeAfterOptionalMask_ReturnsEmptyString()
        {
            // Arrange
            var attribute = ENATTR.NORMAL | ENATTR.__OPTIONAL;

            // Act
            var result = TranslatorResource.GetEnemyAttributeName(attribute);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetEnemyAttributeName_WithKnownAndUnknownAttributes_ReturnsJoinedNamesInExpectedOrder()
        {
            // Arrange
            var originalDefaultLanguage = Translator.DefaultLanguage;
            var unknownAttribute = (ENATTR)536870912;
            var attribute = ENATTR.ATK | ENATTR.FIRE | ENATTR.BIG | unknownAttribute | ENATTR.__OPTIONAL;
            Translator.DefaultLanguage = LanguageType.English;

            try
            {
                // Act
                var result = TranslatorResource.GetEnemyAttributeName(attribute);

                // Assert
                Assert.Equal("ATK Up+Fire+Giant+536870912", result);
            }
            finally
            {
                Translator.DefaultLanguage = originalDefaultLanguage;
            }
        }

        [Fact]
        public void GetEnemyDisplayName_WithAttributeNameEmpty_ReturnsEnemyNameOnly()
        {
            // Arrange
            var originalDefaultLanguage = Translator.DefaultLanguage;
            Translator.DefaultLanguage = LanguageType.English;

            try
            {
                // Act
                var result = TranslatorResource.GetEnemyDisplayName(ENEMYID.SLIME_0, ENATTR.__OPTIONAL);

                // Assert
                Assert.Equal("Slime", result);
            }
            finally
            {
                Translator.DefaultLanguage = originalDefaultLanguage;
            }
        }

        [Fact]
        public void GetEnemyDisplayName_WithAttributeNamePresent_ReturnsEnemyNameWrappedWithAttributeBrackets()
        {
            // Arrange
            var originalDefaultLanguage = Translator.DefaultLanguage;
            Translator.DefaultLanguage = LanguageType.English;

            try
            {
                // Act
                var result = TranslatorResource.GetEnemyDisplayName(ENEMYID.SLIME_0, ENATTR.ATK | ENATTR.FIRE);

                // Assert
                Assert.Equal("Slime (ATK Up+Fire)", result);
            }
            finally
            {
                Translator.DefaultLanguage = originalDefaultLanguage;
            }
        }
    }
}
