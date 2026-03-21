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
        public void ConfigFileManagerBindShouldAddEntryToConfigEntriesCollection()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
            var manager = new ConfigFileManager(path)
            {
                SaveOnConfigSet = false
            };
            var tableResult = manager.Tables.CreateTable("General", string.Empty);

            Assert.True(tableResult.Success);

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
    }
}
