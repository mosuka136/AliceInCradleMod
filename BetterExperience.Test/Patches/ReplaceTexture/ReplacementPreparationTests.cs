using BetterExperience.Patches.ReplaceTexture;
using Spine;
using Spine.Unity;
using System.Reflection;

namespace BetterExperience.Test.Patches.ReplaceTexture
{
    public sealed class ReplacementPreparationTests : IDisposable
    {
        private readonly EncryptedPackFixture pack = new EncryptedPackFixture();
        private string Sensitive => Path.Combine(pack.Input, "Sensitive");

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public async Task Preparation_LoadsPlainMixedAndEncryptedPacksWithoutUnityObjects(int encrypted)
        {
            pack.CreatePack(encrypted);
            using var scan = new ReplacementWork<ReplacementCatalog>(token =>
                ReplacementCatalog.Discover(pack.Input, Sensitive, false, token));
            await ReplacementWorkTests.Complete(scan);
            Assert.True(scan.TryTake(out var catalog, out var scanError));
            Assert.Null(scanError);
            Assert.Empty(catalog.Errors);
            var layers = Layers(catalog);
            using var work = new ReplacementWork<PreparedSpine>(token => Prepare(layers, token));
            await ReplacementWorkTests.Complete(work);
            Assert.True(work.TryTake(out var prepared, out var error));
            Assert.Null(error);
            Assert.Equal(EncryptedPackFixture.Png, prepared.ImageBytes);
            Assert.NotNull(prepared.Composition.PreparedData.FindAnimation("stand"));
            Assert.Single(prepared.Atlas.Pages);
            Assert.Null(prepared.Atlas.Pages[0].rendererObject);

            var texture = catalog.Packages[0].Targets.First(target => target.Type == "texture");
            using var image = new ReplacementWork<byte[]>(token =>
                ReplacementPreparation.Texture(texture, pack.Input, Sensitive, false, token));
            await ReplacementWorkTests.Complete(image);
            Assert.True(image.TryTake(out var bytes, out var imageError));
            Assert.Null(imageError);
            Assert.Equal(EncryptedPackFixture.Png, bytes);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void Preparation_PreservesUnityAtlasFlipAndSkeletonScale(bool mesh, bool overrideScale)
        {
            pack.CreatePack(15);
            string skeleton = EncryptedPackFixture.Skeleton.Replace("\"name\":\"root\"", "\"name\":\"root\",\"x\":12");
            if (mesh) skeleton = skeleton.Replace("\"width\":1,\"height\":1", "\"type\":\"mesh\",\"uvs\":[0,0,1,0,0,1],\"triangles\":[0,1,2],\"vertices\":[0,0,1,0,0,1],\"hull\":3");
            pack.WriteText("page.json", skeleton, true);
            var layers = Layers(pack.Discover());
            if (overrideScale) layers[0].Display.SkeletonScale = 0.25f;
            var prepared = Prepare(layers);

            var baselineAtlas = PortraitCatalog.ReadAtlas(EncryptedPackFixture.Atlas);
            var baselineComposition = SpineComposer.Compose(EncryptedPackFixture.Skeleton, layers, baselineAtlas);
            baselineAtlas.FlipV();
            var baseline = new SkeletonJson(baselineAtlas) { Scale = overrideScale ? 0.25f : 0.5f }
                .ReadSkeletonData(new StringReader(baselineComposition.Json));
            var actual = prepared.Composition.PreparedData;
            Assert.Equal(overrideScale ? 3 : 6, actual.FindBone("root").X);
            Assert.Equal(baseline.FindBone("root").X, actual.FindBone("root").X);
            var expectedAttachment = baseline.DefaultSkin.GetAttachment(0, "part");
            var actualAttachment = actual.DefaultSkin.GetAttachment(0, "part");
            if (mesh)
            {
                var expectedMesh = Assert.IsType<MeshAttachment>(expectedAttachment);
                var actualMesh = Assert.IsType<MeshAttachment>(actualAttachment);
                Assert.Equal(expectedMesh.UVs, actualMesh.UVs);
                Assert.Equal(expectedMesh.Vertices, actualMesh.Vertices);
            }
            else
            {
                var expectedRegion = Assert.IsType<RegionAttachment>(expectedAttachment);
                var actualRegion = Assert.IsType<RegionAttachment>(actualAttachment);
                Assert.Equal(expectedRegion.UVs, actualRegion.UVs);
                Assert.Equal(expectedRegion.Width, actualRegion.Width);
            }
            Assert.Same(prepared.Atlas.Regions[0], ((IHasTextureRegion)actualAttachment).Region);
        }

        [Fact]
        public void Preparation_KeepsOriginalAtlasForDisplayOnlyLayer()
        {
            pack.CreatePack();
            var layers = Layers(pack.Discover());
            layers[0].ImagePath = null;
            layers[0].AtlasPath = null;
            layers[0].JsonPath = null;
            layers[0].Display.OffsetX = 12;
            var prepared = Prepare(layers);
            Assert.Null(prepared.ImageBytes);
            Assert.Equal(EncryptedPackFixture.Atlas, prepared.AtlasText);
            Assert.Equal(12, prepared.Composition.Display.OffsetX);
            Assert.NotNull(prepared.Composition.PreparedData);
        }

        [Theory]
        [InlineData("page.png")]
        [InlineData("page.atlas")]
        [InlineData("page.json")]
        public async Task Preparation_RejectsFilesDamagedAfterDiscoveryAndCanRetry(string damaged)
        {
            pack.CreatePack(15);
            var layers = Layers(pack.Discover());
            pack.Corrupt(damaged);
            using var failed = new ReplacementWork<PreparedSpine>(token => Prepare(layers, token));
            await ReplacementWorkTests.Complete(failed);
            Assert.True(failed.TryTake(out var result, out var error));
            Assert.Null(result);
            Assert.IsType<InvalidDataException>(error);
            pack.CreatePack(15);
            Assert.NotNull(Prepare(layers).Composition.PreparedData);
        }

        [Fact]
        public void Preparation_RechecksSensitiveBoundaryAuthorizationAndMissingFiles()
        {
            pack.CreatePack(15, "Sensitive", "secret");
            var layers = Layers(pack.Discover(), "secret");
            Assert.Throws<InvalidDataException>(() => Prepare(layers));
            Assert.NotNull(ReplacementPreparation.Spine(EncryptedPackFixture.Skeleton,
                EncryptedPackFixture.Atlas, 0.5f, layers, pack.Input, Sensitive, true, default));
            layers[0].ImagePath = pack.Write("outside.png", EncryptedPackFixture.Png);
            Assert.Throws<InvalidDataException>(() => ReplacementPreparation.Validate(layers, pack.Input, Sensitive, true));
            layers[0].ImagePath = Path.Combine(pack.Input, "missing.png");
            Assert.Throws<FileNotFoundException>(() => ReplacementPreparation.Validate(layers, pack.Input, Sensitive, true));
        }

        [Fact]
        public void Preparation_CancelledRequestsDoNotReadLockedFiles()
        {
            pack.CreatePack(15);
            var layers = Layers(pack.Discover());
            using var locked = File.Open(layers[0].Owner.ManifestPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            var cancelled = new CancellationToken(true);
            Assert.Throws<OperationCanceledException>(() => ReplacementCatalog.Discover(pack.Input, Sensitive, true, cancelled));
            Assert.Throws<OperationCanceledException>(() => Prepare(layers, cancelled));
            Assert.Throws<OperationCanceledException>(() => ReplacementPreparation.Texture(layers[0], pack.Input, Sensitive, true, cancelled));
        }

        [Fact]
        public async Task BackgroundScan_CorruptManifestStillPreservesExistingConfiguration()
        {
            pack.CreatePack(15);
            pack.Corrupt("pack.replacement.json");
            using var scan = new ReplacementWork<ReplacementCatalog>(token =>
                ReplacementCatalog.Discover(pack.Input, Sensitive, false, token));
            await ReplacementWorkTests.Complete(scan);
            Assert.True(scan.TryTake(out var catalog, out var error));
            Assert.Null(error);
            Assert.Single(catalog.Errors);
            var rows = new[] { ("立绘包", true), ("unknown", false) };
            Assert.Equal(rows, catalog.SyncRows(rows));
        }

        [Fact]
        public void Runtime_GameInterfacesAllowReusingPreparedAtlasAndSkeletonData()
        {
            var atlas = typeof(SpineAtlasAsset).GetField("atlas", BindingFlags.Instance | BindingFlags.NonPublic);
            var initialize = typeof(SkeletonDataAsset).GetMethod("InitializeWithData", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(atlas);
            Assert.Equal(typeof(Atlas), atlas.FieldType);
            Assert.NotNull(initialize);
            Assert.Equal(typeof(SkeletonData), Assert.Single(initialize.GetParameters()).ParameterType);
            Assert.False(new BlendModeMaterials().RequiresBlendModeMaterials);
        }

        private static List<ReplacementTarget> Layers(ReplacementCatalog catalog, string id = "立绘包") =>
            catalog.Layers(EncryptedPackFixture.SpineIdentity, new[] { id }).ToList();

        private PreparedSpine Prepare(IReadOnlyList<ReplacementTarget> layers, CancellationToken token = default) =>
            ReplacementPreparation.Spine(EncryptedPackFixture.Skeleton, EncryptedPackFixture.Atlas,
                0.5f, layers, pack.Input, Sensitive, false, token);

        public void Dispose() => pack.Dispose();
    }
}
