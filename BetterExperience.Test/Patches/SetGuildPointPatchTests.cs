using nel;
using System.Runtime.CompilerServices;
using static BetterExperience.Patches.HPatches.SetGuildPointPatch;

namespace BetterExperience.Test.Patches
{
    public class SetGuildPointPatchTests
    {
        // 仅测试字段读写与游戏等级计算，不启动构造器中的游戏资源加载。
        private static GuildManager CreateGuild(int point)
        {
            var guild = (GuildManager)RuntimeHelpers.GetUninitializedObject(typeof(GuildManager));
            guild.gq_point = point;
            return guild;
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(4, 4, 0)]
        [InlineData(5, 5, 1)]
        [InlineData(20, 20, 2)]
        [InlineData(50, 50, 3)]
        [InlineData(115, 115, 4)]
        [InlineData(9999, 9999, 4)]
        [InlineData(10000, 9999, 4)]
        [InlineData(int.MaxValue, 9999, 4)]
        public void Apply_ValidValue_WritesClampedPointsAndUpdatesRank(int requested, int expected, int rank)
        {
            var guild = CreateGuild(50);

            Assert.Equal(ApplyResult.Applied, Apply(guild, requested, () => false));
            Assert.Equal(expected, GetGuildPoint(guild));
            Assert.Equal(rank, guild.current_grank);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void Apply_NegativeValue_PreservesPointsWithoutScanning(int requested)
        {
            var guild = CreateGuild(50);

            Assert.Equal(ApplyResult.Ignored, Apply(guild, requested, () => throw new Exception("Unexpected scan")));
            Assert.Equal(50, guild.gq_point);
        }

        [Fact]
        public void MissingGuild_ReadsUnavailableAndRejectsWrite()
        {
            Assert.Equal(-1, GetGuildPoint(null));
            Assert.Equal(ApplyResult.Unavailable, Apply(null, 100, () => throw new Exception("Unexpected scan")));
        }

        [Fact]
        public void Apply_UninitializedGuild_DoesNotUnlockGuild()
        {
            var guild = CreateGuild(-1);

            Assert.Equal(ApplyResult.GuildNotInitialized, Apply(guild, 100, () => throw new Exception("Unexpected scan")));
            Assert.Equal(-1, guild.gq_point);
        }

        [Fact]
        public void Apply_InterfaceOpen_RejectsWriteUntilUserRetriesAfterClosing()
        {
            var guild = CreateGuild(50);

            Assert.Equal(ApplyResult.InterfaceOpen, Apply(guild, 100, () => true));
            Assert.Equal(50, guild.gq_point);
            Assert.Equal(ApplyResult.Applied, Apply(guild, 100, () => false));
            Assert.Equal(100, guild.gq_point);

        }

        [Fact]
        public void Apply_InterfaceScanFails_DoesNotWritePoints()
        {
            var guild = CreateGuild(50);

            Assert.Throws<InvalidOperationException>(() => Apply(guild, 100,
                () => throw new InvalidOperationException("Scene query failed")));
            Assert.Equal(50, guild.gq_point);
        }
    }
}
