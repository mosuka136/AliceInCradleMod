using BetterExperience.HGuiSpace;
using BetterExperience.HLogGUI.Resource;
using BetterExperience.HProvider;
using BetterExperience.HTranslatorSpace;
using System;
using System.Collections.Generic;
using UnityEngine;
using static BetterExperience.HLogSpace.HLog;

namespace BetterExperience.HLogGUI
{
    public class ListEditor
    {
        private Vector2 _scrollPosition;

        public LogLevel Level { get; set; } = LogLevel.Info;
        public bool IsColumnWidthDirty { get; set; } = true;
        public bool HasRepeatedEntry { get; private set; } = false;
        public bool HasExceptionEntry { get; private set; } = false;
        public float TotalColumnWidth { get; private set; }
        public Dictionary<EntryContentType, ColumnEditor> ColumnEditorList { get; private set; }
        public EntryEditor EntryEditor { get; }

        public ToastEditor ToastEditor { get; }
        public IUnityGuiProvider UnityGui { get; }
        public IUnityProvider UnityService { get; }
        public StyleResource StyleProvider { get; }

        public ListEditor(IUnityGuiProvider unityGui, IUnityProvider unityService, StyleResource styleProvider, ToastEditor toastEditor)
        {
            UnityGui = unityGui;
            UnityService = unityService;
            StyleProvider = styleProvider;
            ToastEditor = toastEditor;
            EntryEditor = new EntryEditor(unityGui, unityService, toastEditor);
        }

        public class ColumnEditor
        {
            private readonly float MinColumnWidth = 32f;
            private const float ColumnWidthPadding = 18f;

            public bool IsVisible { get; set; } = true;
            public bool ExpendWidth { get; set; } = false;
            public float Width { get; set; } = 0f;

            public EntryContentType ContentType { get; }
            public Translator Header { get; }
            public GUIStyle Style { get; }
            public IUnityGuiProvider UnityGui { get; }
            public IUnityProvider UnityService { get; }

            public ColumnEditor(EntryContentType sortOrder, Translator header, GUIStyle style, IUnityGuiProvider unityGui, IUnityProvider unityService)
            {
                ContentType = sortOrder;
                Header = header;
                Style = style;
                UnityGui = unityGui;
                UnityService = unityService;
            }

            public void RenderHeader(EntryListBinding list)
            {
                if (UnityGui == null || Style == null || list == null)
                    return;

                if (!IsVisible)
                    return;

                var oldFontStyle = Style.fontStyle;
                if (list.SortOrder == ContentType)
                    Style.fontStyle = list.IsSortDescending ? FontStyle.BoldAndItalic : FontStyle.Bold;

                if (UnityGui.Button(Header, Style, ExpendWidth ? UnityGui.ExpandWidth(true) : UnityGui.Width(Width)))
                {
                    if (list.SortOrder == ContentType)
                        list.IsSortDescending = !list.IsSortDescending;
                    else
                    {
                        list.SortOrder = ContentType;
                        list.IsSortDescending = false;
                    }
                }

                Style.fontStyle = oldFontStyle;
            }

            public void RenderCell(EntryBinding entry, EntryEditor editor, LogLevel level)
            {
                if (UnityGui == null || Style == null || entry == null)
                    return;

                if (!IsVisible)
                    return;

                if (!IsLogLevelHigherOrEqual(entry.Level, level))
                    return;

                editor.Render(GetText(entry, ContentType), Style, Width);
            }

            public void UpdateWidth(EntryListBinding list, LogLevel level)
            {
                if (UnityService == null || Style == null || list == null)
                    return;

                var width = 0f;
                foreach (var entry in list.Entries)
                {
                    if (!IsLogLevelHigherOrEqual(entry.Level, level))
                        continue;
                    var splitText = GetText(entry, ContentType).Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    var showText = splitText.Length > 0 ? splitText[0] : " ";
                    width = UnityService.Max(width, MeasureTextWidth(showText));
                }
                Width = UnityService.Max(width, MeasureTextWidth(Header));
            }

            public float MeasureTextWidth(string text)
            {
                var width = Style.CalcSize(UnityGui.GetContent(text)).x + ColumnWidthPadding;
                return UnityService.Max(width, MinColumnWidth);
            }

