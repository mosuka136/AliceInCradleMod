using BetterExperience.HAdapter;
using System;
using System.Security;

namespace BetterExperience.HConfigGUI.UI
{
    public static class LayoutResource
    {
        private const float LabelWidthMargin = 10f;
        private const float FallbackCharacterWidth = 8f;

        private static float _labelWidth = -1f;

        public static float GetLabelWidth(UiSheetModel sheet)
        {
            if (_labelWidth > 0f)
                return _labelWidth;

            if (sheet == null || sheet.Sheet.Count == 0)
                return 0f;

            float maxWidth = 0f;
            foreach (var table in sheet.Sheet)
            {
                foreach (var entry in table.Table)
                {
                    var width = GetEntryLabelWidth(entry);
                    if (width > maxWidth)
                        maxWidth = width;
                }
            }

            _labelWidth = maxWidth + LabelWidthMargin;
            return _labelWidth;
        }

        public static void InvalidateLayout()
        {
            _labelWidth = -1f;
        }

        private static float GetEntryLabelWidth(UiEntryModel entry)
        {
            try
            {
                return GuiStyleAdapter.LabelStyle.CalcSize(new GuiContentAdapter(entry.Name)).x;
            }
            catch (Exception ex) when (ex is SecurityException || ex is InvalidOperationException)
            {
                var text = entry.Name == null ? string.Empty : entry.Name.ToString();
                return text.Length * FallbackCharacterWidth;
            }
        }
    }
}
