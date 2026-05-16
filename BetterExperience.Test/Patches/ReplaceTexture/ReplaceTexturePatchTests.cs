using BetterExperience.BConfigManager;
using BetterExperience.HotkeyManager;
using BetterExperience.HProvider;
using BetterExperience.Patches;
using BetterExperience.Patches.ReplaceTexture;
using HarmonyLib;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using XX;
using SvTexture = nel.BetobetoManager.SvTexture;

namespace BetterExperience.Test
{
    public class ReplaceTexturePatchTests : IDisposable
    {
        private readonly List<Harmony> _harmonies = new List<Harmony>();
        private readonly List<string> _tempDirectories = new List<string>();
        private readonly List<string> _tempFiles = new List<string>();

        public ReplaceTexturePatchTests()
        {
            ResetReplaceTexturePatchState();
        }

        [Fact]
        public void Initialize_FirstCallThenSecondCall_SubscribesFrameUpdateOnlyOnce()
        {
            // Arrange
            InitializeConfig();

            // Act
            HPatches.ReplaceTexturePatch.Initialize();
            HPatches.ReplaceTexturePatch.Initialize();

            // Assert
            var handler = (MulticastDelegate)GetStaticFieldValue(typeof(FrameUpdateManager), nameof(FrameUpdateManager.OnFrameUpdate));

            Assert.NotNull(handler);
            Assert.Single(handler.GetInvocationList());
        }

        [Fact]
        public void Update_EnableReplaceTextureDisabled_DoesNotRestoreOrReloadTextures()
        {
            // Arrange
            InitializeConfig();
            ConfigManager.EnableReplaceTexture.Value = false;
            var imageDirectory = CreateMissingDirectoryPath();
            var sensitiveDirectory = Path.Combine(imageDirectory, "Sensitive");
            TextureManager.Initialize(imageDirectory, sensitiveDirectory, new[] { ".png" });

            // Act
            HPatches.ReplaceTexturePatch.Update();

            // Assert
            Assert.False(Directory.Exists(imageDirectory));
            Assert.False(Directory.Exists(sensitiveDirectory));
        }

        [Fact]
        public void Update_HotkeyNotPressed_DoesNotRestoreOrReloadTextures()
        {
            // Arrange
            InitializeConfig();
            ConfigManager.EnableReplaceTexture.Value = false;
            var imageDirectory = CreateMissingDirectoryPath();
            var sensitiveDirectory = Path.Combine(imageDirectory, "Sensitive");
            TextureManager.Initialize(imageDirectory, sensitiveDirectory, new[] { ".png" });
            ConfigManager.EnableReplaceTexture.Value = true;
            ConfigManager.FlushTextureHotkey.Value = CreateHotkey(false);

            // Act
            HPatches.ReplaceTexturePatch.Update();

            // Assert
            Assert.False(Directory.Exists(imageDirectory));
            Assert.False(Directory.Exists(sensitiveDirectory));
        }

        [Fact]
        public void Update_HotkeyPressed_InvokesRestoreReloadAndPictureReplacement()
        {
            // Arrange
            InitializeConfig();
            ConfigManager.EnableReplaceTexture.Value = false;
            var imageDirectory = CreateMissingDirectoryPath();
            var sensitiveDirectory = Path.Combine(imageDirectory, "Sensitive");
            TextureManager.Initialize(imageDirectory, sensitiveDirectory, new[] { ".png" });
            ConfigManager.EnableReplaceTexture.Value = true;
            ConfigManager.FlushTextureHotkey.Value = CreateHotkey(true);
            var assetKey = "asset/" + Guid.NewGuid().ToString("N");
            HPatches.ReplaceTexturePatch.LoadContainerOneImagePostfix(CreateUninitialized<MTIOneImage>(), assetKey, "load", "image");

            // Act
            HPatches.ReplaceTexturePatch.Update();

            // Assert
            Assert.True(Directory.Exists(imageDirectory));
            Assert.True(Directory.Exists(sensitiveDirectory));
            Assert.True(GetTrackedPictures().ContainsKey(assetKey));
        }

