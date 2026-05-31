using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Editor.ValueEditor;
using System.Collections.Generic;

namespace BetterExperience.HConfigGUI.Editor
{
    public class ValueEditorRegistry
    {
        private readonly List<IValueEditor> _editors = new List<IValueEditor>();

        public void RegisterEditor(IValueEditor editor)
        {
            _editors.Add(editor);
        }

        public IValueEditor GetEditor(IEntryBinding entry)
        {
            foreach (var editor in _editors)
            {
                if (editor.CanEdit(entry))
                    return editor;
            }
            return UnsupportedEditor.Instance;
        }
    }
}