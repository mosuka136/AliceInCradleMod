using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HProvider;
using BetterExperience.HTranslatorSpace;
using System;
using UnityEngine;

namespace BetterExperience.HConfigGUI.Editor
{
    public class PopupEditor
    {
        public readonly int PopupID = Guid.NewGuid().GetHashCode();

        public Rect PopupRect { get; private set; }
        public IUnityGuiProvider UnityGui { get; }
        public StyleResource StyleProvider { get; }
        public GuiStateStore GuiStateStore { get; }

        public Translator Title { get; set; }
        public Action DrawContentAction { get; set; }
        public Action ClosePopupAction { get; set; }

        public PopupEditor(IUnityGuiProvider unityGui, StyleResource styleProvider, GuiStateStore guiStateStore)
        {
            UnityGui = unityGui;
            StyleProvider = styleProvider;
            GuiStateStore = guiStateStore;

            float width = UnityGui.ScreenWidth * 0.25f;
            float height = UnityGui.ScreenHeight * 0.15f;
            PopupRect = new Rect((UnityGui.ScreenWidth - width) / 2f, (UnityGui.ScreenHeight - height) / 2f, width, height);
        }

        public void DrawPopup(Translator title, Action drawContentAction, Action closePopupAction)
        {
            Title = title;
            DrawContentAction = drawContentAction;
            ClosePopupAction = closePopupAction;

            var color = UnityGui.Color;
            UnityGui.Color = new Color(0f, 0f, 0f, 0.55f);
            UnityGui.Box(new Rect(0f, 0f, UnityGui.ScreenWidth, UnityGui.ScreenHeight), string.Empty);
            UnityGui.Color = color;

            PopupRect = UnityGui.ModalWindow(PopupID, PopupRect, DrawPopupWindow, string.Empty, UnityGui.BoxStyle);
        }

        private void DrawPopupWindow(int id)
        {
            UnityGui.BeginVertical();
            UnityGui.Label(Title, StyleProvider.PopupTitleStyle, UnityGui.ExpandWidth(true));

            DrawContentAction?.Invoke();

            if (UnityGui.Button(TranslatorResource.Close, UnityGui.ExpandWidth(true)))
            {
                ClosePopupAction?.Invoke();
                GuiStateStore.SetBool(GuiStateStore.IsPopupOpenKey, false);
            }

            UnityGui.EndVertical();
        }
    }
}
