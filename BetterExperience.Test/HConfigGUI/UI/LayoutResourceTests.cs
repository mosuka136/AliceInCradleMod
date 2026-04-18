using BetterExperience.BConfigManager;
using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace BetterExperience.Test.HConfigGUI.UI
{
    public class LayoutResourceTests : IDisposable
    {
        private readonly string _tempConfigPath;

        public LayoutResourceTests()
        {
            _tempConfigPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
            ConfigManager.Initialize(_tempConfigPath);
            LayoutResource.InvalidateLayout();
        }

        public void Dispose()
        {
            try
            {
                if (File.Exists(_tempConfigPath))
                    File.Delete(_tempConfigPath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        private UiSheetModel CreateEmptySheet()
        {
            var sheet = new UiSheetModel();
            var sheetField = typeof(UiSheetModel).GetField("<Sheet>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (sheetField != null)
            {
                sheetField.SetValue(sheet, new List<UiTableModel>());
            }
            return sheet;
        }

        private void SetCachedLabelWidth(float value)
        {
            var field = typeof(LayoutResource).GetField("_labelWidth", BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
            {
                field.SetValue(null, value);
            }
        }

    }
}
