using BetterExperience.Patches.ReplaceTexture;
using Spine;

namespace BetterExperience.Test.Patches.ReplaceTexture
{
    public sealed class EncryptedReplacementTests : IDisposable
    {
        private readonly EncryptedPackFixture pack = new EncryptedPackFixture();

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public void CatalogAndComposer_LoadPlainEncryptedAndMixedTargets(int encrypted)
        {
            pack.CreatePack(encrypted);
            var catalog = pack.Discover();
            Assert.Empty(catalog.Errors);
            Assert.True(catalog.DeclaredIdsComplete);
            Assert.Contains("立绘包", catalog.DeclaredIds);
            Assert.Equal(3, Assert.Single(catalog.Packages).Targets.Count);
            var layer = Assert.Single(catalog.Layers(EncryptedPackFixture.SpineIdentity, new[] { "立绘包" }));
            var atlas = PortraitCatalog.ReadAtlas(ReplacementResourceIO.ReadText(layer.AtlasPath));
            byte[] png = ReplacementResourceIO.ReadBytes(layer.ImagePath);
            Assert.Equal(EncryptedPackFixture.Png, png);
            PortraitCatalog.ValidateImage(png, atlas);
            var composed = SpineComposer.Compose(EncryptedPackFixture.Skeleton, new[] { layer }, atlas);
            var data = new SkeletonJson(atlas).ReadSkeletonData(new StringReader(composed.Json));
            Assert.NotNull(data.FindAnimation("stand"));
            Assert.NotNull(data.FindSkin("default"));
            Assert.Equal("root", Assert.Single(data.Bones).Name);
        }

        [Fact]
        public void MixedLayers_PreserveConfiguredOrderAndLaterOverride()
        {
            pack.CreatePack(15, "first", "first");
            pack.CreatePack(2, "second", "second");
            pack.WriteText("first/page.json", EncryptedPackFixture.Skeleton.Replace("\"name\":\"root\"", "\"name\":\"root\",\"x\":1"), true);
            pack.WriteText("second/page.json", EncryptedPackFixture.Skeleton.Replace("\"name\":\"root\"", "\"name\":\"root\",\"x\":2"));
            var catalog = pack.Discover();
            Assert.Empty(catalog.Errors);
            var atlas = PortraitCatalog.ReadAtlas(EncryptedPackFixture.Atlas);
            foreach (var order in new[] { new[] { "first", "second" }, new[] { "second", "first" } })
            {
                var layers = catalog.Layers(EncryptedPackFixture.SpineIdentity, order).ToList();
                Assert.Equal(order, layers.Select(layer => layer.PackageId));
                var result = SpineComposer.Compose(EncryptedPackFixture.Skeleton, layers, atlas);
                var data = new SkeletonJson(atlas).ReadSkeletonData(new StringReader(result.Json));
                Assert.Equal(order[1] == "first" ? 1 : 2, data.FindBone("root").X);
            }
        }

        [Fact]
        public void CorruptManifest_PreservesConfigurationUntilSuccessfulRefresh()
        {
            pack.CreatePack(15);
            var rows = new List<(string, bool)> { ("立绘包", true), ("unknown", false) };
            pack.Corrupt("pack.replacement.json");
            var broken = pack.Discover();
            Assert.Empty(broken.Packages);
            Assert.Single(broken.Errors);
            Assert.False(broken.DeclaredIdsComplete);
            Assert.Equal(rows, broken.SyncRows(rows));

            pack.WriteText("pack.replacement.json", EncryptedPackFixture.Manifest("立绘包"), true);
            var refreshed = pack.Discover();
            Assert.Empty(refreshed.Errors);
            Assert.Equal(new[] { ("立绘包", true) }, refreshed.SyncRows(rows));
        }

        [Theory]
        [InlineData("page.png", 0)]
        [InlineData("page.atlas", 2)]
        [InlineData("page.json", 2)]
        public void CorruptDependency_MarksFailedTargetsAndRecoversOnRefresh(string file, int validTargets)
        {
            pack.CreatePack(15);
            pack.Corrupt(file);
            var damaged = pack.Discover();
            var package = Assert.Single(damaged.Packages);
            Assert.Equal(validTargets, package.Targets.Count);
            Assert.Contains(EncryptedPackFixture.SpineIdentity, package.InvalidTargetIdentities);
            Assert.Equal(3 - validTargets, damaged.Errors.Count);
            Assert.True(damaged.DeclaredIdsComplete);
            Assert.Equal(new[] { ("立绘包", true) }, damaged.SyncRows(new[] { ("立绘包", true) }));

            pack.CreatePack(15);
            Assert.Empty(pack.Discover().Errors);
            Assert.Equal(3, Assert.Single(pack.Discover().Packages).Targets.Count);
        }

        [Fact]
        public void Composer_RejectsResourceChangedAfterDiscovery()
        {
            pack.CreatePack(15);
            var layers = pack.Discover().Layers(EncryptedPackFixture.SpineIdentity, new[] { "立绘包" }).ToList();
            var atlas = PortraitCatalog.ReadAtlas(EncryptedPackFixture.Atlas);
            string lastGood = SpineComposer.Compose(EncryptedPackFixture.Skeleton, layers, atlas).Json;
            pack.Corrupt("page.json");
            Assert.Throws<InvalidDataException>(() => SpineComposer.Compose(EncryptedPackFixture.Skeleton, layers, atlas));
            Assert.NotNull(new SkeletonJson(atlas).ReadSkeletonData(new StringReader(lastGood)).FindAnimation("stand"));
        }

        [Fact]
        public void EncryptedSensitivePack_RequiresAuthorizationAndKeepsRowsWhenHidden()
        {
            pack.CreatePack(15, "Sensitive", "secret");
            var hidden = pack.Discover(false);
            Assert.Empty(hidden.Packages);
            Assert.Empty(hidden.Errors);
            Assert.Contains("secret", hidden.DeclaredIds);
            Assert.Equal(new[] { ("secret", true) }, hidden.SyncRows(new[] { ("secret", true) }));
            Assert.True(Assert.Single(pack.Discover(true).Packages).Sensitive);
            pack.Corrupt("Sensitive/page.png");
            Assert.Empty(pack.Discover(false).Errors);
        }

        [Theory]
        [InlineData("Sensitive/page.png")]
        [InlineData("../outside.png")]
        public void EncryptedManifest_DoesNotBypassPathOrSensitiveBoundaries(string image)
        {
            pack.CreatePack(15);
            pack.Write("Sensitive/page.png", EncryptedPackFixture.Png, true);
            pack.WriteText("pack.replacement.json", EncryptedPackFixture.Manifest("立绘包").Replace("\"image\":\"page.png\"", "\"image\":\"" + image + "\""), true);
            var catalog = pack.Discover();
            Assert.Empty(Assert.Single(catalog.Packages).Targets);
            Assert.Equal(3, catalog.Errors.Count);
        }

        public void Dispose() => pack.Dispose();
    }
}