        [Fact]
        public void Update_RestoreOriginalTextureThrows_SwallowsException()
        {
            // Arrange
            InitializeConfig();
            ConfigManager.EnableReplaceTexture.Value = false;
            var imageDirectory = CreateMissingDirectoryPath();
            var sensitiveDirectory = Path.Combine(imageDirectory, "Sensitive");
            TextureManager.Initialize(imageDirectory, sensitiveDirectory, new[] { ".png" });
            ConfigManager.EnableReplaceTexture.Value = true;
            ConfigManager.FlushTextureHotkey.Value = CreateHotkey(true);
            SeedFailingOriginalSpineTexture();

            // Act
            var exception = Record.Exception(() => HPatches.ReplaceTexturePatch.Update());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void CleanExecutePostfix_EnableReplaceTextureDisabled_DoesNotInvokeSpineReplacement()
        {
            // Arrange
            InitializeConfig();
            ConfigManager.EnableReplaceTexture.Value = false;

            // Act
            HPatches.ReplaceTexturePatch.CleanExecutePostfix(null);

            // Assert
            Assert.Empty(GetTrackedSpineTextures());
        }

        [Fact]
        public void CleanExecutePostfix_InstanceIsNull_SwallowsException()
        {
            // Arrange
            InitializeConfig();
            ConfigManager.EnableReplaceTexture.Value = true;

            // Act
            var exception = Record.Exception(() => HPatches.ReplaceTexturePatch.CleanExecutePostfix(null));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void LoadContainerOneImagePostfix_EnableReplaceTextureDisabled_DoesNotInvokeTryReplace()
        {
            // Arrange
            InitializeConfig();
            ConfigManager.EnableReplaceTexture.Value = false;
            var assetKey = "asset/disabled";

            // Act
            HPatches.ReplaceTexturePatch.LoadContainerOneImagePostfix(null, assetKey, "load", "image");

            // Assert
            Assert.False(GetTrackedPictures().ContainsKey(assetKey));
        }

        [Fact]
        public void LoadContainerOneImagePostfix_EnableReplaceTextureEnabled_InvokesTryReplace()
        {
            // Arrange
            InitializeConfig();
            ConfigManager.EnableReplaceTexture.Value = true;
            var assetKey = "asset/enabled";
            var mti = CreateUninitialized<MTIOneImage>();

            // Act
            HPatches.ReplaceTexturePatch.LoadContainerOneImagePostfix(mti, assetKey, "load", "image");

            // Assert
            Assert.True(GetTrackedPictures().TryGetValue(assetKey, out var tracked));
            Assert.Same(mti, tracked);
        }

        [Fact]
        public void LoadContainerOneImagePostfix_AssetKeyIsNull_SwallowsException()
        {
            // Arrange
            InitializeConfig();
            ConfigManager.EnableReplaceTexture.Value = true;

            // Act
            var exception = Record.Exception(() => HPatches.ReplaceTexturePatch.LoadContainerOneImagePostfix(null, null, "load", "image"));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void TryReplace_MtiIsNull_DoesNotThrow()
        {
            // Arrange
            InitializeConfig();

            // Act
            var exception = Record.Exception(() => HPatches.ReplaceTexturePatch.TryReplace((MTIOneImage)null, "asset/texture"));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void TryReplace_AssetKeyDoesNotContainSeparator_DoesNotThrow()
        {
            // Arrange
            InitializeConfig();
            var mti = CreateUninitialized<MTIOneImage>();

            // Act
            var exception = Record.Exception(() => HPatches.ReplaceTexturePatch.TryReplace(mti, "invalid"));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void TryReplace_ReplacementTextureDoesNotExist_DoesNotThrow()
        {
            // Arrange
            InitializeConfig();
            var mti = CreateUninitialized<MTIOneImage>();

            // Act
            var exception = Record.Exception(() => HPatches.ReplaceTexturePatch.TryReplace(mti, "asset/missing-texture"));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void TryReplace_ReplacementTextureExists_DoesNotThrow()
        {
            // Arrange
            InitializeConfig();
            var imageDirectory = CreateTempDirectory();
            var sensitiveDirectory = CreateTempDirectory();
            File.WriteAllBytes(Path.Combine(imageDirectory, "sample.png"), CreatePngBytes());
            TextureManager.Initialize(imageDirectory, sensitiveDirectory, new[] { ".png" });
            var mti = CreateUninitialized<MTIOneImage>();

            // Act
            var exception = Record.Exception(() => HPatches.ReplaceTexturePatch.TryReplace(mti, "asset/sample"));

            // Assert
            Assert.Null(exception);
        }

        public void Dispose()
        {
            ResetReplaceTexturePatchState();

            foreach (var harmony in _harmonies)
            {
                harmony.UnpatchSelf();
            }

            foreach (var file in _tempFiles)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                }
            }

            foreach (var directory in _tempDirectories)
            {
                try
                {
                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, true);
                    }
                }
                catch
                {
                }
            }
        }

        private static T CreateUninitialized<T>() where T : class
        {
            return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        }

        private static Hotkey CreateHotkey(bool wasPressedThisFrame)
        {
            var trigger = new Mock<IHotkeyTrigger>();
            trigger.Setup(x => x.WasPressedThisFrame()).Returns(wasPressedThisFrame);
            return new Hotkey(new UnityProvider(), trigger.Object);
        }

        private void InitializeConfig()
        {
            var configPath = CreateTempFilePath("cfg");
            ConfigManager.Initialize(configPath);
            ConfigManager.EnableHLog.Value = false;
            ConfigManager.EnableReplaceTexture.Value = true;
            ConfigManager.EnableSensitivities.Value = true;
            ConfigManager.FlushTextureHotkey.Value = new Hotkey(new UnityProvider());
        }

        private static void ResetReplaceTexturePatchState()
        {
            SetStaticField(typeof(HPatches.ReplaceTexturePatch), "_initialized", false);
            ClearStaticCollection(typeof(HPatches.ReplaceTexturePatch), "_spineTexture");
            ClearStaticCollection(typeof(HPatches.ReplaceTexturePatch), "_pictureTexture");
            ClearStaticCollection(typeof(HPatches.ReplaceTexturePatch), "_originalPictureTexture");
            ClearStaticCollection(typeof(HPatches.ReplaceTexturePatch), "_originalSpineTexture");
            SetStaticField(typeof(FrameUpdateManager), nameof(FrameUpdateManager.OnFrameUpdate), null);
            SetStaticField(typeof(TextureManager), "_imagePath", null);
            SetStaticField(typeof(TextureManager), "_sensitiveImagePath", null);
            SetStaticField(typeof(TextureManager), "_supportExtensions", null);
            ClearStaticCollection(typeof(TextureManager), "_imageInfos");
        }

        private static Dictionary<string, MTIOneImage> GetTrackedPictures()
        {
            return (Dictionary<string, MTIOneImage>)GetStaticFieldValue(typeof(HPatches.ReplaceTexturePatch), "_pictureTexture");
        }

        private static List<SvTexture> GetTrackedSpineTextures()
        {
            return (List<SvTexture>)GetStaticFieldValue(typeof(HPatches.ReplaceTexturePatch), "_spineTexture");
        }

        private static object GetStaticFieldValue(Type type, string fieldName)
        {
            var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            Assert.NotNull(field);
            return field.GetValue(null);
        }

        private static void SeedFailingOriginalSpineTexture()
        {
            var originalSpineTextures = (Dictionary<SvTexture, Texture>)GetStaticFieldValue(typeof(HPatches.ReplaceTexturePatch), "_originalSpineTexture");
            var svTexture = CreateUninitialized<SvTexture>();
            var mtiImage = CreateUninitialized<MTIOneImage>();
            var image = CreateUninitialized<MImage>();
            var currentTexture = CreateUninitialized<Texture2D>();
            var replacementTexture = CreateUninitialized<Texture2D>();

            SetMemberValue(image, "Tx_", currentTexture);
            SetMemberValue(mtiImage, "LImage_", image);
            SetMemberValue(svTexture, "MtiImage0", mtiImage);

            originalSpineTextures[svTexture] = replacementTexture;
        }

        private static void SetMemberValue(object instance, string memberName, object value)
        {
            ArgumentNullException.ThrowIfNull(instance);

            var type = instance.GetType();
            var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property?.SetMethod != null)
            {
                property.SetValue(instance, value);
                return;
            }

            var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? type.GetField($"<{memberName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(field);
            field.SetValue(instance, value);
        }

        private static void SetStaticField(Type type, string fieldName, object value)
        {
            var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            Assert.NotNull(field);
            field.SetValue(null, value);
        }

        private static void ClearStaticCollection(Type type, string fieldName)
        {
            var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            Assert.NotNull(field);

            var value = field.GetValue(null);
            value?.GetType().GetMethod("Clear", Type.EmptyTypes)?.Invoke(value, null);
        }

        private string CreateTempDirectory()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            _tempDirectories.Add(directory);
            return directory;
        }

        private string CreateMissingDirectoryPath()
        {
            var directory = CreateTempDirectory();
            Directory.Delete(directory);
            return directory;
        }

        private string CreateTempFilePath(string extension)
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + "." + extension);
            _tempFiles.Add(path);
            return path;
        }

