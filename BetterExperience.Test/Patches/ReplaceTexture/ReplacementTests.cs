using BetterExperience.Patches.ReplaceTexture;
using Spine;

namespace BetterExperience.Test.Patches.ReplaceTexture
{
    public sealed class ReplacementTests : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(),
            "be-replacement-" + Guid.NewGuid().ToString("N"));
        private const string AtlasText = "page.png\nsize: 16,16\nfilter: Linear,Linear\npart\nbounds: 0,0,16,16\n";
        private const string Original = """
            {"skeleton":{"spine":"4.1.24","hash":"test"},
            "bones":[{"name":"root"},{"name":"arm","parent":"root"}],
            "slots":[{"name":"body","bone":"arm","attachment":"part"}],
            "skins":[{"name":"default","attachments":{"body":{"part":{
            "type":"mesh","uvs":[0,0,1,0,0,1],"triangles":[0,1,2],
            "vertices":[0,0,16,0,0,16],"hull":3}}}}],
            "animations":{"stand":{"bones":{"arm":{"rotate":[{}, {"time":1,"value":10}]}}}}}
            """;

        public ReplacementTests() => Directory.CreateDirectory(directory);

        [Fact]
        public void ResourcesPatch_TargetsNonGenericStringOverload()
        {
            var method = global::BetterExperience.Patches.ReplacementResourcesPatch.TargetMethod();

            Assert.Equal(nameof(UnityEngine.Resources.Load), method.Name);
            Assert.False(method.IsGenericMethod);
            Assert.Equal(new[] { typeof(string) }, method.GetParameters().Select(parameter => parameter.ParameterType));
        }

        [Fact]
        public void Runtime_CapturesMergedSkinStackInOrder()
        {
            var data = new SkeletonData();
            data.Skins.Add(new Skin("default"));
            data.Skins.Add(new Skin("arm2A"));
            data.Skins.Add(new Skin("armGas_A"));
            var skeleton = new Skeleton(data);
            skeleton.SetSkin("arm2A");
            skeleton.MergeSkin("armGas_A");

            Assert.Equal(new[] { "arm2A", "armGas_A" },
                ReplacementRuntime.CaptureSkinNames(skeleton.SkinList));
        }

        [Fact]
        public void Catalog_DiscoversMultiTargetPacksAndLayersInConfiguredOrder()
        {
            WritePng("one.png");
            WriteManifest("low", """
                {"formatVersion":2,"id":"low","targets":[
                {"type":"texture","loader":"mti","assetKey":"SpineAnimEv/manpu__manpu","imageKey":"Manpu.png","image":"one.png"},
                {"type":"texture","loader":"resources","path":"UI/Icon","objectType":"Sprite","image":"one.png"}]}
                """);
            WriteManifest("high", """
                {"formatVersion":2,"id":"high","targets":[
                {"type":"texture","loader":"mti","assetKey":"SpineAnimEv/manpu__manpu","imageKey":"Manpu.png","image":"one.png"}]}
                """);

            var catalog = ReplacementCatalog.Discover(directory, Path.Combine(directory, "Sensitive"), true);

            Assert.Empty(catalog.Errors);
            Assert.Equal(2, catalog.Packages.Count);
            string identity = "texture\nmti\nSpineAnimEv/manpu__manpu\nManpu.png";
            Assert.Equal(new[] { "high", "low" }, catalog.Layers(identity, new[] { "high", "low" })
                .Select(target => target.PackageId));
        }

        [Fact]
        public void Catalog_RejectsDuplicateTargetsInsideOnePack()
        {
            WritePng("one.png");
            WriteManifest("duplicate", """
                {"formatVersion":2,"id":"duplicate","targets":[
                {"type":"texture","loader":"resources","path":"UI/Icon","objectType":"Texture2D","image":"one.png"},
                {"type":"texture","loader":"resources","path":"UI/Icon","objectType":"Texture2D","image":"one.png"}]}
                """);

            var catalog = ReplacementCatalog.Discover(directory, Path.Combine(directory, "Sensitive"), true);

            Assert.Empty(catalog.Packages);
            Assert.Contains(catalog.Errors, error => error.Contains("Duplicate target in package"));
        }

        [Fact]
        public void Catalog_KeepsValidTargetsWhenAnotherTargetIsDamaged()
        {
            WritePng("one.png");
            WriteManifest("partial", """
                {"formatVersion":2,"id":"partial","targets":[
                {"type":"texture","loader":"resources","path":"UI/Good","objectType":"Texture2D","image":"one.png"},
                {"type":"texture","loader":"resources","path":"UI/Broken","objectType":"Texture2D","image":"missing.png"}]}
                """);

            var catalog = ReplacementCatalog.Discover(directory, Path.Combine(directory, "Sensitive"), true);
            var package = Assert.Single(catalog.Packages);

            Assert.Single(package.Targets);
            Assert.Equal("UI/Good", package.Targets[0].ResourcePath);
            Assert.Contains("texture\nresources\nUI/Broken\nTexture2D", package.InvalidTargetIdentities);
            Assert.Single(catalog.Errors);
        }

        [Fact]
        public void Catalog_TreatsJsonVariantsAsIndependentSpineTargets()
        {
            WriteManifest("variants", """
                {"formatVersion":2,"id":"variants","targets":[
                {"type":"spine","key":"event","jsonKey":"normal","display":{"offsetX":1}},
                {"type":"spine","key":"event","jsonKey":"damaged","display":{"offsetX":2}}]}
                """);

            var catalog = ReplacementCatalog.Discover(directory, Path.Combine(directory, "Sensitive"), true);
            var package = Assert.Single(catalog.Packages);

            Assert.Empty(catalog.Errors);
            Assert.Equal(2, package.Targets.Count);
            Assert.NotEqual(package.Targets[0].Identity, package.Targets[1].Identity);
        }

        [Fact]
        public void Catalog_RejectsSkinsAndAttachmentsInOneLayer()
        {
            File.WriteAllText(Path.Combine(directory, "source.json"), Original);
            WriteManifest("ambiguous", """
                {"formatVersion":2,"id":"ambiguous","targets":[
                {"type":"spine","key":"stand_normal","jsonKey":"stand_normal",
                 "spine":{"json":"source.json","replace":["skins","attachments"]}}]}
                """);

            var catalog = ReplacementCatalog.Discover(directory, Path.Combine(directory, "Sensitive"), true);
            var package = Assert.Single(catalog.Packages);

            Assert.Empty(package.Targets);
            Assert.Contains(catalog.Errors, error => error.Contains("cannot be selected"));
        }

        [Fact]
        public void Catalog_RejectsDependenciesCrossingSensitiveBoundary()
        {
            Directory.CreateDirectory(Path.Combine(directory, "Sensitive"));
            WritePng("normal.png");
            File.WriteAllText(Path.Combine(directory, "Sensitive", "bad.replacement.json"), """
                {"formatVersion":2,"id":"bad","targets":[
                {"type":"texture","loader":"resources","path":"UI/Icon","objectType":"Texture2D","image":"../normal.png"}]}
                """);

            var catalog = ReplacementCatalog.Discover(directory, Path.Combine(directory, "Sensitive"), true);

            var package = Assert.Single(catalog.Packages);
            Assert.Empty(package.Targets);
            Assert.Contains("texture\nresources\nUI/Icon\nTexture2D", package.InvalidTargetIdentities);
            Assert.Contains(catalog.Errors, error => error.Contains("same normal or Sensitive tree"));
        }

        [Fact]
        public void Composer_RemapsWeightedVerticesFromAttachmentSourceBones()
        {
            string external = """
                {"skeleton":{"spine":"4.1.24"},
                "bones":[{"name":"other"},{"name":"root"},{"name":"arm","parent":"root"}],
                "slots":[{"name":"body","bone":"arm","attachment":"part"}],
                "skins":[{"name":"default","attachments":{"body":{"part":{
                "type":"mesh","uvs":[0,0,1,0,0,1],"triangles":[0,1,2],
                "vertices":[1,2,0,0,1,1,2,16,0,1,1,2,0,16,1],"hull":3}}}}],
                "animations":{}}
                """;
            string jsonPath = Path.Combine(directory, "attachment.json");
            File.WriteAllText(jsonPath, external);
            var layer = SpineLayer(jsonPath, "attachments");

            var result = SpineComposer.Compose(Original, new[] { layer }, Atlas());
            var mesh = Attachment(PortraitJson.Parse(result.Json), "default", "body", "part");
            var vertices = PortraitJson.Array(mesh["vertices"]);

            Assert.Equal(1, PortraitJson.Integer(vertices[1]));
            Assert.Equal(1, PortraitJson.Integer(vertices[6]));
            Assert.Equal(1, PortraitJson.Integer(vertices[11]));
            Assert.NotNull(new SkeletonJson(Atlas()).ReadSkeletonData(new StringReader(result.Json)));
        }

        [Fact]
        public void Composer_AllowsCompletelyCustomSkeletonThroughCompatibilityAliases()
        {
            string custom = """
                {"skeleton":{"spine":"4.1.99"},
                "bones":[{"name":"root"},{"name":"pelvis","parent":"root"}],
                "slots":[{"name":"body","bone":"pelvis","attachment":"part"}],
                "skins":[{"name":"base","attachments":{"body":{"part":{
                "type":"mesh","uvs":[0,0,1,0,0,1],"triangles":[0,1,2],
                "vertices":[0,0,16,0,0,16],"hull":3}}}}],
                "animations":{"idle":{"bones":{"pelvis":{"rotate":[{}, {"time":1,"value":5}]}}}}}
                """;
            string jsonPath = Path.Combine(directory, "custom.json");
            File.WriteAllText(jsonPath, custom);
            var layer = SpineLayer(jsonPath, "all");
            layer.AnimationMap.Add("stand", "idle");
            layer.SkinMap.Add("default", "base");
            layer.BoneMap.Add("arm", "pelvis");
            layer.Display.ScaleMultiplier = 1.25f;

            var result = SpineComposer.Compose(Original, new[] { layer }, Atlas());
            var parsed = PortraitJson.Parse(result.Json);
            var data = new SkeletonJson(Atlas()).ReadSkeletonData(new StringReader(result.Json));

            Assert.True(PortraitJson.Object(parsed["animations"]).ContainsKey("stand"));
            Assert.Contains(PortraitJson.Array(parsed["skins"]), skin =>
                PortraitJson.String(PortraitJson.Object(skin), "name") == "default");
            Assert.NotNull(data.FindAnimation("stand"));
            Assert.NotNull(data.FindSkin("default"));
            Assert.Equal("pelvis", result.BoneMap["arm"]);
            Assert.Equal(1.25f, result.Display.ScaleMultiplier);
        }

        [Fact]
        public void Composer_RejectsMissingCompatibilityFallback()
        {
            string jsonPath = Path.Combine(directory, "animations.json");
            var custom = PortraitJson.Parse(Original);
            custom["animations"] = new Dictionary<string, object>
            {
                ["idle"] = new Dictionary<string, object>()
            };
            File.WriteAllText(jsonPath, PortraitJson.Serialize(custom));
            var layer = SpineLayer(jsonPath, "animations");

            Assert.Throws<InvalidDataException>(() => SpineComposer.Compose(Original, new[] { layer }, Atlas()));
        }

        [Fact]
        public void Composer_AttachmentLayersAccumulateAndLaterKeysWin()
        {
            var first = PortraitJson.Parse(Original);
            Attachment(first, "default", "body", "part")["vertices"] =
                new List<object> { 0, 0, 24, 0, 0, 16 };
            string firstPath = Path.Combine(directory, "first.json");
            File.WriteAllText(firstPath, PortraitJson.Serialize(first));

            var second = PortraitJson.Parse(Original);
            var slot = PortraitJson.Object(PortraitJson.Object(
                PortraitJson.Array(second["skins"]).Select(PortraitJson.Object).Single()["attachments"])["body"]);
            slot.Clear();
            slot["extra"] = PortraitJson.Parse("""
                {"value":{"type":"region","path":"part","width":16,"height":16}}
                """)["value"];
            string secondPath = Path.Combine(directory, "second.json");
            File.WriteAllText(secondPath, PortraitJson.Serialize(second));

            var result = PortraitJson.Parse(SpineComposer.Compose(Original,
                new[] { SpineLayer(firstPath, "attachments"), SpineLayer(secondPath, "attachments") }, Atlas()).Json);

            Assert.Equal(24, PortraitJson.Integer(PortraitJson.Array(
                Attachment(result, "default", "body", "part")["vertices"])[2]));
            Assert.Equal("part", PortraitJson.String(Attachment(result, "default", "body", "extra"), "path"));
        }

        [Fact]
        public void Composer_RejectsLegacyDirtWithoutCompatibleRegions()
        {
            var layer = SpineLayer(null);
            layer.Dirt = "legacy";
            Assert.Throws<InvalidDataException>(() => SpineComposer.Compose(Original, new[] { layer }, Atlas()));
        }

        [Fact]
        public void EnabledRows_PreserveLayeringOrderAndDoNotCancelSameTarget()
        {
            var packages = new[]
            {
                new ReplacementPackage { Id = "first" },
                new ReplacementPackage { Id = "second" }
            };
            var current = new List<(string, bool)> { ("second", true), ("first", true) };

            Assert.Equal(current, ReplacementCatalog.SyncRows(packages, current, true));
            Assert.Equal(new[] { "second", "first" }, ReplacementCatalog.EnabledIds(current));
        }

        [Fact]
        public void SyncRows_DropsRowsOfDeletedPacksAndKeepsHiddenOrBrokenOnes()
        {
            WritePng("one.png");
            WriteManifest("normal", """
                {"formatVersion":2,"id":"normal","targets":[
                {"type":"texture","loader":"resources","path":"UI/Icon","objectType":"Texture2D","image":"one.png"}]}
                """);
            // 能读出 id 但解析失败的清单：文件还在磁盘上，配置行必须保留。
            WriteManifest("broken", """{"formatVersion":1,"id":"broken","targets":[]}""");
            // 敏感内容关闭时敏感清单不会被解析（连依赖都不检查），但 id 仍然登记，行同样要保留。
            Directory.CreateDirectory(Path.Combine(directory, "Sensitive"));
            File.WriteAllText(Path.Combine(directory, "Sensitive", "secret.replacement.json"),
                """
                {"formatVersion":2,"id":"secret","targets":[
                {"type":"texture","loader":"resources","path":"UI/S","objectType":"Texture2D","image":"missing.png"}]}
                """);

            var catalog = ReplacementCatalog.Discover(directory, Path.Combine(directory, "Sensitive"), false);

            Assert.True(catalog.DeclaredIdsComplete);
            Assert.Equal(new[] { "broken", "normal", "secret" },
                catalog.DeclaredIds.OrderBy(id => id, StringComparer.Ordinal));
            Assert.NotEmpty(catalog.Errors);

            // 只有磁盘上再也找不到清单的 gone 行被删除，其余行连同用户开关原位保留。
            var current = new List<(string, bool)> { ("normal", true), ("broken", true), ("secret", true), ("gone", true) };
            Assert.Equal(new List<(string, bool)> { ("normal", true), ("broken", true), ("secret", true) },
                catalog.SyncRows(current));
        }

        [Fact]
        public void SyncRows_KeepsUnknownRowsWhenAManifestIdCannotBeRead()
        {
            WriteManifest("anonymous", """{"formatVersion":2,"targets":[]}""");

            var catalog = ReplacementCatalog.Discover(directory, Path.Combine(directory, "Sensitive"), true);

            Assert.False(catalog.DeclaredIdsComplete);
            Assert.Empty(catalog.DeclaredIds);
            Assert.NotEmpty(catalog.Errors);
            // 读不出 id 时无法判断未知行是否属于该清单，本轮不删除任何行。
            var current = new List<(string, bool)> { ("maybe-anonymous", true) };
            Assert.Equal(current, catalog.SyncRows(current));
        }

        [Fact]
        public void HookTargets_ExposeRequiredResourceAndCompatibilityEntrypoints()
        {
            Assert.NotNull(typeof(UnityEngine.Resources).GetMethod("Load", new[] { typeof(string), typeof(Type) }));
            Assert.NotNull(typeof(XX.SpineViewer).GetMethod("FindBone", new[] { typeof(string) }));
            Assert.NotNull(typeof(XX.SpineViewer).GetMethod("existBone", new[] { typeof(string) }));
            Assert.NotNull(typeof(nel.UIPictureBodySpine).GetProperty("scale")?.GetMethod);
            Assert.NotNull(typeof(Spine.Unity.SkeletonRenderer).GetField("useClipping",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic));
        }

        private ReplacementTarget SpineLayer(string jsonPath, params string[] sections)
        {
            var target = new ReplacementTarget
            {
                PackageId = "test",
                Type = "spine",
                SpineKey = "stand_normal",
                JsonKey = "stand_normal",
                JsonPath = jsonPath
            };
            foreach (string section in sections) target.Sections.Add(section);
            return target;
        }

        private static Atlas Atlas() => PortraitCatalog.ReadAtlas(AtlasText);

        private static Dictionary<string, object> Attachment(Dictionary<string, object> json,
            string skinName, string slotName, string attachmentName)
        {
            var skin = PortraitJson.Array(json["skins"]).Select(PortraitJson.Object)
                .Single(item => PortraitJson.String(item, "name") == skinName);
            return PortraitJson.Object(PortraitJson.Object(PortraitJson.Object(skin["attachments"])[slotName])[attachmentName]);
        }

        private void WriteManifest(string name, string text) =>
            File.WriteAllText(Path.Combine(directory, name + ".replacement.json"), text);

        private void WritePng(string name)
        {
            var bytes = new byte[33];
            new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }.CopyTo(bytes, 0);
            new byte[] { 73, 72, 68, 82 }.CopyTo(bytes, 12);
            bytes[19] = bytes[23] = 16;
            File.WriteAllBytes(Path.Combine(directory, name), bytes);
        }

        public void Dispose() => Directory.Delete(directory, true);
    }
}
