using BetterExperience.BepConfigManager;
using BetterExperience.ConfigFileSpace;
using BetterExperience.HEnumHelper;
using BetterExperience.TranslatorSpace;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterExperience.ConfigGUI
{
    public class MainGUI
    {
        private MainGUI()
        {
        }

        public static MainGUI Instance { get; } = new MainGUI();

        private static volatile bool _initialized = false;

        private void Awake()
        {
            if (_initialized)
                return;

            var go = new GameObject($"{nameof(BetterExperience)}_{nameof(MainGUI)}");
            go.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<ConfigGUI>();

            Translator.DefaultLanguage = ConfigManager.SetLanguage.Value;
            ConfigManager.SetLanguage.OnValueChanged += (s, e) =>
            {
                Translator.DefaultLanguage = e;
                ConfigGUI._cachedEntryLabelWidth = -1f;
            };

            _initialized = true;
        }

        [HarmonyPatch(typeof(SceneManager), "Internal_SceneLoaded")]
        public class LoadScenePatch
        {
            public static void Postfix()
            {
                if (MainGUI._initialized)
                    return;

                Instance.Awake();
            }
        }

        public class ConfigGUI : MonoBehaviour
        {
            private const int WindowID = 123456;

            internal static float _cachedEntryLabelWidth = -1f;

            private bool _showGUI = false;
            private Rect _windowRect;
            private bool _hasDraggedWindowSinceOpen;

            private HotkeyInputSystem _hotkey;
            private string _openedEnumEntryName;

            private GUIStyle _tableStyle;
            private GUIStyle _tooltipStyle;

            private static readonly Dictionary<Type, Func<string, (bool, object)>> _parsers = new Dictionary<Type, Func<string, (bool, object)>>()
            {
                [typeof(sbyte)] = s => (sbyte.TryParse(s, out var v), v),
                [typeof(short)] = s => (short.TryParse(s, out var v), v),
                [typeof(int)] = s => (int.TryParse(s, out var v), v),
                [typeof(long)] = s => (long.TryParse(s, out var v), v),
                [typeof(byte)] = s => (byte.TryParse(s, out var v), v),
                [typeof(ushort)] = s => (ushort.TryParse(s, out var v), v),
                [typeof(uint)] = s => (uint.TryParse(s, out var v), v),
                [typeof(ulong)] = s => (ulong.TryParse(s, out var v), v),
                [typeof(float)] = s => (float.TryParse(s, out var v), v),
                [typeof(double)] = s => (double.TryParse(s, out var v), v),
            };

            private readonly Translator _titleStr = new Translator("更好的体验模组配置", "BetterExperience Mod Configurations");
            private readonly Translator _onStr = new Translator("开启", "On");
            private readonly Translator _offStr = new Translator("关闭", "Off");
            private readonly Translator _resetStr = new Translator("重置", "Reset");
            private readonly Translator _changedStr = new Translator("已修改: ", "Changed: ");
            private readonly Translator _resetDoneStr = new Translator("已重置: ", "Reset: ");

            private string _toastMessage;
            private float _toastEndTime;
            private GUIStyle _toastStyle;
            private const float ToastDuration = 2f;
            private const float ToastFadeDuration = 0.5f;

            private void OnGUI()
            {
                if (!_showGUI)
                    return;

                var previousPosition = _windowRect.position;
                _windowRect = GUI.Window(WindowID, _windowRect, DrawConfigWindow, _titleStr);

                if (!_hasDraggedWindowSinceOpen && _windowRect.position != previousPosition)
                    _hasDraggedWindowSinceOpen = true;

                TryAutoHideOnFocusLost();
            }

            private void Update()
            {
                if (_hotkey == null)
                {
                    var h = ConfigManager.ConfigUIHotkey.Value;
                    if (!HotkeyInputSystem.TryParse(h, out _hotkey))
                    {
                        HLog.Warn("Invalid Hotkey: " + h);
                        h = "F1";
                        HotkeyInputSystem.TryParse(h, out _hotkey);
                        HLog.Info("Config UI hotkey set: " + h);
                    }
                }

                if (_hotkey != null && _hotkey.IsValid && _hotkey.WasPressedThisFrame())
                {
                    _showGUI = !_showGUI;
                    if (_showGUI)
                    {
                        _hasDraggedWindowSinceOpen = false;
                        _openedEnumEntryName = null;
                        _cachedEntryLabelWidth = -1f;
                        float w = Screen.width * 0.3f;
                        float h = Screen.height * 0.8f;
                        _windowRect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
                    }
                }
            }

            private void TryAutoHideOnFocusLost()
            {
                if (_hasDraggedWindowSinceOpen)
                    return;

                var currentEvent = Event.current;
                if (currentEvent == null || currentEvent.type != EventType.MouseDown)
                    return;

                if (_windowRect.Contains(currentEvent.mousePosition))
                    return;

                _showGUI = false;
                _openedEnumEntryName = null;
                GUI.FocusControl(null);
            }

            private Vector2 _scrollPosition;

            private void DrawConfigWindow(int windowID)
            {
                GUILayout.BeginArea(new Rect(10, 30, _windowRect.width - 20, _windowRect.height - 40));
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);


                if (_cachedEntryLabelWidth < 0)
                    _cachedEntryLabelWidth = CalculateLabelWidth(ConfigManager.Tables);

                if (_tableStyle == null)
                {
                    _tableStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold
                    };
                }

                foreach (ConfigTable table in ConfigManager.Tables.Values)
                {
                    GUILayout.BeginVertical("box");

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(table.Name, table.Description), _tableStyle, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    foreach (var entry in table.Table)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent(entry.Name, entry.Description), GUILayout.Width(_cachedEntryLabelWidth));
                        DrawEntryControl(entry);
                        DrawEntryResetButton(entry);
                        GUILayout.EndHorizontal();

                        DrawEnumMenu(entry);
                    }

                    GUILayout.Space(10);

                    GUILayout.EndVertical();
                    GUILayout.Space(10);
                }

                GUILayout.EndScrollView();
                GUILayout.EndArea();

                DrawToast();

                GUI.DragWindow();

                DrawTooltip();
            }

            private void DrawTooltip()
            {
                if (string.IsNullOrEmpty(GUI.tooltip))
                    return;

                if (_tooltipStyle == null || _tooltipStyle.normal.background == null)
                {
                    _tooltipStyle = new GUIStyle(GUI.skin.box)
                    {
                        wordWrap = true,
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset(6, 6, 4, 4)
                    };
                    var bgTex = new Texture2D(1, 1);
                    bgTex.hideFlags = HideFlags.HideAndDontSave;
                    bgTex.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 0.95f));
                    bgTex.Apply();
                    _tooltipStyle.normal.background = bgTex;
                    _tooltipStyle.normal.textColor = Color.white;
                }

                var tooltipContent = new GUIContent(GUI.tooltip);
                float maxTooltipWidth = _windowRect.width * 0.6f;
                float tooltipHeight = _tooltipStyle.CalcHeight(tooltipContent, maxTooltipWidth);

                var mousePos = Event.current.mousePosition;
                float x = Mathf.Clamp(mousePos.x + 15, 0, _windowRect.width - maxTooltipWidth);
                float y = Mathf.Clamp(mousePos.y + 15, 0, _windowRect.height - tooltipHeight);
                GUI.Label(new Rect(x, y, maxTooltipWidth, tooltipHeight), tooltipContent, _tooltipStyle);
            }

            public static float CalculateLabelWidth(OrderedDictionary tables)
            {
                if (tables == null || tables.Count == 0)
                    return 0f;

                float maxWidth = 0;
                foreach (ConfigTable table in tables.Values)
                {
                    foreach (var entry in table.Table)
                    {
                        var content = new GUIContent(entry.Name);
                        var width = GUI.skin.label.CalcSize(content).x;
                        if (width > maxWidth)
                            maxWidth = width;
                    }
                }
                return maxWidth + 10f;
            }

            private void DrawEntryControl(IConfigEntry entry)
            {
                if (entry == null)
                    return;

                var type = entry.ValueType;
                if (type == typeof(bool))
                {
                    var currentValue = (bool)entry.BoxedValue;
                    var newBool = GUILayout.Toggle(currentValue, currentValue ? _onStr : _offStr, GUILayout.ExpandWidth(true));
                    if (newBool != currentValue)
                    {
                        entry.BoxedValue = newBool;
                        ShowToast((string)_changedStr + entry.Name);
                    }
                }
                else if (type == typeof(string))
                {
                    var strValue = entry.BoxedValue as string ?? "";
                    var newStrValue = GUILayout.TextField(strValue, GUILayout.ExpandWidth(true));
                    if (newStrValue != strValue)
                    {
                        entry.BoxedValue = newStrValue;
                        ShowToast((string)_changedStr + entry.Name);
                    }
                }
                else if (type.IsEnum)
                {
                    DrawEnumControl(entry);
                }
                else if (_parsers.TryGetValue(type, out var parser))
                {
                    var strValue = entry.BoxedValue.ToString();
                    var newStrValue = GUILayout.TextField(strValue, GUILayout.ExpandWidth(true));
                    if (newStrValue != strValue)
                    {
                        var (success, parsedValue) = parser(newStrValue);
                        if (success)
                        {
                            entry.BoxedValue = parsedValue;
                            ShowToast((string)_changedStr + entry.Name);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"Unsupported Type: {type.Name}", GUILayout.ExpandWidth(true));
                }
            }

            private void DrawEnumControl(IConfigEntry entry)
            {
                if (entry == null)
                    return;

                if (!entry.ValueType.IsEnum)
                    return;

                var entryName = $"{entry.TableName}.{entry.Key}";
                bool isOpened = _openedEnumEntryName == entryName;

                if (GUILayout.Button(EnumHelper.GetDescription(entry.ValueType, (Enum)entry.BoxedValue), GUILayout.ExpandWidth(true)))
                {
                    _openedEnumEntryName = isOpened ? null : entryName;
                }
            }

            private void DrawEnumMenu(IConfigEntry entry)
            {
                if (entry == null)
                    return;

                if (!entry.ValueType.IsEnum)
                    return;

                if (_openedEnumEntryName == $"{entry.TableName}.{entry.Key}")
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(_cachedEntryLabelWidth);
                    GUILayout.BeginVertical("box");

                    foreach (var value in Enum.GetValues(entry.ValueType))
                    {
                        if (!EnumHelper.IsDisplay(entry.ValueType, (Enum)value))
                            continue;
                        if (GUILayout.Button(EnumHelper.GetDescription(entry.ValueType, (Enum)value)))
                        {
                            entry.BoxedValue = value;
                            _openedEnumEntryName = null;
                            ShowToast((string)_changedStr + entry.Name);
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }

            private void DrawEntryResetButton(IConfigEntry entry)
            {
                if (entry == null)
                    return;

                if (GUILayout.Button(_resetStr, GUILayout.ExpandWidth(false)))
                {
                    entry.BoxedValue = entry.BoxedDefaultValue;
                    ShowToast((string)_resetDoneStr + entry.Name);
                }
            }

            private void ShowToast(string message)
            {
                _toastMessage = message;
                _toastEndTime = Time.realtimeSinceStartup + ToastDuration;
            }

            private void DrawToast()
            {
                if (string.IsNullOrEmpty(_toastMessage))
                    return;

                float remaining = _toastEndTime - Time.realtimeSinceStartup;
                if (remaining <= 0f)
                {
                    _toastMessage = null;
                    return;
                }

                if (_toastStyle == null || _toastStyle.normal.background == null)
                {
                    _toastStyle = new GUIStyle(GUI.skin.box)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset(10, 10, 6, 6)
                    };
                    var bgTex = new Texture2D(1, 1);
                    bgTex.hideFlags = HideFlags.HideAndDontSave;
                    bgTex.SetPixel(0, 0, new Color(0f, 0.3f, 0.25f, 1f));
                    bgTex.Apply();
                    _toastStyle.normal.background = bgTex;
                    _toastStyle.normal.textColor = Color.white;
                }

                float alpha = remaining < ToastFadeDuration ? remaining / ToastFadeDuration : 1f;
                var prevColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha);

                var content = new GUIContent(_toastMessage);
                var size = _toastStyle.CalcSize(content);
                float toastWidth = Mathf.Min(size.x + 20f, _windowRect.width - 20f);
                float toastHeight = size.y + 4f;
                float x = (_windowRect.width - toastWidth) / 2f;
                float y = _windowRect.height - toastHeight - 30f;

                GUI.Label(new Rect(x, y, toastWidth, toastHeight), content, _toastStyle);
                GUI.color = prevColor;
            }
        }
    }
}
