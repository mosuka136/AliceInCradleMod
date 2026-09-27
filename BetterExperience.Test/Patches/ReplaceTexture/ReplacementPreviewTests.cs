using BetterExperience.Patches;
using BetterExperience.Patches.ReplaceTexture;
using nel;

namespace BetterExperience.Test.Patches.ReplaceTexture
{
    public sealed class ReplacementPreviewTests
    {
        [Fact]
        public void NewlyEnabledPortraits_IgnoreDisablingReorderingTexturesAndInvalidTargets()
        {
            var catalog = new ReplacementCatalog();
            var stand = Add(catalog, "stand", "stand");
            var bench = Add(catalog, "bench", "bench");
            var texture = Add(catalog, "texture", "texture");
            texture.Type = "texture";
            var bad = Add(catalog, "bad", "bad");
            bad.Owner.InvalidTargetIdentities.Add(bad.Identity);
            var before = Select(catalog, "stand");
            var after = Select(catalog, "stand", "bench", "texture", "bad");
            Assert.Equal(new[] { bench }, after.NewlyEnabledPortraits(before));
            Assert.Empty(before.NewlyEnabledPortraits(after));
            Assert.Empty(Select(catalog, "bench", "stand", "texture", "bad").NewlyEnabledPortraits(after));
            Assert.Equal(new[] { stand, bench }, after.NewlyEnabledPortraits(new ReplacementSelection(catalog, after.EnabledIds, false, true)));
        }

        [Fact]
        public void NewlyEnabledPortraits_RespectSensitiveAuthorization()
        {
            var catalog = new ReplacementCatalog();
            var secret = Add(catalog, "secret", "bench");
            secret.Owner.Sensitive = true;
            var denied = new ReplacementSelection(catalog, new[] { "secret" }, true, false);
            var allowed = Select(catalog, "secret");
            Assert.Empty(denied.NewlyEnabledPortraits(allowed));
            Assert.Equal(new[] { secret }, allowed.NewlyEnabledPortraits(denied));
        }

        [Fact]
        public void PreviewLayers_PromoteSelectedResourceWithoutChangingConfiguredOrder()
        {
            var catalog = new ReplacementCatalog();
            var low = Add(catalog, "low", "stand");
            var middle = Add(catalog, "middle", "stand");
            var high = Add(catalog, "high", "stand");
            var other = Add(catalog, "other", "bench");
            var selection = Select(catalog, "low", "middle", "high", "other");
            Assert.Equal(new[] { middle, high, low }, ReplacementPreview.Layers(selection, low));
            Assert.Equal(new[] { low, high, middle }, ReplacementPreview.Layers(selection, middle));
            Assert.Equal(new[] { low, middle, high }, selection.Layers(low.Identity));
            Assert.Equal(new[] { other }, selection.Layers(other.Identity));
            Assert.Empty(ReplacementPreview.Layers(Select(catalog, "high"), low));
        }

        [Fact]
        public void PreviewComposition_ShowsNewLowPriorityPackThenNormalCompositionStillShowsHighPack()
        {
            using var pack = new EncryptedPackFixture();
            pack.CreatePack(15, "low", "low");
            pack.CreatePack(15, "high", "high");
            pack.WriteText("low/page.json", EncryptedPackFixture.Skeleton.Replace("\"name\":\"root\"", "\"name\":\"root\",\"x\":10"), true);
            pack.WriteText("high/page.json", EncryptedPackFixture.Skeleton.Replace("\"name\":\"root\"", "\"name\":\"root\",\"x\":20"), true);
            var catalog = pack.Discover();
            var selection = Select(catalog, "low", "high");
            var normal = selection.Layers(EncryptedPackFixture.SpineIdentity);
            var target = normal.First();
            var temporary = ReplacementPreview.Layers(selection, target);
            string root = pack.Input, sensitive = Path.Combine(root, "Sensitive");
            var preview = ReplacementPreparation.Spine(EncryptedPackFixture.Skeleton, EncryptedPackFixture.Atlas,
                1, temporary, root, sensitive, false, default);
            var restored = ReplacementPreparation.Spine(EncryptedPackFixture.Skeleton, EncryptedPackFixture.Atlas,
                1, normal, root, sensitive, false, default);
            Assert.Equal(10, preview.Composition.PreparedData.FindBone("root").X);
            Assert.Equal(20, restored.Composition.PreparedData.FindBone("root").X);
            Assert.Equal(new[] { "low", "high" }, selection.EnabledIds);
            Assert.Equal(new[] { "low", "high" }, normal.Select(layer => layer.PackageId));
            Assert.Equal(new[] { "high", "low" }, temporary.Select(layer => layer.PackageId));
        }

