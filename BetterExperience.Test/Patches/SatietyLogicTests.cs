using BetterExperience.Patches;

namespace BetterExperience.Test.Patches
{
    public class SatietyLogicTests
    {
        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(true, true, false, false)]
        [InlineData(true, false, true, false)]
        [InlineData(true, false, false, true)]
        public void ShouldSkipProgress_BlocksFoodDrainExceptWaterAndPreview(
            bool noDrain,
            bool onlyWater,
            bool isTemporaryStomach,
            bool expected)
        {
            Assert.Equal(
                expected,
                SatietyLogic.ShouldSkipProgress(noDrain, onlyWater, isTemporaryStomach));
        }
    }
}
