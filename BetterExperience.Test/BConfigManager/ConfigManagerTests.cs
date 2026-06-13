using BetterExperience.BConfigManager;
using BetterExperience.HLogSpace;
using BetterExperience.HTranslatorSpace;
using System.Reflection;

namespace BetterExperience.Test.BConfigManager
{
    public class ConfigManagerTests : IDisposable
    {
        private static readonly FieldInfo OnFrameUpdateField = GetRequiredField(typeof(FrameUpdateManager), nameof(FrameUpdateManager.OnFrameUpdate));

        private readonly Dictionary<PropertyInfo, object> _originalStaticProperties;
        private readonly object _originalOnFrameUpdate;
        private readonly bool _originalEnableLog;
        private readonly List<string> _tempFiles = new List<string>();

        public ConfigManagerTests()
        {
            _originalStaticProperties = typeof(ConfigManager)
                .GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(property => property.GetIndexParameters().Length == 0 && property.SetMethod != null)
                .ToDictionary(property => property, property => property.GetValue(null));
            _originalOnFrameUpdate = OnFrameUpdateField.GetValue(null);
            _originalEnableLog = HLog.EnableLog;

            HLog.EnableLog = false;
            OnFrameUpdateField.SetValue(null, null);
        }

        [Fact]
        public void Initialize_WhenConfigFileDoesNotExist_BindsRepresentativeDefaultsAndCreatesFile()
        {
            // Arrange
            var configPath = CreateTempConfigPath();

            // Act
            ConfigManager.Initialize(configPath);

            // Assert
            Assert.NotNull(ConfigManager.Config);
            Assert.True(File.Exists(configPath));
            Assert.True(ConfigManager.Sheet.Contains("General"));
            Assert.True(ConfigManager.Sheet.Contains("Player"));
            Assert.True(ConfigManager.Sheet.Contains("Reel"));
            Assert.True(ConfigManager.Sheet.Contains("Map"));
            Assert.True(ConfigManager.Sheet.Contains("Texture"));

            Assert.True(ConfigManager.EnableBetterExperience.Value);
            Assert.Equal(LanguageType.English, ConfigManager.SetLanguage.Value);
            Assert.True(ConfigManager.EnableBeingAttacked.Value);
            Assert.False(ConfigManager.EnableNoHpDamage.Value);
            Assert.Equal(-1f, ConfigManager.SetPlayerWalkSpeed.Value);
            Assert.Equal(-1f, ConfigManager.SetReelSpeed.Value);
            Assert.True(ConfigManager.EnableWormTrap.Value);
            Assert.False(ConfigManager.EnableBetterSaveSite.Value);
            Assert.False(ConfigManager.EnableMosaic.Value);
            Assert.True(ConfigManager.EnableSensitivities.Value);
        }

        [Fact]
        public void Initialize_WhenConfigFileContainsSavedValues_RebindsRepresentativeEntriesFromFile()
        {
            // Arrange
            var configPath = CreateTempConfigPath();
            ConfigManager.Initialize(configPath);
            ConfigManager.EnableBetterExperience.Value = false;
            ConfigManager.SetPlayerWalkSpeed.Value = 2.5f;
            ConfigManager.SetReelSpeed.Value = 0.25f;
            ConfigManager.EnableWormTrap.Value = false;
            ConfigManager.EnableMosaic.Value = true;

            // Act
            ConfigManager.Initialize(configPath);

            // Assert
            Assert.False(ConfigManager.EnableBetterExperience.Value);
            Assert.Equal(2.5f, ConfigManager.SetPlayerWalkSpeed.Value);
            Assert.Equal(0.25f, ConfigManager.SetReelSpeed.Value);
            Assert.False(ConfigManager.EnableWormTrap.Value);
            Assert.True(ConfigManager.EnableMosaic.Value);
        }

        [Fact]
        public void Initialize_WhenCalled_AddsReloadConfigHandlerToFrameUpdateEvent()
        {
            // Arrange
            var configPath = CreateTempConfigPath();

            // Act
            ConfigManager.Initialize(configPath);

            // Assert
            var handler = Assert.IsAssignableFrom<MulticastDelegate>(OnFrameUpdateField.GetValue(null));
            Assert.Contains(
                handler.GetInvocationList(),
                invocation => invocation.Method == typeof(ConfigManager).GetMethod(nameof(ConfigManager.ReloadConfigOnUserOrder), BindingFlags.Static | BindingFlags.Public));
        }

        public void Dispose()
        {
            foreach (var pair in _originalStaticProperties)
            {
                pair.Key.SetValue(null, pair.Value);
            }

            OnFrameUpdateField.SetValue(null, _originalOnFrameUpdate);
            HLog.EnableLog = _originalEnableLog;

            foreach (var file in _tempFiles)
            {
                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                    if (File.Exists(file + ".tmp"))
                        File.Delete(file + ".tmp");
                }
                catch
                {
                }
            }
        }

        private string CreateTempConfigPath()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".cfg");
            _tempFiles.Add(path);
            return path;
        }

        private static FieldInfo GetRequiredField(Type type, string fieldName)
        {
            var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
            {
                throw new InvalidOperationException($"Field '{fieldName}' was not found on {type.FullName}.");
            }

            return field;
        }
    }
}