        [Fact]
        public void PreviewLayers_RejectRevokedSensitiveAndDamagedTargets()
        {
            var catalog = new ReplacementCatalog();
            var target = Add(catalog, "pack", "stand");
            target.Owner.Sensitive = true;
            Assert.Empty(ReplacementPreview.Layers(new ReplacementSelection(catalog, new[] { "pack" }, true, false), target));
            target.Owner.InvalidTargetIdentities.Add(target.Identity);
            Assert.Empty(ReplacementPreview.Layers(Select(catalog, "pack"), target));
        }

        [Fact]
        public void Choose_PrefersCurrentPoseWithinHighestPriorityNewPackage()
        {
            var catalog = new ReplacementCatalog();
            var low = Add(catalog, "low", "stand");
            var high = Add(catalog, "high", "bench");
            var highStand = new ReplacementTarget { Owner = high.Owner, PackageId = "high", Type = "spine", SpineKey = "stand", JsonKey = "stand" };
            var stand = Pose(UIEMOT.STAND, "stand");
            var bench = Pose(UIEMOT.BENCH, "bench");
            Assert.Same(stand, ReplacementPreview.Choose(new[] { low, highStand, high }, new[] { stand, bench }, out var currentTarget));
            Assert.Same(highStand, currentTarget);
            Assert.Same(bench, ReplacementPreview.Choose(new[] { low, high }, new[] { stand, bench }, out var highestTarget));
            Assert.Same(high, highestTarget);
        }

        [Fact]
        public void Choose_RequiresExactJsonVariantAndExcludesCutinsAndRegularImages()
        {
            var catalog = new ReplacementCatalog();
            var target = Add(catalog, "pack", "shared");
            target.JsonKey = "variant";
            var variant = Pose(UIEMOT.DOWN, "shared");
            variant.JsonKey = "variant";
            variant.Selection = new PortraitSelection(UIEMOT.DOWN, UIPictureBase.EMSTATE.TORNED);
            var cutin = Pose(UIEMOT.CUTS_COW_0, "shared");
            cutin.JsonKey = "variant";
            var wrongJson = Pose(UIEMOT.STAND, "shared");
            Assert.Null(ReplacementPreview.Choose(new[] { target }, new[] { cutin, wrongJson }, out _));
            Assert.Same(variant, ReplacementPreview.Choose(new[] { target }, new[] { cutin, wrongJson, variant }, out _));
            target.Type = "texture";
            Assert.Null(ReplacementPreview.Choose(new[] { target }, new[] { variant }, out _));
            Assert.Null(ReplacementPreview.Choose(Array.Empty<ReplacementTarget>(), new[] { variant }, out _));
        }

        private static ReplacementPreviewPose Pose(UIEMOT pose, string key) => new ReplacementPreviewPose
        { Selection = new PortraitSelection(pose), SpineKey = key, JsonKey = key };

        private static ReplacementSelection Select(ReplacementCatalog catalog, params string[] ids) =>
            new ReplacementSelection(catalog, ids, true, true);

        private static ReplacementTarget Add(ReplacementCatalog catalog, string id, string key)
        {
            var package = new ReplacementPackage { Id = id };
            var target = new ReplacementTarget { Owner = package, PackageId = id, Type = "spine", SpineKey = key, JsonKey = key };
            package.Targets.Add(target);
            catalog.Packages.Add(package);
            return target;
        }
    }
}
