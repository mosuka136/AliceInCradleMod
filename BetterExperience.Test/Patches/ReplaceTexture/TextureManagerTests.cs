using BetterExperience.BConfigManager;
using BetterExperience.Patches.ReplaceTexture;
using HarmonyLib;
using Moq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BetterExperience.Test
{
    public class TextureManagerTests : IDisposable
    {
        private static readonly byte[] FakePngBytes =
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x01, 0x02, 0x03, 0x04
        };

        private readonly List<string> _tempDirectories = new List<string>();
        private readonly List<string> _tempFiles = new List<string>();

        public TextureManagerTests()
        {
            var configPath = CreateTempFilePath("cfg");
            ConfigManager.Initialize(configPath);
            ConfigManager.EnableHarmonyLog.Value = false;
            ResetTextureManagerState();
        }

        public void Dispose()
        {
            ResetTextureManagerState();

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

            foreach (var directory in _tempDirectories.OrderByDescending(path => path.Length))
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

        [Fact]
        public void Initialize_EnableReplaceTextureDisabled_DoesNotCreateDirectoriesOrLoadTextures()
        {
            // Arrange
            SetTextureConfig(false, true);
            var imageDirectory = CreateMissingDirectoryPath();
            var sensitiveDirectory = Path.Combine(imageDirectory, "Sensitive");

            // Act
            TextureManager.Initialize(imageDirectory, sensitiveDirectory, new[] { ".png", ".btep" });

            // Assert
            Assert.False(Directory.Exists(imageDirectory));
            Assert.False(Directory.Exists(sensitiveDirectory));
            Assert.Null(TextureManager.GetReplaceTexture("hero"));
        }

        [Fact]
        public void Initialize_EnableSensitivitiesDisabled_DoesNotThrowWhenSensitiveFilesExist()
        {
            // Arrange
            var imageDirectory = CreateTempDirectoryPath();
            var sensitiveDirectory = Path.Combine(imageDirectory, "Sensitive");
            Directory.CreateDirectory(sensitiveDirectory);
            File.WriteAllBytes(Path.Combine(imageDirectory, "normal.png"), FakePngBytes);
            File.WriteAllBytes(Path.Combine(sensitiveDirectory, "sensitive.png"), FakePngBytes);
            SetTextureConfig(true, false);

            // Act
            var exception = Record.Exception(() => TextureManager.Initialize(imageDirectory, sensitiveDirectory, new[] { ".png", ".btep" }));

            // Assert
            Assert.Null(exception);
            Assert.True(Directory.Exists(imageDirectory));
            Assert.True(Directory.Exists(sensitiveDirectory));
        }

        [Fact]
        public void Initialize_EmptyNameAndDuplicateFiles_DoesNotThrow()
        {
            // Arrange
            var imageDirectory = CreateTempDirectoryPath();
            var sensitiveDirectory = Path.Combine(imageDirectory, "Sensitive");
            Directory.CreateDirectory(sensitiveDirectory);
            File.WriteAllBytes(Path.Combine(imageDirectory, ".png"), FakePngBytes);
            File.WriteAllBytes(Path.Combine(imageDirectory, "duplicate.png"), FakePngBytes);
            File.WriteAllBytes(Path.Combine(sensitiveDirectory, "duplicate.png"), FakePngBytes);
            File.WriteAllBytes(Path.Combine(imageDirectory, "unique.png"), FakePngBytes);
            SetTextureConfig(true, true);

            // Act
            var exception = Record.Exception(() => TextureManager.Initialize(imageDirectory, sensitiveDirectory, new[] { ".png", ".btep" }));

            // Assert
            Assert.Null(exception);
            Assert.True(Directory.Exists(imageDirectory));
            Assert.True(Directory.Exists(sensitiveDirectory));
            Assert.Null(TextureManager.GetReplaceTexture("unique"));
        }

        [Fact]
        public void Initialize_SupportedExtensionsIsNull_SwallowsException()
        {
            // Arrange
            var imageDirectory = CreateTempDirectoryPath();
            var sensitiveDirectory = Path.Combine(imageDirectory, "Sensitive");
            Directory.CreateDirectory(sensitiveDirectory);
            SetTextureConfig(true, true);

            // Act
            var exception = Record.Exception(() => TextureManager.Initialize(imageDirectory, sensitiveDirectory, null));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Reload_WithStoredPathsAndConfigEnabled_CreatesDirectories()
        {
            // Arrange
            var imageDirectory = CreateMissingDirectoryPath();
            var sensitiveDirectory = Path.Combine(imageDirectory, "Sensitive");
            SetTextureConfig(false, true);
            TextureManager.Initialize(imageDirectory, sensitiveDirectory, new[] { ".png", ".btep" });
            SetTextureConfig(true, true);

            // Act
            TextureManager.Reload();

            // Assert
            Assert.True(Directory.Exists(imageDirectory));
            Assert.True(Directory.Exists(sensitiveDirectory));
        }

        [Fact]
        public void Reload_WithoutStoredPaths_DoesNotThrow()
        {
            // Arrange
            ResetTextureManagerState();

            // Act
            var exception = Record.Exception(() => TextureManager.Reload());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void CreateTexture_ImageNameIsNull_ReturnsNull()
        {
            // Act
            var texture = TextureManager.CreateTexture(null);

            // Assert
            Assert.Null(texture);
        }

        [Fact]
        public void CreateTexture_FileDoesNotExist_ReturnsNull()
        {
            // Arrange
            TextureManager.Initialize(string.Empty, string.Empty, new[] { ".png", ".btep" });
            var imagePath = Path.Combine(CreateTempDirectoryPath(), "missing.png");

            // Act
            var texture = TextureManager.CreateTexture(imagePath);

            // Assert
            Assert.Null(texture);
        }

        [Fact]
        public void CreateTexture_UnsupportedExtension_ReturnsNull()
        {
            // Arrange
            TextureManager.Initialize(string.Empty, string.Empty, new[] { ".png" });
            var imagePath = CreateImageFile("hero.jpg", FakePngBytes);

            // Act
            var texture = TextureManager.CreateTexture(imagePath);

            // Assert
            Assert.Null(texture);
        }

        [Fact]
        public void CreateTexture_PngLikeFileInTestHost_ReturnsNull()
        {
            // Arrange
            TextureManager.Initialize(string.Empty, string.Empty, new[] { ".png", ".btep" });
            var imagePath = CreateImageFile("hero.png", FakePngBytes);

            // Act
            var texture = TextureManager.CreateTexture(imagePath);

            // Assert
            Assert.Null(texture);
        }

        [Fact]
        public void GetReplaceTexture_TextureNameIsNull_ReturnsNull()
        {
            // Act
            var result = TextureManager.GetReplaceTexture(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetReplaceTexture_TextureNotLoaded_ReturnsNull()
        {
            // Act
            var result = TextureManager.GetReplaceTexture("missing");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void TryReplace_InvalidArguments_ReturnsFalse()
        {
            // Arrange
            var destinationObject = new Mock<UnityEngine.Object>();
            UnityEngine.Object destination = destinationObject.Object;

            // Act
            var nullNameResult = TextureManager.TryReplace(null, typeof(Texture2D), ref destination);
            var nullTypeResult = TextureManager.TryReplace("loaded", null, ref destination);
            destination = null;
            var nullDestinationResult = TextureManager.TryReplace("loaded", typeof(Texture2D), ref destination);

            // Assert
            Assert.False(nullNameResult);
            Assert.False(nullTypeResult);
            Assert.False(nullDestinationResult);
        }

        [Fact]
        public void TryReplace_TextureNotLoaded_ReturnsFalse()
        {
            // Arrange
            var destinationObject = new Mock<UnityEngine.Object>();
            UnityEngine.Object destination = destinationObject.Object;

            // Act
            var result = TextureManager.TryReplace("missing", typeof(Texture2D), ref destination);

            // Assert
            Assert.False(result);
            Assert.Same(destinationObject.Object, destination);
        }


        private static Texture CreatePatchedTexture(
            string name,
            FilterMode filterMode,
            TextureWrapMode wrapMode,
            TextureWrapMode wrapModeV,
            TextureWrapMode wrapModeW,
            TextureWrapMode wrapModeU,
            int anisoLevel,
            float mipMapBias,
            HideFlags hideFlags)
        {
            var texture = (Texture)RuntimeHelpers.GetUninitializedObject(typeof(Texture));
            var harmony = new Harmony(Guid.NewGuid().ToString("N"));

            try
            {
                PatchTextureProperty<Texture, string>(harmony, nameof(Texture.name));
                PatchTextureProperty<Texture, FilterMode>(harmony, nameof(Texture.filterMode));
                PatchTextureProperty<Texture, TextureWrapMode>(harmony, nameof(Texture.wrapMode));
                PatchTextureProperty<Texture, TextureWrapMode>(harmony, nameof(Texture.wrapModeV));
                PatchTextureProperty<Texture, TextureWrapMode>(harmony, nameof(Texture.wrapModeW));
                PatchTextureProperty<Texture, TextureWrapMode>(harmony, nameof(Texture.wrapModeU));
                PatchTextureProperty<Texture, int>(harmony, nameof(Texture.anisoLevel));
                PatchTextureProperty<Texture, float>(harmony, nameof(Texture.mipMapBias));
                PatchTextureProperty<UnityEngine.Object, HideFlags>(harmony, nameof(UnityEngine.Object.hideFlags));

                texture.name = name;
                texture.filterMode = filterMode;
                texture.wrapMode = wrapMode;
                texture.wrapModeV = wrapModeV;
                texture.wrapModeW = wrapModeW;
                texture.wrapModeU = wrapModeU;
                texture.anisoLevel = anisoLevel;
                texture.mipMapBias = mipMapBias;
                texture.hideFlags = hideFlags;
            }
            finally
            {
                harmony.UnpatchSelf();
            }

            return texture;
        }

        private static void PatchTextureProperty<TDeclaringType, TValue>(Harmony harmony, string propertyName)
        {
            var getter = AccessTools.PropertyGetter(typeof(TDeclaringType), propertyName);
            var setter = AccessTools.PropertySetter(typeof(TDeclaringType), propertyName);
            var storage = new ConditionalWeakTable<object, PropertyBox<TValue>>();

            PatchGetter(harmony, getter, storage, default(TValue));
            PatchSetter(harmony, setter, storage);
        }

        private static void PatchGetter<TValue>(Harmony harmony, System.Reflection.MethodInfo getter, ConditionalWeakTable<object, PropertyBox<TValue>> storage, TValue defaultValue)
        {
            var dynamicMethod = new DynamicMethod(
                "TextureGetter_" + Guid.NewGuid().ToString("N"),
                typeof(bool),
                new[] { typeof(object), getter.DeclaringType.MakeByRefType() },
                typeof(TextureManagerTests).Module,
                true);
            var il = dynamicMethod.GetILGenerator();
            var boxLocal = il.DeclareLocal(typeof(PropertyBox<TValue>));
            var hasValueLabel = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, typeof(ConditionalWeakTable<object, PropertyBox<TValue>>).GetMethod("TryGetValue"));
            il.Emit(OpCodes.Brtrue_S, hasValueLabel);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloca_S, boxLocal);
            il.Emit(OpCodes.Initobj, typeof(PropertyBox<TValue>));
            il.Emit(OpCodes.Ldloc, boxLocal);
            il.Emit(OpCodes.Ldfld, typeof(PropertyBox<TValue>).GetField(nameof(PropertyBox<TValue>.Value)));
            il.Emit(OpCodes.Stobj, getter.ReturnType);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(hasValueLabel);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloc, boxLocal);
            il.Emit(OpCodes.Ldfld, typeof(PropertyBox<TValue>).GetField(nameof(PropertyBox<TValue>.Value)));
            il.Emit(OpCodes.Stobj, getter.ReturnType);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);

            var prefix = new HarmonyMethod(dynamicMethod);
            harmony.Patch(getter, prefix: prefix);
        }

        private static void PatchSetter<TValue>(Harmony harmony, System.Reflection.MethodInfo setter, ConditionalWeakTable<object, PropertyBox<TValue>> storage)
        {
            var dynamicMethod = new DynamicMethod(
                "TextureSetter_" + Guid.NewGuid().ToString("N"),
                typeof(bool),
                new[] { typeof(object), setter.DeclaringType.MakeByRefType(), typeof(TValue) },
                typeof(TextureManagerTests).Module,
                true);
            var il = dynamicMethod.GetILGenerator();
            var createLocal = il.DeclareLocal(typeof(Func<object, PropertyBox<TValue>>));

            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc, createLocal);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloc, createLocal);
            il.Emit(OpCodes.Callvirt, typeof(ConditionalWeakTable<object, PropertyBox<TValue>>).GetMethod("GetValue"));
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Stfld, typeof(PropertyBox<TValue>).GetField(nameof(PropertyBox<TValue>.Value)));
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);

            var prefix = new HarmonyMethod(dynamicMethod);
            harmony.Patch(setter, prefix: prefix);
        }

        private sealed class PropertyBox<TValue>
        {
            public PropertyBox()
            {
                Value = default(TValue);
            }

            public TValue Value;
        }

        private static void SetTextureConfig(bool enableReplaceTexture, bool enableSensitivities)
        {
            ConfigManager.EnableReplaceTexture.Value = enableReplaceTexture;
            ConfigManager.EnableSensitivities.Value = enableSensitivities;
        }

        private void ResetTextureManagerState()
        {
            SetTextureConfig(false, true);
            TextureManager.Initialize(string.Empty, string.Empty, new[] { ".png", ".btep" });
            TextureManager.Reload();
        }

        private string CreateMissingDirectoryPath()
        {
            var path = CreateTempDirectoryPath();
            Directory.Delete(path);
            return path;
        }

        private string CreateTempDirectoryPath()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            _tempDirectories.Add(path);
            Directory.CreateDirectory(path);
            return path;
        }

        private string CreateTempFilePath(string extension)
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + "." + extension);
            _tempFiles.Add(path);
            return path;
        }

        private string CreateImageFile(string fileName, byte[] content)
        {
            var directory = CreateTempDirectoryPath();
            var path = Path.Combine(directory, fileName);
            File.WriteAllBytes(path, content);
            return path;
        }
    }
}
