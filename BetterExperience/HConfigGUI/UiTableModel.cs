using BetterExperience.HConfigSpace;
using BetterExperience.HTranslatorSpace;
using System.Collections;
using System.Collections.Generic;

namespace BetterExperience.HConfigGUI
{
    /// <summary>
    /// 配置 GUI 使用的单表模型。
    /// 该类型只包装配置表和其中的配置项，不负责排序或过滤。
    /// </summary>
    public class UiTableModel : IEnumerable<UiEntryModel>
    {
        private ConfigTable _table;

        public Translator Name => _table.Name;
        public Translator Description => _table.Description;
        /// <summary>
        /// 表内配置项，顺序与运行时配置表保持一致。
        /// </summary>
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
