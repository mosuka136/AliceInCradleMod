using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Editor.ValueEditor;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HProvider;

namespace BetterExperience.HConfigGUI.Editor
{
    public class TableEditor
    {
        public IUnityProvider UnityService { get; }
        public IUnityGuiProvider UnityGui { get; }
        public StyleResource StyleProvider { get; }

        public ValueEditorRegistry EditorRegistry { get; }
        public GuiStateStore GuiStateStore { get; }
        public EntryChangeSink ChangeSink { get; }
        public EntryEditor EntryRenderer { get; }

        public TableEditor(IUnityProvider unityService, IUnityGuiProvider unityGui, GuiStateStore guiStateStore, StyleResource styleProvider)
        {
            UnityService = unityService;
            UnityGui = unityGui;
            GuiStateStore = guiStateStore;
            StyleProvider = styleProvider;

            EditorRegistry = new ValueEditorRegistry();
            EditorRegistry.RegisterEditor(new BooleanEditor(unityGui));
            EditorRegistry.RegisterEditor(new StringEditor(unityGui));
            EditorRegistry.RegisterEditor(new SliderEditor(unityGui, unityService, styleProvider));
            EditorRegistry.RegisterEditor(new NumberEditor(unityGui));
            EditorRegistry.RegisterEditor(new EnumEditor(unityGui));
            EditorRegistry.RegisterEditor(new HotkeyEditor(unityGui, styleProvider));

            ChangeSink = new EntryChangeSink();
            EntryRenderer = new EntryEditor(EditorRegistry, GuiStateStore, ChangeSink, unityGui);
        }

        public void DrawTable(TableBinding table)
        {
            UnityGui.BeginVertical();
            UnityGui.BeginHorizontal();
            UnityGui.Label(UnityGui.GetContent(table.Name, table.Description), StyleProvider.TableTitleStyle, UnityGui.ExpandWidth(true));
            UnityGui.EndHorizontal();

            foreach (var entry in table.Table)
                EntryRenderer.Render(entry);

            UnityGui.Space(10f);
            UnityGui.EndVertical();
        }

        public void Update(float deltaTime)
        {
            ChangeSink.FlushValue(deltaTime);
        }
    }
}
