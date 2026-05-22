using BetterExperience.BConfigManager;
using BetterExperience.HProvider;
using BetterExperience.HConfigGUI.UI;
using BetterExperience.HotkeyManager;
using BetterExperience.HTranslatorSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace BetterExperience.HConfigGUI
{
    /// <summary>
    /// 配置 GUI 的运行时状态容器。
    /// 它连接配置模型、Unity 输入/IMGUI 服务和临时 UI 状态，不负责实际绘制；绘制逻辑由 <c>UI</c> 命名空间下的 Renderer 完成。
    /// </summary>
    public class ViewModel
    {
        // 文本框和滑条编辑会先进入缓存，延迟应用可减少每帧写配置文件和刷新提示的频率。
        private readonly Dictionary<UiEntryModel, float> _entryDelayApplyTime = new Dictionary<UiEntryModel, float>();

        public UnityProvider UnityService { get; private set; }
        public UnityGuiProvider UnityGuiService { get; private set; }

        public StyleResource StyleResourceInstance { get; private set; }
        public LayoutResource LayoutResourceInstance { get; private set; }

        public Rect WindowRect { get; set; }
        /// <summary>
        /// 配置项标签列宽。-1 表示尚未按当前语言计算，需要在下次绘制前重新测量。
        /// </summary>
        public float LabelWidth { get; set; } = -1f;

        public UiSheetModel Sheet { get; }
        /// <summary>
        /// 当前展开的枚举配置项；同一时间只允许展开一个枚举选择列表。
        /// </summary>
        public UiEntryModel OpenedEnumEntry { get; set; }
        /// <summary>
        /// 当前展开的热键配置项；同一时间只允许编辑一个热键。
        /// </summary>
        public UiEntryModel OpenedHotkeyEntry { get; set; }
        /// <summary>
        /// 当前正在录制的单个热键组合。非空时关闭窗口会被阻止，以免丢失录制状态。
        /// </summary>
        public HotkeyChord RecordingHotkey { get; set; }
        /// <summary>
        /// 用户是否正在与热键录制交互。这个状态用于区分用户是正在按键等待录制，还是已经松开按键结束录制。
        /// </summary>
        public bool IsUserInteracting { get; set; }
        public Hotkey ConfigUIHotkey { get; set; }
        public string ToastMessage { get; set; }
        public float ToastEndTime { get; set; }
        public float ToastDuration { get; set; } = 2f;
        public float ToastFadeDuration { get; set; } = 0.5f;
        public float StringDuration { get; set; } = 0.5f;
        public float NumberDuration { get; set; } = 0.5f;
        public float SliderDuration { get; set; } = 0.5f;

        public ViewModel()
        {
            UnityService = new UnityProvider();
            UnityGuiService = new UnityGuiProvider();

            StyleResourceInstance = new StyleResource(this);
            LayoutResourceInstance = new LayoutResource(this);

            Sheet = new UiSheetModel();

            ConfigUIHotkey = ConfigManager.ConfigUIHotkey.Value;
            Translator.DefaultLanguage = ConfigManager.SetLanguage.Value;

            ConfigManager.ConfigUIHotkey.OnValueChanged += (s, e) => ConfigUIHotkey = e;
            ConfigManager.SetLanguage.OnValueChanged += (s, e) =>
            {
                Translator.DefaultLanguage = e;
                LabelWidth = -1f;
            };
        }

        public void Update(float deltaTime)
        {
            UpdateValueTime(deltaTime);
            RecordHotkey();
        }

        /// <summary>
        /// 推进延迟应用计时，并在计时结束后把缓存值写回配置项。
        /// </summary>
        /// <param name="deltaTime">通常传入 Unity 的未缩放时间，避免游戏暂停影响配置编辑。</param>
        public void UpdateValueTime(float deltaTime)
        {
            var keys = _entryDelayApplyTime.Keys.ToList();
            foreach (var key in keys)
            {
                _entryDelayApplyTime[key] -= deltaTime;
            }

            var entriesToUpdate = _entryDelayApplyTime.Where(kv => kv.Value <= 0f).Select(kv => kv.Key).ToList();
            foreach (var entry in entriesToUpdate)
            {
                if (entry.CacheValue == null)
                {
                    _entryDelayApplyTime.Remove(entry);
                    continue;
                }

                entry.Value = entry.CacheValue;
                _entryDelayApplyTime.Remove(entry);
                ShowToast(TranslatorResource.Changed + entry.Name, ToastDuration);
            }
        }

        /// <summary>
        /// 在热键录制模式下捕获下一次输入。
        /// 输入可以是任意组合的键盘主键、键盘修饰键和游戏手柄按钮，但不支持跨设备组合（例如同时按下键盘和手柄的按钮）。
        /// 捕获到输入后会写入 <see cref="RecordingHotkey"/> 的 <see cref="HotkeyChord.Chord"/> 属性。
        /// </summary>
        public void RecordHotkey()
        {
            var recordingHotkey = RecordingHotkey;
            if (recordingHotkey == null)
                return;

            var isAnyKeyPressed = false;

            var gamepadChord = new GamepadChord(recordingHotkey.UnityService);
            var keyboardChord = new KeyboardChord(recordingHotkey.UnityService);

            var gamepad = UnityService.GamepadCurrent;
            if (gamepad != null)
            {
                foreach (GamepadButton button in Enum.GetValues(typeof(GamepadButton)))
                {
                    var controller = gamepad[button];
                    if (controller == null)
                        continue;

                    if (controller.wasPressedThisFrame || controller.isPressed)
                    {
                        isAnyKeyPressed = true;
                        gamepadChord.AddButton(button);
                    }
                }
            }

            var keyboard = UnityService.KeyboardCurrent;
            if (keyboard == null)
                return;

            foreach (var key in keyboard.allKeys)
            {
                if (key == null)
                    continue;

                if (key.isPressed)
                {
                    isAnyKeyPressed = true;
                    if (KeyboardModifierTrigger.IsModifierKey(key.keyCode))
                    {
                        keyboardChord.AddModifier(key.keyCode);
                        continue;
                    }
                }

                if (key.wasPressedThisFrame)
                {
                    keyboardChord.SetMainKey(key.keyCode);
                    isAnyKeyPressed = true;
                }
            }

            if (!IsUserInteracting && isAnyKeyPressed)
            {
                if (gamepadChord.IsValid)
                    recordingHotkey.Chord = gamepadChord;
                else
                    recordingHotkey.Chord = keyboardChord;

                IsUserInteracting = true;
                return;
            }
            else if (IsUserInteracting && !isAnyKeyPressed)
            {
                if (!recordingHotkey.IsValid)
                    recordingHotkey.Chord = null;

                IsUserInteracting = false;
                return;
            }

            if (keyboardChord.IsValid || (!recordingHotkey.IsValid && keyboardChord.HasAnyKey))
                recordingHotkey.Chord = keyboardChord;
            else if (gamepadChord.IsValid && gamepadChord.Count >= (recordingHotkey.Chord as GamepadChord)?.Count)
                recordingHotkey.Chord = gamepadChord;

            IsUserInteracting = isAnyKeyPressed;
        }

        /// <summary>
        /// 设置配置项值。
        /// </summary>
        /// <param name="entry">目标 UI 配置项。</param>
        /// <param name="value">要写入的值，类型必须与配置项兼容。</param>
        /// <param name="delaySec">大于 0 时先缓存，延迟到计时结束再写入。</param>
        public void SetValue(UiEntryModel entry, object value, float delaySec = 0f)
        {
            if (entry == null)
                return;

            if (delaySec > 0f)
            {
                _entryDelayApplyTime[entry] = delaySec;
                entry.CacheValue = value;
                return;
            }
            entry.Value = value;
            ShowToast(TranslatorResource.Changed + entry.Name, ToastDuration);
        }

        public void ResetValue(UiEntryModel entry)
        {
            if (entry == null)
                return;

            _entryDelayApplyTime.Remove(entry);
            entry.Value = entry.DefaultValue;
            ShowToast(TranslatorResource.ResetDone + entry.Name, ToastDuration);
        }

        public void ShowToast(string message, float duration)
        {
            ToastMessage = message;
            ToastEndTime = UnityService.RealtimeSinceStartup + duration;
        }
    }
}
