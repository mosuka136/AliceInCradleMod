using nel;
using static BetterExperience.Patches.HPatches.SetCurrencyCountPatch;

namespace BetterExperience.Test.Patches
{
    public class SetCurrencyCountPatchTests
    {
        private static CoinEntry CreateEntry(CoinStorage.CTYPE type, uint current, uint obtain)
        {
            var entry = new CoinEntry(type, type.ToString());
            entry.Set(current);
            entry.total_obtain = obtain;
            return entry;
        }

        [Theory]
        [InlineData(CoinStorage.CTYPE.GOLD)]
        [InlineData(CoinStorage.CTYPE.CRAFTS)]
        [InlineData(CoinStorage.CTYPE.JUICE)]
        [InlineData(CoinStorage.CTYPE.BAR_SCORE)]
        public void TryGetLockFlag_KnownCurrencies_AreRecognized(CoinStorage.CTYPE type)
        {
            Assert.True(TryGetLockFlag(type, out var locked));
            Assert.False(locked);
        }

        [Theory]
        [InlineData(CoinStorage.CTYPE._MAX)]
        [InlineData(CoinStorage.CTYPE._TEMPORARY)]
        public void TryGetLockFlag_UnknownCurrencies_AreNotLocked(CoinStorage.CTYPE type)
        {
            Assert.False(TryGetLockFlag(type, out var locked));
            Assert.False(locked);
        }

        [Fact]
        public void ShouldRaiseObtain_OnlyBarScore()
        {
            Assert.False(ShouldRaiseObtain(CoinStorage.CTYPE.GOLD));
            Assert.False(ShouldRaiseObtain(CoinStorage.CTYPE.CRAFTS));
            Assert.False(ShouldRaiseObtain(CoinStorage.CTYPE.JUICE));
            Assert.True(ShouldRaiseObtain(CoinStorage.CTYPE.BAR_SCORE));
        }

        [Fact]
        public void WriteCount_BarScore_RaisesObtainToAtLeastCurrent()
        {
            var entry = CreateEntry(CoinStorage.CTYPE.BAR_SCORE, 10, 5);

            Assert.Equal(20000u, WriteCount(entry, 20000, true));
            Assert.Equal(20000u, entry.Get());
            Assert.Equal(20000u, entry.total_obtain);
        }

        [Fact]
        public void WriteCount_BarScore_DoesNotLowerExistingObtain()
        {
            var entry = CreateEntry(CoinStorage.CTYPE.BAR_SCORE, 10, 50000);

            WriteCount(entry, 20000, true);
            Assert.Equal(20000u, entry.Get());
            Assert.Equal(50000u, entry.total_obtain);
        }

        [Fact]
        public void WriteCount_Gold_LeavesObtainUnchanged()
        {
            var entry = CreateEntry(CoinStorage.CTYPE.GOLD, 10, 5);

            WriteCount(entry, 20000, ShouldRaiseObtain(CoinStorage.CTYPE.GOLD));
            Assert.Equal(20000u, entry.Get());
            Assert.Equal(5u, entry.total_obtain);
        }

        [Fact]
        public void WriteCount_ClampsToMaxCount()
        {
            var entry = CreateEntry(CoinStorage.CTYPE.BAR_SCORE, 0, 0);

            Assert.Equal(CoinEntry.MAX_COUNT, WriteCount(entry, uint.MaxValue, true));
            Assert.Equal(CoinEntry.MAX_COUNT, entry.Get());
            Assert.Equal(CoinEntry.MAX_COUNT, entry.total_obtain);
        }

        [Theory]
        [InlineData(0L, true, 0u)]
        [InlineData(20000L, true, 20000u)]
        [InlineData(-1L, false, 0u)]
        [InlineData((long)uint.MaxValue + 1, false, 0u)]
        public void TryConvertCount_FiltersOutOfRangeValues(long count, bool expected, uint value)
        {
            Assert.Equal(expected, TryConvertCount(count, out var converted));
            Assert.Equal(value, converted);
        }

        [Fact]
        public void DealWithCurrencyCount_Unlocked_AllowsChange()
        {
            var entry = CreateEntry(CoinStorage.CTYPE.BAR_SCORE, 10, 10);
            Assert.True(DealWithCurrencyCount(false, entry));
        }
    }
}
