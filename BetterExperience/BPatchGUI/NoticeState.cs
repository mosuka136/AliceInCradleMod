using System;
using System.Collections.Generic;
using System.Linq;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BPatchGUI
{
    /// <summary>提示的内存状态。时钟由 GUI 宿主提供，便于独立验证到期和调用方隔离。</summary>
    internal sealed class NoticeState
    {
        internal const float DefaultDuration = 3f;
        private readonly Func<float> _clock;
        private readonly List<Entry> _entries = new List<Entry>();

        internal sealed class Entry
        {
            internal object Owner;
            internal Translator Message;
            internal bool Persistent;
            internal double ExpiresAt;
        }

        internal NoticeState(Func<float> clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        internal void Show(object owner, Translator message, float duration = DefaultDuration)
        {
            if (float.IsNaN(duration) || float.IsInfinity(duration) || duration <= 0f)
                duration = DefaultDuration;
            Set(owner, message, false, (double)_clock() + duration);
        }

        internal void SetStatus(object owner, Translator message) => Set(owner, message, true, 0d);

        private void Set(object owner, Translator message, bool persistent, double expiresAt)
        {
            Prune();
            var entry = _entries.Find(item => ReferenceEquals(item.Owner, owner) && item.Persistent == persistent);
            if (message == null || (string.IsNullOrWhiteSpace(message.Chinese) && string.IsNullOrWhiteSpace(message.English)))
            {
                if (entry != null)
                    _entries.Remove(entry);
                return;
            }
            if (entry == null)
            {
                entry = new Entry { Owner = owner, Persistent = persistent };
                _entries.Add(entry);
            }
            entry.Message = message;
            entry.ExpiresAt = expiresAt;
        }

        internal void RemoveStatus(object owner) => _entries.RemoveAll(item => ReferenceEquals(item.Owner, owner) && item.Persistent);

        internal void Clear(object owner) => _entries.RemoveAll(item => ReferenceEquals(item.Owner, owner));

        internal void ClearAll() => _entries.Clear();

        internal Entry[] GetVisible()
        {
            Prune();
            // 状态行始终排在限时提示上方；同类提示按首次发布顺序排列。
            return _entries.OrderByDescending(item => item.Persistent).ToArray();
        }

        private void Prune()
        {
            double now = _clock();
            _entries.RemoveAll(item => !item.Persistent && now >= item.ExpiresAt);
        }
    }
}
