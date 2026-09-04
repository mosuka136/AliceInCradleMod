using BetterExperience.Patches;

namespace BetterExperience.Test.Patches
{
    public class BottleHolderCountOverrideTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(12)]
        [InlineData(BottleHolderCountOverride.MaxCount)]
        public void Save_UsesOriginalCount_ThenRestoresRuntimeCount(int target)
        {
            var count = 3;
            var state = new BottleHolderCountOverride(() => count, value => count = value);

            state.Apply(target);
            Assert.Equal(target, count);
            state.SuspendForSave();
            Assert.Equal(3, count);
            state.ResumeAfterSave();
            Assert.Equal(target, count);
        }

        [Theory]
        [InlineData(2, 5, 14)]
        [InlineData(-2, 1, 10)]
        [InlineData(-10, 0, 2)]
        public void RepeatedSaves_PreserveGameAcquisitionsAndConsumption(int delta, int saved, int runtime)
        {
            var count = 3;
            var state = new BottleHolderCountOverride(() => count, value => count = value);
            state.Apply(12);
            count += delta;

            for (var i = 0; i < 2; i++)
            {
                state.SuspendForSave();
                Assert.Equal(saved, count);
                state.ResumeAfterSave();
                Assert.Equal(runtime, count);
            }
        }

        [Fact]
        public void Reapplying_DoesNotTreatPreviousOverrideAsAcquiredItems()
        {
            var count = 3;
            var state = new BottleHolderCountOverride(() => count, value => count = value);
            state.Apply(12);
            count += 2;
            state.Apply(20);

            state.SuspendForSave();
            Assert.Equal(5, count);
            state.ResumeAfterSave();
            Assert.Equal(20, count);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(BottleHolderCountOverride.MaxCount + 1)]
        [InlineData(int.MaxValue)]
        public void InvalidTarget_DoesNotWrite(int target)
        {
            var count = 3;
            var state = new BottleHolderCountOverride(() => count, value => count = value);

            Assert.Throws<ArgumentOutOfRangeException>(() => state.Apply(target));
            Assert.Equal(3, count);
        }

        [Fact]
        public void FailedSerialization_FinalizerRestoresRuntimeCount()
        {
            var count = 3;
            var state = new BottleHolderCountOverride(() => count, value => count = value);
            state.Apply(12);

            Assert.Throws<IOException>((Action)(() =>
            {
                try
                {
                    state.SuspendForSave();
                    Assert.Equal(3, count);
                    throw new IOException("save failed");
                }
                finally
                {
                    HPatches.SetBottleHolderCountPatch.SaveInventoryFinalizer(state);
                }
            }));

            Assert.Equal(12, count);
        }

        [Fact]
        public void FailedSavePreparation_FinalizerRestoresRuntimeCount()
        {
            var count = 3;
            var fail = false;
            var state = new BottleHolderCountOverride(() => count, value =>
            {
                count = value;
                if (fail && value == 3)
                    throw new InvalidOperationException("refresh failed");
            });
            state.Apply(12);
            fail = true;

            Assert.Throws<InvalidOperationException>(() => state.SuspendForSave());
            HPatches.SetBottleHolderCountPatch.SaveInventoryFinalizer(state);

            Assert.Equal(12, count);
        }

        [Fact]
        public void PartialApplyFailure_IsNotSavedAsGameProgress()
        {
            var count = 3;
            var state = new BottleHolderCountOverride(() => count, value =>
            {
                count = value;
                if (value == 12)
                {
                    count = 8;
                    throw new InvalidOperationException("partial write");
                }
            });

            Assert.Throws<InvalidOperationException>(() => state.Apply(12));
            state.SuspendForSave();
            Assert.Equal(3, count);
            state.ResumeAfterSave();
            Assert.Equal(8, count);
        }

        [Fact]
        public void NestedSerialization_RestoresOnlyAfterOutermostCall()
        {
            var count = 3;
            var state = new BottleHolderCountOverride(() => count, value => count = value);
            state.Apply(12);

            state.SuspendForSave();
            state.SuspendForSave();
            Assert.Throws<InvalidOperationException>(() => state.Apply(20));
            state.ResumeAfterSave();
            Assert.Equal(3, count);
            state.ResumeAfterSave();
            Assert.Equal(12, count);
            state.ResumeAfterSave();
            Assert.Equal(12, count);
        }

    }
}
