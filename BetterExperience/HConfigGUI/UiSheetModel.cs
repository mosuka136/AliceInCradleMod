using BetterExperience.BConfigManager;
using System.Collections;
using System.Collections.Generic;

namespace BetterExperience.HConfigGUI
{
    /// <summary>
    /// 配置 GUI 使用的整页模型。
    /// 构造时从 <see cref="ConfigManager"/> 快照化当前配置表结构，后续值变化仍通过各配置项引用同步。
    /// </summary>
    public class UiSheetModel : IEnumerable<UiTableModel>
    {
        /// <summary>
        /// 按配置声明顺序排列的表模型。
        /// </summary>
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
