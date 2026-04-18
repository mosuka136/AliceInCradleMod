using BetterExperience.BConfigManager;
using BetterExperience.HConfigGUI;

namespace BetterExperience.Test
{
    [Collection(ConfigManagerTestCollectionDefinition.Name)]
    public class UiSheetModelTests : IDisposable
    {
        private readonly List<string> _tempFiles = new List<string>();

        public UiSheetModelTests()
        {
            ConfigManager.Initialize(CreateTempConfigPath());
        }

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
        [Trait("Category", "ProductionBugSuspected")]
        public void GetEnumerator_IEnumerable_WhenEnumerating_IteratesOverSheetItems()
        {
            var model = new UiSheetModel();

            // Act
            var items = new List<object>();
            foreach (var item in model)
            {
                items.Add(item);
            }

            // Assert
            Assert.Equal(model.Sheet.Count, items.Count);
        }
    }
}
