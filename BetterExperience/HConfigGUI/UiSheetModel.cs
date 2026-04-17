using BetterExperience.BConfigManager;
using System.Collections;
using System.Collections.Generic;

namespace BetterExperience.HConfigGUI
{
    public class UiSheetModel : IEnumerable<UiTableModel>
    {
        public List<UiTableModel> Sheet { get; }

        public UiSheetModel()
        {
            Sheet = new List<UiTableModel>();
            foreach (var table in ConfigManager.Sheet.Values)
            {
                Sheet.Add(new UiTableModel(table));
            }
        }

        public IEnumerator<UiTableModel> GetEnumerator()
        {
            return Sheet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
