using BetterExperience.HProvider;
using BetterExperience.HConfigSpace;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterExperience.HotkeyManager
{
    /// <summary>
    /// 表示一个可由多个按键组合触发的热键配置。
    /// 配置文件格式使用逗号分隔多个组合，例如 <c>Ctrl+F1,GamepadStart</c>；任意一个组合在当前帧按下即视为触发。
    /// </summary>
    public class Hotkey : IConfigEntryAdapter
    {
        private static readonly UnityProvider _defaultUnityService = new UnityProvider();

        /// <summary>
        /// 多个组合在配置文本中的分隔符。
        /// </summary>
        public const char Separator = ',';

        public UnityProvider UnityService { get; }

        /// <summary>
        /// 可触发该热键的组合列表。列表为空时不会触发。
        /// </summary>
        public List<HotkeyChord> Hotkeys { get; set; }

        public int Count => Hotkeys.Count;

        /// <summary>
        /// 临时禁用标记。GUI 录制热键时会把旧值标记为无效，避免录制过程中旧热键继续触发。
        /// </summary>
        public bool Valid { get; set; } = true;

        public Hotkey()
        {
            UnityService = _defaultUnityService;
            Hotkeys = new List<HotkeyChord>();
        }

        public Hotkey(UnityProvider unityService)
        {
            UnityService = unityService;
            Hotkeys = new List<HotkeyChord>();
        }

        public Hotkey(string hotkey, UnityProvider unityService)
        {
            UnityService = unityService;
            Hotkeys = new List<HotkeyChord>();
            if (!TryParse(hotkey))
                throw new ArgumentException($"Invalid hotkey string: {hotkey}");
        }

        public Hotkey(Hotkey hotkey, UnityProvider unityService)
        {
            UnityService = unityService;
            Hotkeys = hotkey.Hotkeys.Select(h => new HotkeyChord(UnityService, h.MainKey, h.Modifiers.ToArray())).ToList();
        }

        public Hotkey(UnityProvider unityService, params HotkeyChord[] hotkeys)
        {
            UnityService = unityService;
            Hotkeys = hotkeys.ToList();
        }

        public Hotkey(UnityProvider unityService, IHotkeyTrigger main, params IHotkeyTrigger[] modifiers)
        {
            UnityService = unityService;
            var chord = new HotkeyChord(UnityService, main, modifiers);
            Hotkeys = new List<HotkeyChord> { chord };
        }

        public bool WasPressedThisFrame()
        {
            if (!Valid)
                return false;

            foreach (var ch in Hotkeys)
            {
                if (ch.WasPressedThisFrame())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 添加一个热键组合；同一对象引用不会重复加入。
        /// </summary>
        public void Add(HotkeyChord chord)
        {
            if (chord != null && !Hotkeys.Contains(chord))
                Hotkeys.Add(chord);
        }

        public void Remove(HotkeyChord chord)
        {
            if (chord != null)
                Hotkeys.Remove(chord);
        }

        public void RemoveInvalidHotkey()
        {
            Hotkeys.RemoveAll(h => !h.IsValid || h == null);
        }

        /// <summary>
        /// 比较两个热键是否包含相同组合。
        /// 组合顺序不参与比较，因此适合判断 GUI 编辑后的配置是否实际发生变化。
        /// </summary>
        public bool HasSameHotkey(Hotkey other)
        {
            if (other == null)
                return false;

            var thisChords = new List<string>(Hotkeys.Select(h => h.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)));
            var otherChords = new List<string>(other.Hotkeys.Select(h => h.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)));

            thisChords.Sort();
            otherChords.Sort();

            return thisChords.SequenceEqual(otherChords);
        }

        /// <summary>
        /// 从配置文本解析热键。
        /// </summary>
        /// <param name="text">逗号分隔的组合文本。</param>
        /// <returns>解析是否成功；失败时当前列表可能已经被清空或部分填充。</returns>
        public bool TryParse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                HLog.Debug("Failed to parse hotkey: input is empty.");
                return false;
            }

            Hotkeys.Clear();

            var chordStrings = text.Split(Separator);
            foreach (var ch in chordStrings)
            {
                var chordStr = ch.Trim();
                if (string.IsNullOrWhiteSpace(chordStr))
                    continue;

                var chord = new HotkeyChord(UnityService);
                if (!chord.TryParse(chordStr))
                {
                    HLog.Debug($"Failed to parse hotkey chord: {chordStr}");
                    return false;
                }
                Hotkeys.Add(chord);
            }

            return true;
        }

        /// <summary>
        /// 将热键编码为配置文件文本。
        /// </summary>
        public override string ToString()
        {
            return string.Join(Separator.ToString(), Hotkeys.Select(h => h.ToString()).Where(s => !string.IsNullOrEmpty(s)));
        }

        public ConfigFileResult<string> Encode()
        {
            return ToString();
        }

        public ConfigFileResult<object> Decode(string content)
        {
            if (TryParse(content))
                return this;
            return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Failed to decode Hotkey"));
        }

        public ConfigFileResult<string> EncodeValueType()
        {
            return "Hotkey";
        }
    }
}