        private static byte[] CreatePngBytes()
        {
            return Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9WlH0X8AAAAASUVORK5CYII=");
        }

        private static class PatchTracker
        {
            public static int AddOnFrameUpdateCalls { get; private set; }

            public static int ReloadCalls { get; private set; }

            public static bool ThrowWhenRestoringOriginalTexture { get; set; }

            public static int RestoreOriginalTextureCalls { get; private set; }

            public static int TryReplacePictureCalls { get; private set; }

            public static int TryReplaceSpineCalls { get; private set; }

            public static void Reset()
            {
                AddOnFrameUpdateCalls = 0;
                ReloadCalls = 0;
                RestoreOriginalTextureCalls = 0;
                ThrowWhenRestoringOriginalTexture = false;
                TryReplacePictureCalls = 0;
                TryReplaceSpineCalls = 0;
            }

            public static void AddOnFrameUpdatePostfix()
            {
                AddOnFrameUpdateCalls++;
            }

            public static void ReloadPostfix()
            {
                ReloadCalls++;
            }

            public static void RestoreOriginalTexturePrefix()
            {
                RestoreOriginalTextureCalls++;
                if (ThrowWhenRestoringOriginalTexture)
                {
                    throw new InvalidOperationException("boom");
                }
            }

            public static void TryReplacePicturePostfix(MTIOneImage mti, string asset_key)
            {
                TryReplacePictureCalls++;
            }

            public static void TryReplaceSpinePostfix(SvTexture svTexture, Texture image)
            {
                TryReplaceSpineCalls++;
            }
        }
    }
}