            public static string GetText(EntryBinding entry, EntryContentType contentType)
            {
                switch (contentType)
                {
                    case EntryContentType.Id:
                        return entry.Id;
                    case EntryContentType.Timestamp:
                        return entry.Timestamp;
                    case EntryContentType.ThreadId:
                        return entry.ThreadId;
                    case EntryContentType.Frame:
                        return entry.Frame;
                    case EntryContentType.Scene:
                        return entry.Scene;
                    case EntryContentType.Level:
                        return entry.Level;
                    case EntryContentType.Message:
                        return entry.Message;
                    case EntryContentType.File:
                        return entry.File;
                    case EntryContentType.Line:
                        return entry.Line;
                    case EntryContentType.Member:
                        return entry.Member;
                    case EntryContentType.Exception:
                        return entry.Exception;
                    case EntryContentType.LastRepeatTime:
                        return entry.LastRepeatTime;
                    case EntryContentType.RepeatCount:
                        return entry.RepeatCount;
                    default:
                        return string.Empty;
                }
            }
        }

        public void Render(EntryListBinding list)
        {
            if (UnityGui == null || StyleProvider == null || list == null)
                return;

            CheckRenderCondition(list);

            RenderColumnVisibilityMenu();
            RenderColumnLogLevelMenu();

            _scrollPosition = UnityGui.BeginScrollView(_scrollPosition);
            UnityGui.BeginHorizontal(UnityGui.BoxStyle);

            foreach (var columnEditor in ColumnEditorList.Values)
            {
                UnityGui.BeginVertical();
                columnEditor.RenderHeader(list);
                foreach (var entry in list.Entries)
                    columnEditor.RenderCell(entry, EntryEditor, Level);
                UnityGui.EndVertical();
            }

            UnityGui.BeginVertical();
            RenderMiscMenuHeader();
            foreach (var entry in list.Entries)
                RenderMiscMenu(entry);
            UnityGui.EndVertical();

            UnityGui.EndHorizontal();
            UnityGui.EndScrollView();
        }

        public void CheckRenderCondition(EntryListBinding list)
        {
            if (ColumnEditorList == null)
            {
                ColumnEditorList = new Dictionary<EntryContentType, ColumnEditor>()
                {
                    { EntryContentType.Id, new ColumnEditor(EntryContentType.Id, TranslatorResource.IdTopBar, StyleProvider.IdButtonStyle, UnityGui, UnityService) },
                    { EntryContentType.Timestamp, new ColumnEditor(EntryContentType.Timestamp, TranslatorResource.TimestampTopBar, StyleProvider.TimestampButtonStyle, UnityGui, UnityService) },
                    { EntryContentType.LastRepeatTime, new ColumnEditor(EntryContentType.LastRepeatTime, TranslatorResource.LastRepeatTimeTopBar, StyleProvider.LastRepeatTimeButtonStyle, UnityGui, UnityService) },
                    { EntryContentType.ThreadId, new ColumnEditor(EntryContentType.ThreadId, TranslatorResource.ThreadIdTopBar, StyleProvider.ThreadIdButtonStyle, UnityGui, UnityService) },
                    { EntryContentType.Frame, new ColumnEditor(EntryContentType.Frame, TranslatorResource.FrameTopBar, StyleProvider.FrameButtonStyle, UnityGui, UnityService) },
                    { EntryContentType.Scene, new ColumnEditor(EntryContentType.Scene, TranslatorResource.SceneTopBar, StyleProvider.SceneButtonStyle, UnityGui, UnityService) },
                    { EntryContentType.Level, new ColumnEditor(EntryContentType.Level, TranslatorResource.LevelTopBar, StyleProvider.LevelButtonStyle, UnityGui, UnityService) },
                    { EntryContentType.Message, new ColumnEditor(EntryContentType.Message, TranslatorResource.MessageTopBar, StyleProvider.MessageButtonStyle, UnityGui, UnityService) },
                    { EntryContentType.Exception, new ColumnEditor(EntryContentType.Exception, TranslatorResource.ExceptionTopBar, StyleProvider.ExceptionButtonStyle, UnityGui, UnityService) },
                    { EntryContentType.File, new ColumnEditor(EntryContentType.File, TranslatorResource.FileTopBar, StyleProvider.FileButtonStyle, UnityGui, UnityService) },
                    { EntryContentType.Line, new ColumnEditor(EntryContentType.Line, TranslatorResource.LineTopBar, StyleProvider.LineButtonStyle, UnityGui, UnityService) },
                    { EntryContentType.Member, new ColumnEditor(EntryContentType.Member, TranslatorResource.MemberTopBar, StyleProvider.MemberButtonStyle, UnityGui, UnityService) },
                    { EntryContentType.RepeatCount, new ColumnEditor(EntryContentType.RepeatCount, TranslatorResource.RepeatCountTopBar, StyleProvider.RepeatCountButtonStyle, UnityGui, UnityService) },
                };

                ColumnEditorList[EntryContentType.LastRepeatTime].IsVisible = false;
                ColumnEditorList[EntryContentType.Exception].IsVisible = false;
                ColumnEditorList[EntryContentType.File].IsVisible = false;
                ColumnEditorList[EntryContentType.Line].IsVisible = false;
                ColumnEditorList[EntryContentType.Member].IsVisible = false;
                ColumnEditorList[EntryContentType.RepeatCount].IsVisible = false;
            }

            if (IsColumnWidthDirty)
            {
                foreach (var columnEditor in ColumnEditorList.Values)
                    columnEditor.UpdateWidth(list, Level);
                UpdateTotalColumnWidth();
                IsColumnWidthDirty = false;
            }

            if (!HasRepeatedEntry && list.HasRepeatedEntry)
            {
                ColumnEditorList[EntryContentType.LastRepeatTime].IsVisible = true;
                ColumnEditorList[EntryContentType.RepeatCount].IsVisible = true;
                HasRepeatedEntry = true;
            }

            if (!HasExceptionEntry && list.HasExceptionEntry)
            {
                ColumnEditorList[EntryContentType.Exception].IsVisible = true;
                HasExceptionEntry = true;
            }
        }

