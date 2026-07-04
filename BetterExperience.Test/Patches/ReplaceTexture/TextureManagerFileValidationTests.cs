using BetterExperience.Patches.ReplaceTexture;
using System.Reflection;

namespace BetterExperience.Test.Patches.ReplaceTexture
{
    public class TextureManagerFileValidationTests : IDisposable
    {
        private static readonly byte[] PngSignature =
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
        };

        private static readonly FieldInfo SupportExtensionsField =
            typeof(TextureManager).GetField("_supportExtensions", BindingFlags.NonPublic | BindingFlags.Static);

        private readonly string[] _originalSupportExtensions;
        private readonly string _tempDirectory;

        public TextureManagerFileValidationTests()
        {
            Assert.NotNull(SupportExtensionsField);
            _originalSupportExtensions = (string[])SupportExtensionsField.GetValue(null);
            SupportExtensionsField.SetValue(null, new[] { ".png" });

            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void CheckFileValid_WithMissingPath_ReturnsFalse(string filePath)
        {
            // Act
            var result = TextureManager.CheckFileValid(filePath);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckFileValid_WithNonexistentFile_ReturnsFalse()
        {
            // Arrange
            var filePath = Path.Combine(_tempDirectory, "missing.png");

            // Act
            var result = TextureManager.CheckFileValid(filePath);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckFileValid_WithUnsupportedExtension_ReturnsFalse()
        {
            // Arrange
            var filePath = CreateFile("image.jpg", PngSignature);

            // Act
            var result = TextureManager.CheckFileValid(filePath);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckFileValid_WithUppercaseSupportedExtensionAndPngSignature_ReturnsTrue()
        {
            // Arrange
            var filePath = CreateFile("image.PNG", PngSignature);

            // Act
            var result = TextureManager.CheckFileValid(filePath);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckFileValid_WithFileShorterThanPngSignature_ReturnsFalse()
        {
            // Arrange
            var filePath = CreateFile("short.png", PngSignature.Take(PngSignature.Length - 1).ToArray());

            // Act
            var result = TextureManager.CheckFileValid(filePath);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckFileValid_WithIncorrectPngSignature_ReturnsFalse()
        {
            // Arrange
            var invalidSignature = (byte[])PngSignature.Clone();
            invalidSignature[invalidSignature.Length - 1] ^= 0xFF;
            var filePath = CreateFile("invalid.png", invalidSignature);

            // Act
            var result = TextureManager.CheckFileValid(filePath);

            // Assert
            Assert.False(result);
        }

        public void Dispose()
        {
            SupportExtensionsField.SetValue(null, _originalSupportExtensions);
            Directory.Delete(_tempDirectory, true);
        }

        private string CreateFile(string fileName, byte[] content)
        {
            var filePath = Path.Combine(_tempDirectory, fileName);
            File.WriteAllBytes(filePath, content);
            return filePath;
        }
    }
}
