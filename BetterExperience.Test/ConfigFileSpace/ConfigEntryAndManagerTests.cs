using BetterExperience.ConfigFileSpace;

namespace BetterExperience.Test.ConfigFileSpace
{
    public class ConfigEntryAndManagerTests
    {
        [Fact]
        public void ConfigEntryValueSetterWhenValueChangesShouldEncodeAndStoreNewValue()
        {
            var model = new ConfigFileEntryModel
            {
                Key = "Port",
                Value = "1"
            };
            var entry = new ConfigEntry<int>("General", model, 0, "port");

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
            var entry = new ConfigEntry<int>("General", model, 0, "port");
            int receivedValue = 0;
            object receivedSender = null;
            entry.SettingChanged += (sender, value) =>
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
            var entry = new ConfigEntry<int>("General", model, 0, "port");
            bool eventRaised = false;
            entry.SettingChanged += (_, _) => eventRaised = true;

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
            var entry = new ConfigEntry<string>("General", model, "default", "name");
            string receivedValue = null;
            entry.SettingChanged += (_, value) => receivedValue = value;

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
            var entry = new ConfigEntry<int>("General", model, 0, "counter");
            var receivedValues = new List<int>();
            entry.SettingChanged += (_, value) => receivedValues.Add(value);

            entry.Value = 1;
            entry.Value = 2;
            entry.Value = 3;

            Assert.Equal(new List<int> { 1, 2, 3 }, receivedValues);
        }

        [Fact]
        public void ConfigFileManagerBindShouldAddEntryToConfigEntriesCollection()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
            var manager = new ConfigFileManager(path)
            {
                SaveOnConfigSet = false
            };

            var createTableException = Record.Exception(() => manager.CreateTable("General", string.Empty));

            Assert.Null(createTableException);

            var entry = manager.Bind("General", "Port", 8080, "port");

            Assert.NotNull(entry);
            Assert.Single(manager.ConfigEntries);
            Assert.Same(entry, manager.ConfigEntries[0]);
        }

        [Fact]
        public void ConfigFileManagerReloadWhenNoBoundEntriesShouldNotThrow()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
            var manager = new ConfigFileManager(path);

            var exception = Record.Exception(() => manager.Reload());

            Assert.Null(exception);
        }

        [Fact]
        public void ConfigFileManagerReloadWhenValueChangedShouldRaiseSettingChanged()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
            var manager = new ConfigFileManager(path) { SaveOnConfigSet = false };
            manager.CreateTable("General");
            var entry = manager.Bind("General", "Port", 8080, "port");
            manager.Save();

            var fileContent = File.ReadAllText(path);
            File.WriteAllText(path, fileContent.Replace("Port = 8080", "Port = 9090"));

            int receivedValue = 0;
            entry.SettingChanged += (_, value) => receivedValue = value;

            manager.Reload();

            Assert.Equal(9090, entry.Value);
            Assert.Equal(9090, receivedValue);
        }

        [Fact]
        public void ConfigFileManagerReloadWhenValueUnchangedShouldNotRaiseSettingChanged()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
            var manager = new ConfigFileManager(path) { SaveOnConfigSet = false };
            manager.CreateTable("General");
            manager.Bind("General", "Port", 8080, "port");
            manager.Save();

            bool eventRaised = false;
            ((ConfigEntry<int>)manager.ConfigEntries[0]).SettingChanged += (_, _) => eventRaised = true;

            manager.Reload();

            Assert.False(eventRaised);
        }
    }
}
