using BetterExperience.HClassAttribute;
using System;
using UnityEngine;

namespace BetterExperience.HConfigGUI.UI
{
    /// <summary>
    /// 配置界面的 Unity 宿主组件。
    /// 该组件在游戏启动后由 <see cref="GameBootManager"/> 创建，负责接收热键、驱动 ViewModel 更新并在 OnGUI 中绘制窗口。
    /// </summary>
    [RegisterOnGameBoot]
    public class GuiHost : MonoBehaviour
    {
        private ViewModel _viewModel;
        private SheetRenderer _sheetRenderer;
        private ToastRenderer _toastRenderer;
        private TooltipRenderer _tooltipRenderer;

        public readonly int WindowID = Guid.NewGuid().GetHashCode();

        private Vector2 _scrollPosition = Vector2.zero;
        private bool _isVisible = false;
        // 首次打开后如果用户拖动过窗口，则不再用“点击窗口外”自动隐藏，避免拖动释放时误判为失焦。
        private bool _hasDraggedWindowSinceOpen = false;

        private void Awake()
        {
            try
            {
                _viewModel = new ViewModel();
                _sheetRenderer = new SheetRenderer(_viewModel);
                _toastRenderer = new ToastRenderer(_viewModel);
                _tooltipRenderer = new TooltipRenderer(_viewModel);

                float width = Screen.width * 0.3f;
                float height = Screen.height * 0.8f;
                _viewModel.WindowRect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);

                HLog.Debug($"Config GUI host created. WindowId={WindowID}");
            }
            catch (Exception ex)
            {
                HLog.Error("Failed to create Config GUI host.", ex);
                Destroy(this);
            }
        }

        private void Update()
        {
            var hotkey = _viewModel.ConfigUIHotkey;
            if (hotkey != null && hotkey.WasPressedThisFrame())
            {
                HLog.Debug("Config GUI toggle hotkey pressed.");
                ToggleVisibility();
            }

            _viewModel.Update(_viewModel.UnityService.UnscaledDeltaTime);
        }

        private void OnGUI()
        {
            if (!_isVisible)
                return;

            if (_viewModel.LabelWidth < 0f)
                _viewModel.LabelWidth = _viewModel.LayoutResourceInstance.GetLabelWidth();

            var rect = GUI.Window(WindowID, _viewModel.WindowRect, DrawWindow, TranslatorResource.Title);
            
            if (!_hasDraggedWindowSinceOpen && _viewModel.WindowRect.position != rect.position)
                _hasDraggedWindowSinceOpen = true;
            _viewModel.WindowRect = rect;

            TryAutoHideOnFocusLost();
        }

        private void DrawWindow(int id)
        {
            _viewModel.UnityGuiService.BeginArea(new Rect(10f, 30f, _viewModel.WindowRect.width - 20f, _viewModel.WindowRect.height - 40f));
            _scrollPosition = _viewModel.UnityGuiService.BeginScrollView(_scrollPosition);

            _sheetRenderer.Render(_viewModel.Sheet);

            _viewModel.UnityGuiService.EndScrollView();
            _viewModel.UnityGuiService.EndArea();

            _toastRenderer.Render();
            _tooltipRenderer.Render();
            GUI.DragWindow();
        }

        private void TryAutoHideOnFocusLost()
        {
            if (_hasDraggedWindowSinceOpen)
                return;

            var currentEvent = Event.current;
            if (currentEvent == null || currentEvent.type != EventType.MouseDown)
                return;

            if (_viewModel.WindowRect.Contains(currentEvent.mousePosition))
                return;

            HLog.Debug("Config GUI auto-hidden because focus was lost.");
            Hide();
            GUI.FocusControl(null);
        }

        /// <summary>
        /// 隐藏配置窗口并清理展开项/录制状态。
        /// 热键录制中不会关闭窗口，以避免用户正在编辑的组合被静默丢弃。
        /// </summary>
        public void Hide()
        {
            if (_viewModel.RecordingHotkey != null)
            {
                HLog.Info("Cannot hide config GUI while hotkey recording is active.");
                _viewModel.ShowToast(TranslatorResource.CanNotHideBeforeEndRecord, _viewModel.ToastDuration);
                return;
            }

            if (!_isVisible && _viewModel.OpenedEnumEntry == null && _viewModel.OpenedHotkeyEntry == null && _viewModel.RecordingHotkey == null && !_hasDraggedWindowSinceOpen)
                return;

            _isVisible = false;
            _viewModel.OpenedEnumEntry = null;
            _viewModel.OpenedHotkeyEntry = null;
            _viewModel.RecordingHotkey = null;
            _hasDraggedWindowSinceOpen = false;

            HLog.Debug("Config GUI hidden.");
        }

        /// <summary>
        /// 切换配置窗口可见性。
        /// </summary>
        public void ToggleVisibility()
        {
            if (_isVisible)
                Hide();
            else
            {
                _isVisible = true;
                HLog.Debug("Config GUI shown.");
            }
        }
    }
}
