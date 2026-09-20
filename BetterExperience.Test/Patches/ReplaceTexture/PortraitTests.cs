using BetterExperience.Patches.ReplaceTexture;
using Moq;
using Spine;
using UnityModBase.HGuiSpace.Bindings;
using UnityModBase.HGuiSpace.Editor;
using UnityModBase.HGuiSpace.Editor.ValueEditor;
using UnityModBase.HGuiSpace.Resource;
using UnityModBase.HProvider;

namespace BetterExperience.Test.Patches.ReplaceTexture
{
    public class PortraitTests : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "be-portrait-" + Guid.NewGuid().ToString("N"));
        private const string AtlasText = "page.png\nsize: 16,16\nfilter: Linear,Linear\npart\nbounds: 0,0,16,16\n";
        private const string Original = """
            {"skeleton":{"spine":"4.1.24","hash":"test"},"bones":[{"name":"root"}],
            "slots":[{"name":"body","bone":"root","attachment":"part"}],
            "skins":[{"name":"default","attachments":{"body":{"part":{
            "type":"mesh","uvs":[0,0,1,0,0,1],"triangles":[0,1,2],"vertices":[0,0,16,0,0,16],"hull":3}}}}],
            "animations":{"stand":{"bones":{"root":{"rotate":[{"value":0},{"time":1,"value":10}]}}}}}
            """;

        public PortraitTests() => Directory.CreateDirectory(directory);

        [Fact]
        public void Merge_ChangesGeometryAndKeepsOriginalAnimation()
        {
            var external = PortraitJson.Parse(Original);
            Attachment(external)["vertices"] = new List<object> { 0, 0, 24, 0, 0, 20 };
            external["animations"] = new Dictionary<string, object>();
            string merged = PortraitMerger.Merge(Original, PortraitJson.Serialize(external), PortraitCatalog.ReadAtlas(AtlasText));
            var result = PortraitJson.Parse(merged);
            Assert.True(PortraitJson.Equal(external["skins"], result["skins"]));
            Assert.True(PortraitJson.Equal(PortraitJson.Parse(Original)["animations"], result["animations"]));
            var skeleton = new SkeletonJson(PortraitCatalog.ReadAtlas(AtlasText)).ReadSkeletonData(new StringReader(merged));
            Assert.NotNull(skeleton.FindAnimation("stand"));
            Assert.Equal(24f, ((MeshAttachment)skeleton.DefaultSkin.GetAttachment(0, "part")).Vertices[2]);
        }

        [Fact]
        public void Merge_MissingReplacementKeepsOriginalAttachment()
        {
            var external = PortraitJson.Parse(Original);
            external["skins"] = new List<object>();
            var result = PortraitJson.Parse(PortraitMerger.Merge(Original, PortraitJson.Serialize(external), PortraitCatalog.ReadAtlas(AtlasText)));
            Assert.True(PortraitJson.Equal(PortraitJson.Parse(Original)["skins"], result["skins"]));
        }

        [Theory]
        [InlineData("bones")]
        [InlineData("slots")]
        [InlineData("ik")]
        public void Merge_RejectsSkeletonContractChanges(string property)
        {
            var external = PortraitJson.Parse(Original);
            external[property] = new List<object>();
            Assert.Throws<InvalidDataException>(() => Merge(external));
        }

        [Fact]
        public void Merge_RejectsVersionMismatch()
        {
            var external = PortraitJson.Parse(Original);
            PortraitJson.Object(external["skeleton"])["spine"] = "4.2.0";
            Assert.Throws<InvalidDataException>(() => Merge(external));
        }

        [Theory]
        [InlineData("triangles", "[0,1,9]")]
        [InlineData("vertices", "[1,9,0,0,1,1,0,16,0,1,1,0,0,16,1]")]
        [InlineData("vertices", "[1,0,0,0,0.5,1,0,16,0,1,1,0,0,16,1]")]
        [InlineData("vertices", "[1,0]")]
        public void Merge_RejectsInvalidMesh(string field, string array)
        {
            var external = PortraitJson.Parse(Original);
            Attachment(external)[field] = PortraitJson.Parse("{\"array\":" + array + "}")["array"];
            Assert.Throws<InvalidDataException>(() => Merge(external));
        }

        [Fact]
        public void Merge_RejectsMissingAtlasRegion()
        {
            Assert.Throws<InvalidDataException>(() => PortraitMerger.Merge(Original, Original,
                PortraitCatalog.ReadAtlas(AtlasText.Replace("part", "other"))));
        }

        [Fact]
        public void Merge_RejectsRegionRename()
        {
            var external = PortraitJson.Parse(Original);
            Attachment(external)["path"] = "other";
            Assert.Throws<InvalidDataException>(() => Merge(external));
        }

        [Theory]
        [InlineData("missing")]
        [InlineData("part")]
        public void Merge_RejectsMissingOrCyclicLinkedMesh(string parent)
        {
            var external = PortraitJson.Parse(Original);
            var attachment = Attachment(external);
            attachment.Clear();
            attachment["type"] = "linkedmesh";
            attachment["parent"] = parent;
            Assert.Throws<InvalidDataException>(() => Merge(external));
        }

        [Fact]
        public void Merge_DeformRejectsTopologyButAllowsCoordinateChanges()
        {
            var original = WithDeform();
            string source = PortraitJson.Serialize(original);
            var external = PortraitJson.Parse(source);
            Attachment(external)["vertices"] = new List<object> { 0, 0, 18, 0, 0, 20 };
            Assert.NotNull(PortraitMerger.Merge(source, PortraitJson.Serialize(external), PortraitCatalog.ReadAtlas(AtlasText)));
            Attachment(external)["uvs"] = new List<object> { 0, 0, 0, 1, 1, 0 };
            Assert.Throws<InvalidDataException>(() => PortraitMerger.Merge(source, PortraitJson.Serialize(external), PortraitCatalog.ReadAtlas(AtlasText)));
        }

        [Fact]
        public void Scan_ReservesDisabledPackageImagesWithoutEnablingPackage()
        {
            CreatePackage("one", "stand_normal");
            var result = Scan(false, "one");
            Assert.Empty(result.Packages);
            // 发现阶段与勾选状态无关：包已进入候选列表，只是不激活。
            Assert.Contains(result.Discovered, package => package.Id == "one");
            Assert.Contains(Path.Combine(directory, "page.png"), result.ReservedImages);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Discover_ListsAllPackagesRegardlessOfSelection()
        {
            CreatePackage("one", "stand_normal");
            CreatePackage("two", "sit_normal");
            var result = PortraitCatalog.Discover(directory, Path.Combine(directory, "Sensitive"), false);
            Assert.Equal(new[] { "one", "two" }, result.Discovered.Select(package => package.Id));
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Discover_DuplicateIdsAreExcludedFromOptions()
        {
            CreatePackage("one", "stand_normal");
            CreatePackage("two", "sit_normal", manifestId: "one");
            var result = PortraitCatalog.Discover(directory, Path.Combine(directory, "Sensitive"), false);
            Assert.Empty(result.Discovered);
            Assert.Contains(result.Errors, error => error.Contains("Duplicate portrait id: one"));
        }

        [Fact]
        public void Discover_SkipsSensitiveManifestsSilentlyWhenDisabled()
        {
            CreatePackage("one", "stand_normal", "Sensitive");
            var result = PortraitCatalog.Discover(directory, Path.Combine(directory, "Sensitive"), false);
            Assert.Empty(result.Discovered);
            Assert.Empty(result.Errors);
            Assert.Empty(result.ReservedImages);
        }

        [Fact]
        public void Discover_ReportsInvalidPngHeaderWithoutLoadingThePackage()
        {
            CreatePackage("one", "stand_normal");
            File.WriteAllText(Path.Combine(directory, "page.png"), "invalid");
            var result = PortraitCatalog.Discover(directory, Path.Combine(directory, "Sensitive"), false);
            Assert.Empty(result.Discovered);
            Assert.Single(result.ReservedImages);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void EnabledIds_OnlyCountsToggledRowsAndDeduplicates()
        {
            var ids = PortraitCatalog.EnabledIds(new[] { (" one ", true), ("", true), (null, true), ("one", true), ("two", false), ("two", true) });
            // 有序且按行序去重（保留首次出现位置）：行序是同目标冲突的优先级依据。
            Assert.Equal(new List<string> { "one", "two" }, ids);
            Assert.Empty(PortraitCatalog.EnabledIds(null));
        }

        [Fact]
        public void SyncRows_KeepsRowOrderAppendsNewAndDropsRemoved()
        {
            var discovered = new[] { Pack("one"), Pack("two"), Pack("three") };
            var current = new List<(string, bool)> { ("two", true), ("gone", true), ("", false), (null, false) };
            var rows = PortraitCatalog.SyncRows(discovered, current);
            // 已知包保持用户行序，新包按发现顺序追加且默认关闭；已移除的包（gone）行被删除，空白行原样置尾。
            Assert.Equal(new List<(string, bool)>
            {
                ("two", true), ("one", false), ("three", false), ("", false), (null, false)
            }, rows);
        }

        [Fact]
        public void SyncRows_KeepsUnknownRowsWhenCatalogIsPartiallyHidden()
        {
            var discovered = new[] { Pack("one") };
            var current = new List<(string, bool)> { ("one", true), ("sensitive", true) };
            // 敏感内容关闭时扫描看不到敏感包，无法区分“移除”与“隐藏”，未知行保留旧的置尾行为。
            Assert.Equal(new List<(string, bool)> { ("one", true), ("sensitive", true) },
                PortraitCatalog.SyncRows(discovered, current, false));
        }

        [Fact]
        public void ResolveSelection_DeselectsPreviouslyEnabledConflictingRows()
        {
            var discovered = new[] { Pack("a", "t1"), Pack("b", "t1"), Pack("c", "t2") };
            // 用户新启用 b（与已启用的 a 同目标）：自动取消 a，保留最新选择的 b；无冲突的 c 不受影响。
            var previous = new List<(string, bool)> { ("a", true), ("b", false), ("c", false) };
            var rows = new List<(string, bool)> { ("a", true), ("b", true), ("c", true) };
            Assert.Equal(new List<(string, bool)> { ("a", false), ("b", true), ("c", true) },
                PortraitCatalog.ResolveSelection(rows, previous, discovered));
            // 启动时已存在的存量冲突（无法判定先后）：保留行序靠后的行。
            Assert.Equal(new List<(string, bool)> { ("a", false), ("b", true) },
                PortraitCatalog.ResolveSelection(new List<(string, bool)> { ("a", true), ("b", true) },
                    Array.Empty<(string, bool)>(), discovered));
            // 无冲突时行集合保持不变。
            var quiet = new List<(string, bool)> { ("a", true), ("c", true) };
            Assert.Equal(quiet, PortraitCatalog.ResolveSelection(quiet, quiet, discovered));
        }

        [Fact]
        public void SyncRows_KeepsScannedOrderStableWhenRowsAlreadyMatch()
        {
            var discovered = new[] { Pack("one"), Pack("two") };
            var current = new List<(string, bool)> { ("one", false), ("two", true) };
            var rows = PortraitCatalog.SyncRows(discovered, current);
            Assert.Equal(current, rows);
            // 已同步的列表再次同步应产生相同内容，PortraitRuntime 据此跳过写回。
            Assert.Equal(current, PortraitCatalog.SyncRows(discovered, rows));
        }

        [Fact]
        public void ConfigEncoding_RoundTripsTupleListRows()
        {
            var value = new List<(string, bool)> { ("one", true), ("two", false) };
            var encoded = UnityModBase.HConfigSpace.ConfigFileEntry.EncodeValue(value);
            Assert.True(encoded.Success);
            var decoded = UnityModBase.HConfigSpace.ConfigFileEntry.DecodeValue<List<(string, bool)>>(encoded.Value);
            Assert.True(decoded.Success);
            Assert.Equal(value, decoded.Value);
        }

        [Fact]
        public void ConfigGui_AssignsEditableEditorToTupleListRows()
        {
            // 配置界面必须能给 (id, 启用) 行选择集合编辑器（展开行、每行文本框+开关），而不是只读兜底编辑器。
            var registry = ValueEditorRegistry.CreateDefault(
                new Mock<IUnityProvider>().Object, new Mock<IUnityGuiProvider>().Object,
                new Mock<IEntryStyleResource>().Object, new IValueEditor[0]);
            var binding = new Mock<IEntryBinding>();
            binding.SetupGet(b => b.ValueType).Returns(typeof(List<(string, bool)>));
            var editor = registry.GetEditor(binding.Object);
            Assert.False(editor is UnsupportedEditor, editor.GetType().Name);
        }

        private static PortraitPackage Pack(string id, string target = null) =>
            new PortraitPackage { Id = id, Target = target ?? id };

        [Fact]
        public void Scan_LoadsSelectedPackage()
        {
            CreatePackage("one", "stand_normal");
            var result = Scan(true, "one");
            Assert.Single(result.Packages);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Scan_ConflictingTargetsKeepTheLaterPackAsFallback()
        {
            CreatePackage("one", "stand_normal");
            CreatePackage("two", "stand_normal");
            // 正常流程下冲突行已在同步时被自动取消选中（ResolveSelection）；此处把两个同目标包
            // 直接传入激活，验证兜底行为：保留启用列表靠后的包，避免重复身份导致激活异常。
            var result = Scan(true, "one,two");
            Assert.Equal("two", Assert.Single(result.Packages).Value.Id);
            Assert.Contains(result.Errors, error => error.Contains("two overrides one for target stand_normal"));
            Assert.Equal("one", Assert.Single(Scan(true, "one").Packages).Value.Id);
        }

        [Fact]
        public void Scan_InvalidPngRemainsReservedButIsNotActivated()
        {
            CreatePackage("one", "stand_normal");
            File.WriteAllText(Path.Combine(directory, "page.png"), "invalid");
            var result = Scan(true, "one");
            Assert.Empty(result.Packages);
            Assert.Single(result.ReservedImages);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Scan_UsesGameRuntimePackedDimensionsForRotatedRegions()
        {
            CreatePackage("one", "stand_normal");
            File.WriteAllText(Path.Combine(directory, "one.atlas"),
                "page.png\nsize: 16,16\nfilter: Linear,Linear\npart\nbounds: 8,12,4,8\nrotate:90\n");
            var result = Scan(true, "one");
            Assert.Empty(result.Errors);
            Assert.Single(result.Packages);
        }

        [Fact]
        public void Scan_RejectsSensitiveImageDependency()
        {
            CreatePackage("one", "stand_normal");
            File.WriteAllText(Path.Combine(directory, "one.atlas"), AtlasText.Replace("page.png", "Sensitive/page.png"));
            var result = Scan(true, "one");
            Assert.Empty(result.Packages);
            Assert.Contains(Path.Combine(directory, "Sensitive", "page.png"), result.ReservedImages);
            Assert.Contains(result.Errors, error => error.Contains("Sensitive content is disabled"));
        }

        [Fact]
        public void SensitivePathCheck_UsesDirectoryBoundary()
        {
            string sensitive = Path.Combine(directory, "Sensitive");
            Assert.True(PortraitCatalog.Within(sensitive, Path.Combine(sensitive, "page.png")));
            Assert.False(PortraitCatalog.Within(sensitive, Path.Combine(directory, "SensitiveOther", "page.png")));
        }

        [Theory]
        [InlineData("../outside.png")]
        [InlineData("C:/outside.png")]
        [InlineData("page.png:stream")]
        public void Resolve_RejectsEscapingOrAbsolutePaths(string relative)
        {
            Assert.Throws<InvalidDataException>(() => PortraitCatalog.Resolve(directory, directory, relative));
        }

        [Fact]
        public void Scan_MultipageAtlasReservesAllPagesAndRejectsPackage()
        {
            CreatePackage("one", "stand_normal");
            File.WriteAllText(Path.Combine(directory, "one.atlas"), AtlasText + "\n" + AtlasText.Replace("page.png", "second.png").Replace("part", "part2"));
            var result = Scan(true, "one");
            Assert.Empty(result.Packages);
            Assert.Equal(2, result.ReservedImages.Count);
        }

        [Fact]
        public void Reload_InvalidEditCanRetainPreviousButDisableAndRemovalCannot()
        {
            CreatePackage("one", "stand_normal");
            var previous = Scan(true, "one").Packages.Values.Single();
            File.WriteAllText(previous.JsonPath, "broken json");
            Assert.Empty(Scan(true, "one").Packages);
            string sensitive = Path.Combine(directory, "Sensitive");
            Assert.True(PortraitCatalog.CanRetain(previous, directory, sensitive, false, true, Ids("one")));
            Assert.False(PortraitCatalog.CanRetain(previous, directory, sensitive, false, false, Ids("one")));
            Assert.False(PortraitCatalog.CanRetain(previous, directory, sensitive, false, true, Ids()));
            File.Delete(previous.ManifestPath);
            Assert.False(PortraitCatalog.CanRetain(previous, directory, sensitive, false, true, Ids("one")));
        }

        [Fact]
        public void Reload_FailedSwitchMayKeepPreviousUnlessSensitiveIsDisabled()
        {
            CreatePackage("one", "stand_normal");
            var previous = Scan(true, "one").Packages.Values.Single();
            string sensitive = Path.Combine(directory, "Sensitive");
            Assert.True(PortraitCatalog.CanRetain(previous, directory, sensitive, true, true, Ids("two"), true));
            previous.ImagePath = Path.Combine(sensitive, "page.png");
            Assert.False(PortraitCatalog.CanRetain(previous, directory, sensitive, false, true, Ids("two"), true));
        }

        [Fact]
        public void Merge_PreservesLinkedMeshAndOriginalBoneMotion()
        {
            var original = PortraitJson.Parse(Original);
            var skin = PortraitJson.Object(PortraitJson.Array(original["skins"])[0]);
            var slot = PortraitJson.Object(PortraitJson.Object(skin["attachments"])["body"]);
            slot["linked"] = PortraitJson.Parse("""{"type":"linkedmesh","parent":"part","path":"part"}""");
            string source = PortraitJson.Serialize(original);
            var external = PortraitJson.Parse(source);
            Attachment(external)["vertices"] = new List<object> { 0, 0, 24, 0, 0, 20 };
            string merged = PortraitMerger.Merge(source, PortraitJson.Serialize(external), PortraitCatalog.ReadAtlas(AtlasText));
            var reader = new SkeletonJson(PortraitCatalog.ReadAtlas(AtlasText));
            var oldData = reader.ReadSkeletonData(new StringReader(source));
            var newData = reader.ReadSkeletonData(new StringReader(merged));
            var linked = (MeshAttachment)newData.DefaultSkin.GetAttachment(0, "linked");
            Assert.Same(newData.DefaultSkin.GetAttachment(0, "part"), linked.ParentMesh);
            foreach (float time in new[] { 0f, 0.5f, 1f, 2f })
            {
                var oldSkeleton = Pose(oldData, time);
                var newSkeleton = Pose(newData, time);
                Assert.Equal(oldSkeleton.RootBone.Rotation, newSkeleton.RootBone.Rotation);
                Assert.Equal(oldSkeleton.RootBone.WorldX, newSkeleton.RootBone.WorldX);
                Assert.Equal(oldSkeleton.RootBone.WorldY, newSkeleton.RootBone.WorldY);
            }
        }

        [Fact]
        public void Merge_LinkedMeshWithoutSkinUsesDefaultSkin()
        {
            var original = PortraitJson.Parse(Original);
            PortraitJson.Array(original["skins"]).Add(PortraitJson.Parse("""
                {"name":"alternate","attachments":{"body":{"linked":{"type":"linkedmesh","parent":"part","path":"part"}}}}
                """));
            string source = PortraitJson.Serialize(original);
            string merged = PortraitMerger.Merge(source, source, PortraitCatalog.ReadAtlas(AtlasText));
            var data = new SkeletonJson(PortraitCatalog.ReadAtlas(AtlasText)).ReadSkeletonData(new StringReader(merged));
            var linked = (MeshAttachment)data.FindSkin("alternate").GetAttachment(0, "linked");
            Assert.Same(data.DefaultSkin.GetAttachment(0, "part"), linked.ParentMesh);
        }

        private static Skeleton Pose(SkeletonData data, float time)
        {
            var skeleton = new Skeleton(data);
            var state = new AnimationState(new AnimationStateData(data));
            state.SetAnimation(0, "stand", true);
            state.Update(time);
            state.Apply(skeleton);
            skeleton.UpdateWorldTransform();
            return skeleton;
        }

        [Fact]
        public void Hooks_TargetExpectedGameSignatures()
        {
            var prepare = typeof(nel.BetobetoManager.SvTexture).GetMethod("prepareAtlasAssets");
            Assert.Equal(new[] { "_SpAtlasAsset", "_SpDataAsset", "AMtr", "replace_json_key" },
                prepare.GetParameters().Select(p => p.Name));
            Assert.Equal(typeof(bool), typeof(nel.BetobetoManager.SvTexture).GetMethod("cleanExecute").ReturnType);
            Assert.Equal("_jsonkey", typeof(XX.SpineViewer).GetMethod("switchSkeletonJson").GetParameters().Single().Name);
            foreach (var type in new[] { typeof(PortraitResourcePatch), typeof(PortraitViewerPatch) })
                Assert.DoesNotContain(type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic),
                    method => new[] { "Prepare", "Cleanup", "TargetMethod", "TargetMethods" }.Contains(method.Name));
        }

        [Fact]
        public void Refresh_ReplayApiTargetExpectedGameSignatures()
        {
            // 热键刷新依赖游戏自身的 clearAnim 流程重建网格，并读取当前动画名与循环帧来恢复播放状态。
            var clearAnim = typeof(nel.SpineViewerNel).GetMethod("clearAnim");
            Assert.NotNull(clearAnim);
            Assert.Equal(new[] { typeof(string), typeof(int), typeof(string) }, clearAnim.GetParameters().Select(p => p.ParameterType));
            Assert.Equal(typeof(string), typeof(XX.SpineViewer).GetMethod("getBaseAnimName").ReturnType);
            var getTrack = typeof(XX.SpineViewer).GetMethod("getTrack");
            Assert.Equal(new[] { typeof(int) }, getTrack.GetParameters().Select(p => p.ParameterType));
            Assert.Equal(typeof(Spine.TrackEntry), getTrack.ReturnType);
            var getLoopFrame = typeof(XX.SpineViewer).GetMethod("getAnmLoopFrame");
            Assert.Equal(new[] { typeof(Spine.Animation) }, getLoopFrame.GetParameters().Select(p => p.ParameterType));
            Assert.Equal(typeof(int), getLoopFrame.ReturnType);
        }

        private static Dictionary<string, object> WithDeform()
        {
            var original = PortraitJson.Parse(Original);
            var animations = PortraitJson.Object(original["animations"]);
            PortraitJson.Object(animations["stand"])["attachments"] = PortraitJson.Parse("""
                {"default":{"body":{"part":{"deform":[{}, {"time":1,"vertices":[0,0,1,0,0,0]}]}}}}
                """);
            return original;
        }

        private static Dictionary<string, object> Attachment(Dictionary<string, object> data)
        {
            var skin = PortraitJson.Object(PortraitJson.Array(data["skins"])[0]);
            return PortraitJson.Object(PortraitJson.Object(PortraitJson.Object(skin["attachments"])["body"])["part"]);
        }

        private static string Merge(Dictionary<string, object> external) =>
            PortraitMerger.Merge(Original, PortraitJson.Serialize(external), PortraitCatalog.ReadAtlas(AtlasText));

        private PortraitCatalog Scan(bool enabled, string ids) =>
            PortraitCatalog.Scan(directory, Path.Combine(directory, "Sensitive"), false, enabled, Ids(ids.Split(',')));

        private static List<string> Ids(params string[] ids) => new List<string>(ids);

        private void CreatePackage(string id, string target, string subdirectory = null, string manifestId = null)
        {
            string dir = subdirectory == null ? directory : Path.Combine(directory, subdirectory);
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, id + ".portrait.json"),
                $$"""{"formatVersion":1,"id":"{{manifestId ?? id}}","target":"{{target}}","jsonKey":"{{target}}","json":"source.json","atlas":"{{id}}.atlas"}""");
            File.WriteAllText(Path.Combine(dir, id + ".atlas"), AtlasText);
            File.WriteAllText(Path.Combine(dir, "source.json"), Original);
            // Catalog validation only reads PNG metadata; decoding is tested inside Unity.
            var bytes = new byte[33];
            new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }.CopyTo(bytes, 0);
            new byte[] { 73, 72, 68, 82 }.CopyTo(bytes, 12);
            bytes[19] = bytes[23] = 16;
            File.WriteAllBytes(Path.Combine(dir, "page.png"), bytes);
        }

        public void Dispose() => Directory.Delete(directory, true);
    }
}
