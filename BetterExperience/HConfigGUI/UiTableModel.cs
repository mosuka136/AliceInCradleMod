using BetterExperience.HConfigSpace;
using BetterExperience.HTranslatorSpace;
using System.Collections;
using System.Collections.Generic;

namespace BetterExperience.HConfigGUI
{
    public class UiTableModel : IEnumerable<UiEntryModel>
    {
        private ConfigTable _table;

        public Translator Name => _table.Name;
        public Translator Description => _table.Description;
        public List<UiEntryModel> Table { get; }

        public UiTableModel(ConfigTable table)
        {
            _table = table;
            Table = new List<UiEntryModel>();
            foreach (var entry in _table)
            {
                Table.Add(new UiEntryModel(entry));
            }
        }

        public IEnumerator<UiEntryModel> GetEnumerator()
        {
            return Table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
