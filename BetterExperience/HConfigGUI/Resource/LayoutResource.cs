using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HProvider;
using System;
using System.Security;

namespace BetterExperience.HConfigGUI.Resource
{
    /// <summary>
    /// 配置界面布局尺寸计算器。
    /// 当前主要负责根据所有配置项名称计算统一标签列宽，避免不同表之间输入控件左右跳动。
    /// </summary>
    public class LayoutResource
    {
        public UnityGuiProvider UnityGui { get; }

        public LayoutResource(UnityGuiProvider unityGui)
        {
            UnityGui = unityGui;
        }

        public float GetEntryLabelWidth(SheetBinding sheet)
        {
            if (UnityGui == null || sheet == null)
                return 0f;

            float maxWidth = 0f;
            foreach (var table in sheet.Sheet)
            {
                foreach (var entry in table.Table)
                {
                    var width = UnityGui.LabelStyle.CalcSize(UnityGui.GetContent(entry.Name)).x;
                    if (width > maxWidth)
                        maxWidth = width;
                }
            }

            return maxWidth + 10f;
        }

        public float GetTableButtonWidth(SheetBinding sheet)
        {
            if (UnityGui == null || sheet == null)
                return 0f;

            float maxWidth = 0f;
            foreach (var table in sheet.Sheet)
            {
                var width = UnityGui.ButtonStyle.CalcSize(UnityGui.GetContent(table.Name)).x;
                if (width > maxWidth)
                    maxWidth = width;
            }

            return maxWidth + 60f;
        }
    }
}
