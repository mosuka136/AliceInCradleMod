using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HProvider;
using UnityEngine;

namespace BetterExperience.HConfigGUI.Editor
{
    public class SheetEditor
    {
        public IUnityProvider UnityService { get; }
        public IUnityGuiProvider UnityGui { get; }
        public GuiStateStore GuiStateStore { get; }
        public StyleResource StyleProvider { get; }

        public TableEditor TableEditor { get; }

        private Vector2 _sidebarScrollPosition = Vector2.zero;
        private Vector2 _contentScrollPosition = Vector2.zero;

        public SheetEditor(IUnityProvider unityService, IUnityGuiProvider unityGui, GuiStateStore guiStateStore, StyleResource styleProvider)
        {
            UnityService = unityService;
            UnityGui = unityGui;
            GuiStateStore = guiStateStore;
            StyleProvider = styleProvider;

            TableEditor = new TableEditor(unityService, unityGui, guiStateStore, styleProvider);
        }

        public void DrawSheet(SheetBinding sheet)
        {
            var selectedTableIndex = GuiStateStore.GetInt(GuiStateStore.SelectedTableIndexKey, 0);
            selectedTableIndex = selectedTableIndex >= sheet.Sheet.Count ? sheet.Sheet.Count - 1 : selectedTableIndex;
            selectedTableIndex = selectedTableIndex < 0 ? 0 : selectedTableIndex;

            UnityGui.BeginHorizontal();
            DrawSidebar(sheet, ref selectedTableIndex);
            UnityGui.Space(10f);
            DrawSelectedTable(sheet.Sheet[selectedTableIndex]);
            UnityGui.EndHorizontal();

            GuiStateStore.SetInt(GuiStateStore.SelectedTableIndexKey, selectedTableIndex);
        }

        public void DrawSidebar(SheetBinding sheet, ref int selectedTableIndex)
        {
            UnityGui.BeginVertical(UnityGui.BoxStyle, UnityGui.Width(GuiStateStore.GetFloat(GuiStateStore.TableButtonWidthKey, 150f)));
            _sidebarScrollPosition = UnityGui.BeginScrollView(_sidebarScrollPosition);

            for (int i = 0; i < sheet.Sheet.Count; i++)
            {
                var isSelected = selectedTableIndex == i;
                var table = sheet.Sheet[i];
                if (UnityGui.Button(
                    table.Name,
                    isSelected ? StyleProvider.SidebarSelectedEntryStyle : StyleProvider.SidebarEntryStyle,
                    UnityGui.ExpandWidth(true)))
                {
                    if (!isSelected)
                    {
                        _contentScrollPosition = Vector2.zero;
                        selectedTableIndex = i;
                    }
                }
            }

            UnityGui.EndScrollView();
            UnityGui.EndVertical();
        }

        public void DrawSelectedTable(TableBinding table)
        {
            UnityGui.BeginVertical(UnityGui.BoxStyle);
            _contentScrollPosition = UnityGui.BeginScrollView(_contentScrollPosition);
            TableEditor.DrawTable(table);
            UnityGui.EndVertical();
            UnityGui.EndScrollView();
        }

        public void Update(float deltaTime)
        {
            TableEditor.Update(deltaTime);
        }
    }
}
