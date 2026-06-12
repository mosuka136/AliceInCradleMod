using BetterExperience.HLogGUI.Resource;
using BetterExperience.HProvider;
using System;
using UnityEngine;

namespace BetterExperience.HLogGUI
{
    public class EntryEditor
    {
        public IUnityGuiProvider UnityGui { get; }
        public IUnityProvider UnityService { get; }

        public EntryEditor(IUnityGuiProvider unityGui, IUnityProvider unityService, StyleResource styleProvider)
        {
            UnityGui = unityGui;
            UnityService = unityService;
        }

        public void Render(string text, GUIStyle style, float width)
        {
            if (UnityGui == null || UnityService == null)
                return;

            UnityGui.BeginHorizontal();

            var splitText = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var showText = splitText.Length > 0 ? splitText[0] : " ";
            if (UnityGui.Button(showText, style, UnityGui.Width(width)))
                UnityService.ClipboardCopy(text);

            UnityGui.EndHorizontal();
        }
    }
}
