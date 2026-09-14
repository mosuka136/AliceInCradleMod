using BetterExperience.Patches;
using System;
using System.Collections.Generic;

namespace BetterExperience.Test.Patches
{
    public class DebugGiveLogicTests
    {
        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("  ", "")]
        [InlineData(" mtr_lily_bulb0 ", "mtr_lily_bulb0")]
        public void NormalizeKey_TrimsOrEmpties(string raw, string expected)
        {
            Assert.Equal(expected, DebugGiveLogic.NormalizeKey(raw));
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, 0)]
        [InlineData(4, 4)]
        [InlineData(5, 4)]
        public void ClampGrade_LimitsToZeroThroughFour(int grade, int expected)
        {
            Assert.Equal(expected, DebugGiveLogic.ClampGrade(grade));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(99, 99)]
        [InlineData(100, 99)]
        public void ClampCount_LimitsToOneThroughNinetyNine(int count, int expected)
        {
            Assert.Equal(expected, DebugGiveLogic.ClampCount(count));
        }

        [Theory]
        [InlineData("burst", "burst")]
        [InlineData("skillbook_burst", "burst")]
        [InlineData(" skillbook_burst ", "burst")]
        public void SkillKeyFromInput_StripsSkillbookPrefix(string input, string expected)
        {
            Assert.Equal(expected, DebugGiveLogic.SkillKeyFromInput(input));
        }

        [Theory]
        [InlineData("soup", "soup")]
        [InlineData("Recipe_soup", "soup")]
        [InlineData(" Recipe_soup ", "soup")]
        public void RecipeKeyFromInput_StripsRecipePrefix(string input, string expected)
        {
            Assert.Equal(expected, DebugGiveLogic.RecipeKeyFromInput(input));
        }

        [Theory]
        [InlineData(true, 3, 0)]
        [InlineData(false, 3, 3)]
        [InlineData(false, 9, 4)]
        public void ResolveItemGrade_ForcesZeroForIndividualGrade(bool individualGrade, int requested, int expected)
        {
            Assert.Equal(expected, DebugGiveLogic.ResolveItemGrade(individualGrade, requested));
        }

        [Fact]
        public void UniqueDisplay_AppendsKeyWhenNameRepeats()
        {
            var used = new HashSet<string>(StringComparer.Ordinal);
            Assert.Equal("Herb", DebugGiveLogic.UniqueDisplay("Herb", "mtr_a", used));
            Assert.Equal("Herb [mtr_b]", DebugGiveLogic.UniqueDisplay("Herb", "mtr_b", used));
        }

        [Theory]
        [InlineData("Herb", "mtr_a", "", true)]
        [InlineData("Herb", "mtr_a", "he", true)]
        [InlineData("Herb", "mtr_a", "mtr", true)]
        [InlineData("Herb", "mtr_a", "zzz", false)]
        public void MatchesFilter_ChecksDisplayAndKey(string display, string key, string filter, bool expected)
        {
            Assert.Equal(expected, DebugGiveLogic.MatchesFilter(display, key, filter));
        }

        [Fact]
        public void SelectedKeyFromChoices_SwitchesToNewlyTickedEntry()
        {
            var catalog = new List<(string Key, string Display)>
            {
                ("a", "Alpha"),
                ("b", "Beta")
            };
            var both = new List<(string Display, bool Selected)>
            {
                ("Alpha", true),
                ("Beta", true)
            };
            Assert.Equal("b", DebugGiveLogic.SelectedKeyFromChoices(both, catalog, "a"));

            var onlyNew = new List<(string Display, bool Selected)>
            {
                ("Alpha", false),
                ("Beta", true)
            };
            Assert.Equal("b", DebugGiveLogic.SelectedKeyFromChoices(onlyNew, catalog, "a"));
        }

        [Fact]
        public void SelectedKeyFromChoices_ClearsWhenNoneTicked()
        {
            var catalog = new List<(string Key, string Display)> { ("a", "Alpha") };
            var none = new List<(string Display, bool Selected)> { ("Alpha", false) };
            Assert.Equal("", DebugGiveLogic.SelectedKeyFromChoices(none, catalog, "a"));
        }

        [Fact]
        public void SelectedKeyFromChoices_KeepsPreviousWhenStillTickedOrCountMismatch()
        {
            var catalog = new List<(string Key, string Display)> { ("a", "Alpha") };
            var stillTicked = new List<(string Display, bool Selected)> { ("Alpha", true) };
            Assert.Equal("a", DebugGiveLogic.SelectedKeyFromChoices(stillTicked, catalog, "a"));
            Assert.Equal("prev", DebugGiveLogic.SelectedKeyFromChoices(new List<(string, bool)>(), catalog, "prev"));
        }
    }
}