        public void UpdateTotalColumnWidth()
        {
            TotalColumnWidth = 0f;
            foreach (var columnEditor in ColumnEditorList.Values)
            {
                if (columnEditor.IsVisible)
                    TotalColumnWidth += columnEditor.Width + 10f;
            }

            TotalColumnWidth += UnityService.Max(
                UnityGui.ButtonStyle.CalcSize(UnityGui.GetContent(TranslatorResource.MiscMenu)).x,
                UnityGui.ButtonStyle.CalcSize(UnityGui.GetContent(TranslatorResource.CopyLog)).x) + 10f;

            var menuWidth = 0f;
            foreach (var columnEditor in ColumnEditorList.Values)
                menuWidth += StyleProvider.ColumnVisibilityToggleStyle.CalcSize(UnityGui.GetContent(columnEditor.Header)).x + 10f;

            TotalColumnWidth = UnityService.Max(TotalColumnWidth, menuWidth);
        }

        public void RenderMiscMenuHeader()
        {
            if (UnityGui == null || StyleProvider == null)
                return;
            UnityGui.Button(TranslatorResource.MiscMenu, StyleProvider.MiscButtonStyle, UnityGui.ExpandWidth(true));
        }

        public void RenderMiscMenu(EntryBinding entry)
        {
            if (UnityGui == null || StyleProvider == null || entry == null)
                return;
            if (!IsLogLevelHigherOrEqual(entry.Level, Level))
                return;
            if (UnityGui.Button(TranslatorResource.CopyLog, StyleProvider.MiscButtonStyle, UnityGui.ExpandWidth(true)))
            {
                UnityService.ClipboardCopy(entry.ToString());
                ToastEditor.SetToast($"{TranslatorResource.Copied} Log{entry.Id}");
            }
        }

        public void RenderColumnVisibilityMenu()
        {
            if (UnityGui == null)
                return;

            UnityGui.BeginVertical(UnityGui.BoxStyle);
            UnityGui.Space(4);
            UnityGui.BeginHorizontal();

            foreach (var columnEditor in ColumnEditorList.Values)
            {
                var isVisible = columnEditor.IsVisible;
                isVisible = UnityGui.Toggle(isVisible, columnEditor.Header, StyleProvider.ColumnVisibilityToggleStyle);
                if (columnEditor.IsVisible != isVisible)
                {
                    columnEditor.IsVisible = isVisible;
                    UpdateTotalColumnWidth();
                }
            }

            UnityGui.EndHorizontal();
            UnityGui.Space(4);
            UnityGui.EndVertical();
        }

        public void RenderColumnLogLevelMenu()
        {
            if (UnityGui == null || StyleProvider == null)
                return;

            UnityGui.BeginVertical(UnityGui.BoxStyle);
            UnityGui.Space(4);
            UnityGui.BeginHorizontal();

            foreach (var logLevel in Enum.GetValues(typeof(LogLevel)))
            {
                var level = (LogLevel)logLevel;
                if (UnityGui.Toggle(Level == level, level.ToString(), StyleProvider.ColumnLogLevelToggleStyle))
                {
                    if (level != Level)
                    {
                        Level = level;
                        IsColumnWidthDirty = true;
                    }
                }
                UnityGui.Space(4);
            }

            UnityGui.EndHorizontal();
            UnityGui.Space(4);
            UnityGui.EndVertical();
        }

        public static bool IsLogLevelHigherOrEqual(string entryLogLevel, LogLevel filterLogLevel)
        {
            if (Enum.TryParse(entryLogLevel, out LogLevel entryLevel))
                return entryLevel >= filterLogLevel;
            return false;
        }
    }
}
