using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
using BetterExperience.HConfigGUI;
using BetterExperience.HConfigSpace;
using BetterExperience.BLogSpace;
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
            _originalEnableLog = BLog.EnableLog;

            BLog.EnableLog = false;
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

        [Fact]
        public void Initialize_WhenCalled_BindsAllPublicConfigEntryProperties()
        {
            // Arrange
            var configPath = CreateTempConfigPath();
            var properties = GetConfigEntryProperties().ToArray();

            // Act
            ConfigManager.Initialize(configPath);

            // Assert
            Assert.NotEmpty(properties);
            foreach (var property in properties)
            {
                var entry = Assert.IsAssignableFrom<IConfigEntry>(property.GetValue(null));
                Assert.Equal(property.Name, entry.Key);
                Assert.Equal(property.PropertyType.GetGenericArguments()[0], entry.ValueType);
                Assert.False(string.IsNullOrWhiteSpace(entry.TableName));
                Assert.NotNull(entry.Name);
                Assert.NotNull(entry.Description);
                Assert.True(ConfigManager.Sheet.Contains(entry.TableName), $"Missing table '{entry.TableName}' for config entry '{entry.Key}'.");
                Assert.Contains(entry, ConfigManager.Sheet[entry.TableName]);
            }
        }

        [Fact]
        public void Initialize_WhenCalled_WritesEveryBoundConfigEntryKeyToFile()
        {
            // Arrange
            var configPath = CreateTempConfigPath();

            // Act
            ConfigManager.Initialize(configPath);
            var content = File.ReadAllText(configPath);

            // Assert
            foreach (var property in GetConfigEntryProperties())
            {
                var entry = Assert.IsAssignableFrom<IConfigEntry>(property.GetValue(null));
                Assert.Contains($"{entry.Key} =", content);
            }
        }

        [Fact]
        public void Initialize_WhenSliderAttributeIsDeclared_UiMetadataMatchesAttribute()
        {
            // Arrange
            var configPath = CreateTempConfigPath();
            var sliderProperties = GetConfigEntryProperties()
                .Select(property => new
                {
                    Property = property,
                    Attribute = property.GetCustomAttribute<ConfigSliderAttribute>()
                })
                .Where(x => x.Attribute != null)
                .ToArray();

            // Act
            ConfigManager.Initialize(configPath);

            // Assert
            Assert.NotEmpty(sliderProperties);
            foreach (var item in sliderProperties)
            {
                var entry = Assert.IsAssignableFrom<IConfigEntry>(item.Property.GetValue(null));
                var metadata = Assert.IsType<UiSliderMetadata>(UiMetadataHelper.GetMetadata(entry));
                Assert.Equal(item.Attribute.Min, metadata.Min);
                Assert.Equal(item.Attribute.Max, metadata.Max);
                Assert.Equal(item.Attribute.Step, metadata.Step);
            }
        }

        public void Dispose()
        {
            foreach (var pair in _originalStaticProperties)
            {
                pair.Key.SetValue(null, pair.Value);
            }

            OnFrameUpdateField.SetValue(null, _originalOnFrameUpdate);
            BLog.EnableLog = _originalEnableLog;

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

        private static IEnumerable<PropertyInfo> GetConfigEntryProperties()
        {
            return typeof(ConfigManager)
                .GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(property => property.GetIndexParameters().Length == 0)
                .Where(property => property.PropertyType.IsGenericType)
                .Where(property => property.PropertyType.GetGenericTypeDefinition() == typeof(ConfigEntry<>))
                .OrderBy(property => property.Name);
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
