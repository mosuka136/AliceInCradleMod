using BetterExperience.HAdapter;
using System;
using UnityEngine;
using Rect = BetterExperience.HAdapter.Rect;

namespace BetterExperience.HConfigGUI.UI
{
    public class GuiHost : MonoBehaviour
    {
        private ViewModel _viewModel;
        private IGuiLayout _layout;
        private SheetRenderer _sheetRenderer;
        private ToastRenderer _toastRenderer;
        private TooltipRenderer _tooltipRenderer;

        public readonly int WindowID = Guid.NewGuid().GetHashCode();

        private Vector2 _scrollPosition = Vector2.zero;
        private bool _isVisible = false;
        private bool _hasDraggedWindowSinceOpen = false;

        private void Awake()
        {
            _viewModel = new ViewModel();
            _layout = new GuiLayoutAdapter();
            _sheetRenderer = new SheetRenderer(_viewModel, _layout);
            _toastRenderer = new ToastRenderer(_viewModel);
            _tooltipRenderer = new TooltipRenderer(_viewModel);

            float width = Screen.width * 0.3f;
            float height = Screen.height * 0.8f;
            _viewModel.WindowRect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
        }

        private void Update()
        {
            var hotkey = _viewModel.ConfigUIHotkey;
            if (hotkey != null && hotkey.IsValid && hotkey.WasPressedThisFrame())
                ToggleVisibility();

            _viewModel.Update(UnityTimeAdapter.UnscaledDeltaTime);
        }

        private void OnGUI()
        {
            if (!_isVisible)
                return;

            var rect = GUI.Window(WindowID, _viewModel.WindowRect, DrawWindow, TranslatorResource.Title);
            
            if (!_hasDraggedWindowSinceOpen && _viewModel.WindowRect.position != rect.position)
                _hasDraggedWindowSinceOpen = true;
            _viewModel.WindowRect = rect;

            TryAutoHideOnFocusLost();
        }

        private void DrawWindow(int id)
        {
            _layout.BeginArea(new Rect(10f, 30f, _viewModel.WindowRect.width - 20f, _viewModel.WindowRect.height - 40f));
            _scrollPosition = _layout.BeginScrollView(_scrollPosition);

            _sheetRenderer.Render(_viewModel.Sheet);

            _layout.EndScrollView();
            _layout.EndArea();

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

            Hide();
            GUI.FocusControl(null);
        }

        public void Hide()
        {
            _isVisible = false;
            _viewModel.OpenedEnumEntry = null;
            _hasDraggedWindowSinceOpen = false;
        }

        public void ToggleVisibility()
        {
            if (_isVisible)
                Hide();
            else
                _isVisible = true;
        }
    }
}
