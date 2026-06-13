using BetterExperience.HGuiSpace;
using BetterExperience.HLogGUI.Resource;
using BetterExperience.HProvider;
using System;
using System.Linq;
using UnityEngine;

namespace BetterExperience.HLogGUI
{
    public class EntryEditor
    {
        public IUnityGuiProvider UnityGui { get; }
        public IUnityProvider UnityService { get; }
        public ToastEditor ToastEditor { get; }

        public EntryEditor(IUnityGuiProvider unityGui, IUnityProvider unityService, ToastEditor toastEditor)
        {
            UnityGui = unityGui;
            UnityService = unityService;
            ToastEditor = toastEditor;
        }

        public void Render(string text, GUIStyle style, float width)
        {
            if (UnityGui == null || UnityService == null)
                return;

            UnityGui.BeginHorizontal();

            var splitText = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            splitText = splitText.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            var showText = splitText.Length > 0 ? splitText[0] : " ";

            var content = splitText.Length > 1 ? UnityGui.GetContent(showText, text) : UnityGui.GetContent(showText);
            if (UnityGui.Button(content, style, UnityGui.Width(width)))
            {
                UnityService.ClipboardCopy(text);
                ToastEditor.SetToast(TranslatorResource.Copied + showText);
            }

            UnityGui.EndHorizontal();
        }
    }
}
