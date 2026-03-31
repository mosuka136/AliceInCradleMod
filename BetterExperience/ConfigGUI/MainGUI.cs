using BetterExperience.BepConfigManager;
using BetterExperience.ConfigFileSpace;
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

        private static bool _initialized = false;

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
        private class LoadScenePatch
        {
            private static void Postfix()
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
            private HotkeyInputSystem _hotkey;
            private Rect _windowRect;
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
            private readonly Translator _onStr = new Translator("开启", "On");
            private readonly Translator _offStr = new Translator("关闭", "Off");
            private readonly Translator _resetStr = new Translator("重置", "Reset");

            private void OnGUI()
            {
                if (!_showGUI)
                    return;

                _windowRect = GUI.Window(WindowID, _windowRect, DrawConfigWindow, "Better Experience Mod Configurations");
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
                        _cachedEntryLabelWidth = -1f;
                        float w = Screen.width * 0.3f;
                        float h = Screen.height * 0.8f;
                        _windowRect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
                    }
                }
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

            private static float CalculateLabelWidth(OrderedDictionary tables)
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
                    entry.BoxedValue = GUILayout.Toggle(currentValue, currentValue ? _onStr : _offStr, GUILayout.ExpandWidth(true));
                }
                else if (type == typeof(string))
                {
                    var strValue = entry.BoxedValue as string ?? "";
                    var newStrValue = GUILayout.TextField(strValue, GUILayout.ExpandWidth(true));
                    if (newStrValue != strValue)
                        entry.BoxedValue = newStrValue;
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
                            entry.BoxedValue = parsedValue;
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

                if (GUILayout.Button(entry.BoxedValue.ToString(), GUILayout.ExpandWidth(true)))
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

                    if (entry.ValueType == typeof(LanguageType))
                    {
                        foreach (var value in Enum.GetValues(entry.ValueType))
                        {
                            var lang = (LanguageType)value;
                            if (lang == LanguageType.None || lang == LanguageType.Default)
                                continue;
                            if (GUILayout.Button(lang.GetDescription()))
                            {
                                entry.BoxedValue = value;
                                _openedEnumEntryName = null;
                            }
                        }
                    }
                    else
                    {
                        foreach (var value in Enum.GetValues(entry.ValueType))
                        {
                            if (GUILayout.Button(value.ToString()))
                            {
                                entry.BoxedValue = value;
                                _openedEnumEntryName = null;
                            }
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
                    entry.BoxedValue = entry.BoxedDefaultValue;
            }
        }
    }
}
