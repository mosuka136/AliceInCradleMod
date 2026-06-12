using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
using BetterExperience.HLogGUI.Resource;
using BetterExperience.HLogSpace;
using BetterExperience.HotkeyManager;
using BetterExperience.HProvider;
using System;
using UnityEngine;

namespace BetterExperience.HLogGUI
{
    [RegisterOnGameBoot]
    public class GuiHost : MonoBehaviour
    {
        public readonly int WindowID = Guid.NewGuid().GetHashCode();

        public IUnityProvider UnityService { get; private set; }
        public IUnityGuiProvider UnityGui { get; private set; }
        public StyleResource StyleProvider { get; private set; }
        public ListEditor ListEditor { get; private set; }
        public EntryListBinding EntryList { get; private set; }

        public bool IsVisible { get; private set; } = false;
        // 首次打开后如果用户拖动过窗口，则不再用“点击窗口外”自动隐藏，避免拖动释放时误判为失焦。
        public bool HasDraggedWindowSinceOpen { get; private set; } = false;
        public Rect WindowRect { get; set; }
        public Hotkey LogUIHotkey { get; private set; }

        public void Awake()
        {
            try
            {
                LogUIHotkey = ConfigManager.LogUIHotkey.Value;
                ConfigManager.LogUIHotkey.OnValueChanged += (s, e) => LogUIHotkey = e;
                ConfigManager.SetLanguage.OnValueChanged += (s, e) => ListEditor.IsColumnWidthDirty = true;

                UnityGui = new UnityGuiProvider();
                UnityService = new UnityProvider();
                StyleProvider = new StyleResource(UnityGui);
                EntryList = new EntryListBinding();
                ListEditor = new ListEditor(UnityGui, UnityService, StyleProvider);

                foreach (var log in HLog.EarlyLogs)
                    EntryList.AddEntry(new EntryBinding(log));

                HLog.OnLog += e =>
                {
                    EntryList.AddEntry(new EntryBinding(e));
                    ListEditor.IsColumnWidthDirty = true;
                };

                float width = UnityGui.ScreenWidth * 0.8f;
                float height = UnityGui.ScreenHeight * 0.5f;
                WindowRect = new Rect((UnityGui.ScreenWidth - width) / 2f, (UnityGui.ScreenHeight - height) / 2f, width, height);

                HLog.Debug($"Log GUI host created. WindowId={WindowID}");
            }
            catch (Exception ex)
            {
                HLog.Error("Failed to create Log GUI host.", ex);
                Destroy(this);
            }
        }

        public void OnDestroy()
        {
            HLog.OnLog -= e =>
            {
                EntryList.AddEntry(new EntryBinding(e));
                ListEditor.IsColumnWidthDirty = true;
            };
        }

        public void Update()
        {
            if (LogUIHotkey?.WasPressedThisFrame() == true)
            {
                HLog.Debug("Log GUI toggle hotkey pressed.");
                ToggleVisibility();
            }
        }

        public void OnGUI()
        {
            if (!IsVisible)
                return;

            var rect = WindowRect;
            rect.width = UnityService.Clamp(ListEditor.TotalColumnWidth, UnityGui.ScreenWidth * 0.5f, UnityGui.ScreenWidth * 0.9f);

            rect = GUI.Window(WindowID, rect, DrawWindow, TranslatorResource.Title);

            if (!HasDraggedWindowSinceOpen && WindowRect.position != rect.position)
                HasDraggedWindowSinceOpen = true;
            WindowRect = rect;

            TryAutoHideOnFocusLost();
        }

        public void DrawWindow(int id)
        {
            UnityGui.BeginArea(new Rect(10f, 30f, WindowRect.width - 20f, WindowRect.height - 40f));
            ListEditor.Render(EntryList);
            UnityGui.EndArea();

            GUI.DragWindow();
        }

        public void TryAutoHideOnFocusLost()
        { 
            if (HasDraggedWindowSinceOpen)
                return;

            var currentEvent = Event.current;
            if (currentEvent == null || currentEvent.type != EventType.MouseDown)
                return;

            if (WindowRect.Contains(currentEvent.mousePosition))
                return;

            HLog.Debug("Log GUI auto-hidden because focus was lost.");
            Hide();
            GUI.FocusControl(null);
        }

        /// <summary>
        /// 隐藏日志窗口
        /// </summary>
        public void Hide()
        {
            if (!IsVisible)
                return;

            IsVisible = false;
            HasDraggedWindowSinceOpen = false;

            HLog.Debug("Log GUI hidden.");
        }

        /// <summary>
        /// 切换日志窗口可见性。
        /// </summary>
        public void ToggleVisibility()
        {
            if (IsVisible)
                Hide();
            else
            {
                IsVisible = true;
                HLog.Debug("Log GUI shown.");
            }
        }
    }
}
