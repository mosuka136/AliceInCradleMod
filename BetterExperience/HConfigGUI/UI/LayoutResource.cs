using System;
using System.Security;

namespace BetterExperience.HConfigGUI.UI
{
    /// <summary>
    /// 配置界面布局尺寸计算器。
    /// 当前主要负责根据所有配置项名称计算统一标签列宽，避免不同表之间输入控件左右跳动。
    /// </summary>
    public class LayoutResource
    {
        private const float LabelWidthMargin = 10f;
        // 某些 IMGUI 生命周期或安全上下文下 CalcSize 可能不可用，使用保守字符宽度作为回退。
        private const float FallbackCharacterWidth = 8f;

        private readonly ViewModel _context;

        public LayoutResource(ViewModel context)
        {
            _context = context;
        }

        public float GetLabelWidth()
        {
            if (_context == null)
                return 0f;

            if (_context.Sheet == null || _context.Sheet.Sheet.Count == 0)
                return 0f;

            float maxWidth = 0f;
            foreach (var table in _context.Sheet.Sheet)
            {
                foreach (var entry in table.Table)
                {
                    var width = GetEntryLabelWidth(entry);
                    if (width > maxWidth)
                        maxWidth = width;
                }
            }

            return maxWidth + LabelWidthMargin;
        }

        private float GetEntryLabelWidth(UiEntryModel entry)
        {
            try
            {
                return _context.UnityGuiService.LabelStyle.CalcSize(_context.UnityGuiService.GetContent(entry.Name)).x;
            }
            catch (Exception ex) when (ex is SecurityException || ex is InvalidOperationException)
            {
                var text = entry.Name == null ? string.Empty : entry.Name.ToString();
                return text.Length * FallbackCharacterWidth;
            }
        }
    }
}
