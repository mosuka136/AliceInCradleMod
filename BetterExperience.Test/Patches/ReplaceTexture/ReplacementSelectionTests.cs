using BetterExperience.Patches.ReplaceTexture;

namespace BetterExperience.Test.Patches.ReplaceTexture
{
    public sealed class ReplacementSelectionTests
    {
        [Fact]
        public void Selection_ChangesOnlyAffectedSpineTargetsAndPreservesLayerOrder()
        {
            var catalog = new ReplacementCatalog();
            var stand = Add(catalog, "stand", "stand");
            var bench = Add(catalog, "bench", "bench");
            var overlay = Add(catalog, "overlay", "stand");
            var before = Select(catalog, "stand", "bench");
            var after = Select(catalog, "bench", "stand", "overlay");

            Assert.True(before.SameSpine(after, bench.Identity));
            Assert.False(before.SameSpine(after, stand.Identity));
            Assert.Equal(new[] { stand, overlay }, after.Layers(stand.Identity));
            Assert.True(after.SameSpine(Select(catalog, "stand", "overlay", "bench"), stand.Identity));
            Assert.False(after.SameSpine(Select(catalog, "overlay", "stand", "bench"), stand.Identity));
        }

        [Fact]
        public void Selection_CanSwitchUsingKnownMetadataWithoutReadingFiles()
        {
            using var pack = new EncryptedPackFixture();
            pack.CreatePack(15);
            var catalog = pack.Discover();
            // 缓存目录的选择操作不打开已经加密的资源文件。
            using var file = File.Open(Path.Combine(pack.Input, "pack.replacement.json"), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            var enabled = Select(catalog, "立绘包");
            var disabled = Select(catalog);
            Assert.Single(enabled.Layers(EncryptedPackFixture.SpineIdentity));
            Assert.Empty(disabled.Layers(EncryptedPackFixture.SpineIdentity));
            Assert.False(enabled.SameSpine(disabled, EncryptedPackFixture.SpineIdentity));
        }

        [Fact]
        public void Selection_OnlyWinningOrdinaryTextureTriggersReload()
        {
            var catalog = new ReplacementCatalog();
            var low = Add(catalog, "low", "unused");
            var wildcard = Add(catalog, "wildcard", "unused");
            var high = Add(catalog, "high", "unused");
            foreach (var target in new[] { low, wildcard, high })
            {
                target.Type = "texture";
                target.Loader = "mti";
                target.AssetKey = "UI/Sheet";
                target.ImageKey = target == wildcard ? null : "page";
            }
            var all = Select(catalog, "low", "wildcard", "high");
            Assert.Same(high, all.Texture("mti", "UI/Sheet", "page", null));
            Assert.Same(wildcard, all.Texture("mti", "UI/Sheet", "other", null));
            Assert.True(all.SameTexture(Select(catalog, "wildcard", "high"), "mti", "UI/Sheet", "page", null));
            Assert.False(all.SameTexture(Select(catalog, "high", "wildcard"), "mti", "UI/Sheet", "page", null));
        }

        [Fact]
        public void Selection_SensitiveRevocationKeepsNormalTargetsAndFiltersInvalidSensitiveLayers()
        {
            var catalog = new ReplacementCatalog();
            var normal = Add(catalog, "normal", "stand");
            var sensitive = Add(catalog, "secret", "bench");
            sensitive.Owner.Sensitive = true;
            sensitive.Owner.InvalidTargetIdentities.Add("spine\ndamaged\ndamaged");
            var allowed = Select(catalog, "normal", "secret");
            var denied = new ReplacementSelection(catalog, new[] { "normal", "secret" }, true, false);
            Assert.True(allowed.SameSpine(denied, normal.Identity));
            Assert.Empty(denied.Layers(sensitive.Identity));
            Assert.False(allowed.SameSpine(denied, sensitive.Identity));
            Assert.False(denied.Invalid("spine\ndamaged\ndamaged"));
            Assert.Empty(new ReplacementSelection(catalog, new[] { "normal" }, false, true).Layers(normal.Identity));
        }

        [Fact]
        public void Selection_DamagedTargetsStillInvalidateLastGoodState()
        {
            var catalog = new ReplacementCatalog();
            var target = Add(catalog, "broken", "stand");
            target.Owner.Targets.Clear();
            target.Owner.InvalidTargetIdentities.Add(target.Identity);
            var before = Select(catalog);
            var after = Select(catalog, "broken");
            Assert.True(after.Invalid(target.Identity));
            Assert.False(before.SameSpine(after, target.Identity));
            target.Owner.HasUnidentifiedTargetErrors = true;
            Assert.False(before.SameSpine(Select(catalog, "broken"), "spine\nother\nother"));
        }

        [Fact]
        public void Selection_RebuiltCatalogInvalidatesTargetsEvenWhenPackageIdsAreUnchanged()
        {
            var beforeCatalog = new ReplacementCatalog();
            var afterCatalog = new ReplacementCatalog();
            var first = Add(beforeCatalog, "same", "stand");
            Add(afterCatalog, "same", "stand");
            Assert.False(Select(beforeCatalog, "same").SameSpine(Select(afterCatalog, "same"), first.Identity));
        }

        [Fact]
        public void Selection_RevokesOldResourcesEvenWhenDamagedManifestIsAbsentFromCatalog()
        {
            var catalog = new ReplacementCatalog();
            var old = Add(catalog, "old", "stand");
            catalog.Packages.Clear();
            var stillEnabled = Select(catalog, "old");
            var disabled = Select(catalog);
            Assert.True(stillEnabled.SameSpine(disabled, old.Identity));
            Assert.True(stillEnabled.Authorizes(new[] { old.Owner }));
            Assert.False(disabled.Authorizes(new[] { old.Owner }));
            old.Owner.Sensitive = true;
            Assert.False(new ReplacementSelection(catalog, new[] { "old" }, true, false).Authorizes(new[] { old.Owner }));
            Assert.False(new ReplacementSelection(catalog, new[] { "old" }, false, true).Authorizes(new[] { old.Owner }));
        }

        [Fact]
        public void Delay_CoalescesRapidChangesAndCancelsWhenSelectionReturnsToCurrent()
        {
            var delay = new ReplacementSelectionDelay();
            delay.Reset("A");
            Assert.False(delay.Ready("B", 0, false));
            Assert.True(delay.Waiting);
            Assert.False(delay.Ready("C", 0.1f, false));
            Assert.False(delay.Ready("C", 0.2f, false));
            Assert.True(delay.Ready("C", 0.3f, false));
            delay.Reset("C");
            Assert.False(delay.Waiting);
            Assert.False(delay.Ready("B", 0.4f, false));
            Assert.False(delay.Ready("C", 0.5f, false));
            Assert.False(delay.Waiting);
            Assert.False(delay.Ready("C", 2f, false));
            Assert.True(delay.Ready("disabled", 2f, true));
        }

        private static ReplacementSelection Select(ReplacementCatalog catalog, params string[] ids) =>
            new ReplacementSelection(catalog, ids, true, true);

        private static ReplacementTarget Add(ReplacementCatalog catalog, string id, string pose)
        {
            var package = new ReplacementPackage { Id = id };
            var target = new ReplacementTarget { Owner = package, PackageId = id, Type = "spine", SpineKey = pose, JsonKey = pose };
            package.Targets.Add(target);
            catalog.Packages.Add(package);
            return target;
        }
    }
}
