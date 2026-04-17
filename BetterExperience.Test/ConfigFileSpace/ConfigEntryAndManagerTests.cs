using BetterExperience.HConfigFileSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.Test.ConfigFileSpace
{
    public class ConfigEntryAndManagerTests : IDisposable
    {
        private readonly List<string> _tempFiles = new List<string>();

        private string CreateTempConfigPath()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
            _tempFiles.Add(path);
            return path;
        }

        public void Dispose()
        {
            foreach (var path in _tempFiles)
            {
                try { if (File.Exists(path)) File.Delete(path); }
                catch { }
            }
        }
        [Fact]
        public void ConfigEntryValueSetterWhenValueChangesShouldEncodeAndStoreNewValue()
        {
            var model = new ConfigFileEntryModel
            {
                Key = "Port",
                Value = "1"
            };
            var entry = new ConfigEntry<int>("General", model, 0, new Translator(english: "port"), new Translator());

            entry.Value = 2;

            Assert.Equal(2, entry.Value);
            Assert.Equal("2", entry.Entry.Value);
        }

        [Fact]
        public void ConfigEntryValueSetterWhenValueChangesShouldRaiseSettingChangedEvent()
        {
            var model = new ConfigFileEntryModel
            {
                Key = "Port",
                Value = "1"
            };
            var entry = new ConfigEntry<int>("General", model, 0, new Translator(english: "port"), new Translator());
            int receivedValue = 0;
            object receivedSender = null;
            entry.OnValueChanged += (sender, value) =>
            {
                receivedSender = sender;
                receivedValue = value;
            };

            entry.Value = 42;

            Assert.Same(entry, receivedSender);
            Assert.Equal(42, receivedValue);
        }

        [Fact]
        public void ConfigEntryValueSetterWhenValueUnchangedShouldNotRaiseSettingChangedEvent()
        {
            var model = new ConfigFileEntryModel
            {
                Key = "Port",
                Value = "5"
            };
            var entry = new ConfigEntry<int>("General", model, 0, new Translator(english: "port"), new Translator());
            bool eventRaised = false;
            entry.OnValueChanged += (_, _) => eventRaised = true;

            entry.Value = 5;

            Assert.False(eventRaised);
        }

        [Fact]
        public void ConfigEntryValueSetterWhenStringValueChangesShouldRaiseSettingChangedEvent()
        {
            var model = new ConfigFileEntryModel
            {
                Key = "Name",
                Value = "\"old\""
            };
            var entry = new ConfigEntry<string>("General", model, "default", new Translator(english: "name"), new Translator());
            string receivedValue = null;
            entry.OnValueChanged += (_, value) => receivedValue = value;

            entry.Value = "new";

            Assert.Equal("new", receivedValue);
        }

        [Fact]
        public void ConfigEntryValueSetterWhenSetMultipleTimesShouldRaiseSettingChangedForEachChange()
        {
            var model = new ConfigFileEntryModel
            {
                Key = "Counter",
                Value = "0"
            };
            var entry = new ConfigEntry<int>("General", model, 0, new Translator(english: "counter"), new Translator());
            var receivedValues = new List<int>();
            entry.OnValueChanged += (_, value) => receivedValues.Add(value);

            entry.Value = 1;
            entry.Value = 2;
            entry.Value = 3;

            Assert.Equal(new List<int> { 1, 2, 3 }, receivedValues);
        }

        [Fact]
        public void ConfigFileManagerBindShouldAddEntryToConfigEntriesCollection()
        {
            var path = CreateTempConfigPath();
            var manager = new ConfigFileManager(path)
            {
                SaveOnConfigSet = false
            };

            var createTableException = Record.Exception(() => manager.CreateTable("General", new Translator()));

            Assert.Null(createTableException);

            var entry = manager.Bind("General", "Port", 8080, new Translator(english: "port"), new Translator());

            Assert.NotNull(entry);
            Assert.Single(manager.Sheet);
            var table = Assert.IsType<ConfigTable>(manager.Sheet["General"]);
            Assert.Single(table.Table);
            Assert.Same(entry, table.Table[0]);
        }

        [Fact]
        public void ConfigFileManagerReloadWhenNoBoundEntriesShouldNotThrow()
        {
            var path = CreateTempConfigPath();
            var manager = new ConfigFileManager(path);

            var exception = Record.Exception(() => manager.Reload());

            Assert.Null(exception);
        }

        [Fact]
        public void ConfigFileManagerReloadWhenValueChangedShouldRaiseSettingChanged()
        {
            var path = CreateTempConfigPath();
            var manager = new ConfigFileManager(path) { SaveOnConfigSet = false };
            manager.CreateTable("General", new Translator());
            var entry = manager.Bind("General", "Port", 8080, new Translator(english: "port"), new Translator());
            manager.Save();

            var fileContent = File.ReadAllText(path);
            File.WriteAllText(path, fileContent.Replace("Port = 8080", "Port = 9090"));

            int receivedValue = 0;
            entry.OnValueChanged += (_, value) => receivedValue = value;

            manager.Reload();

            Assert.Equal(9090, entry.Value);
            Assert.Equal(9090, receivedValue);
        }

        [Fact]
        public void ConfigFileManagerReloadWhenValueUnchangedShouldNotRaiseSettingChanged()
        {
            var path = CreateTempConfigPath();
            var manager = new ConfigFileManager(path) { SaveOnConfigSet = false };
            manager.CreateTable("General", new Translator());
            manager.Bind("General", "Port", 8080, new Translator(english: "port"), new Translator());
            manager.Save();

            bool eventRaised = false;
            var table = Assert.IsType<ConfigTable>(manager.Sheet["General"]);
            var entry = Assert.IsType<ConfigEntry<int>>(Assert.Single(table.Table));
            entry.OnValueChanged += (_, _) => eventRaised = true;

            manager.Reload();

            Assert.False(eventRaised);
        }
    }
}
