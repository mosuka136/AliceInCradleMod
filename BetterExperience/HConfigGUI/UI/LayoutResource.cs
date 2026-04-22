using System;
using System.Security;

namespace BetterExperience.HConfigGUI.UI
{
    public class LayoutResource
    {
        private const float LabelWidthMargin = 10f;
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
