using System;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 跟踪临时收纳数量与游戏实际获得的增量，序列化时恢复后者。
    /// 由主线程串行调用；支持同一背包嵌套序列化。
    /// </summary>
    internal sealed class BottleHolderCountOverride
    {
        // 与新版 ItemStorage.ObtainInfo.AddCount 的单品级上限一致。
        internal const int MaxCount = 59994;

        private readonly Func<int> _readCount;
        private readonly Action<int> _writeCount;
        private int _originalCount;
        private int _appliedCount;
        private int _resumeCount;
        private int _saveDepth;

        internal BottleHolderCountOverride(Func<int> readCount, Action<int> writeCount)
        {
            _readCount = readCount ?? throw new ArgumentNullException(nameof(readCount));
            _writeCount = writeCount ?? throw new ArgumentNullException(nameof(writeCount));
            _originalCount = _appliedCount = _readCount();
        }

        internal void Apply(int count)
        {
            if (count < 0 || count > MaxCount)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (_saveDepth != 0)
                throw new InvalidOperationException("Cannot change bottle holder count during serialization.");

            CaptureAcquiredCount();
            try
            {
                _writeCount(count);
            }
            finally
            {
                // 写入或刷新中途失败时，已经改变的数量仍属于 Mod 增量。
                _appliedCount = _readCount();
            }
        }

        internal void SuspendForSave()
        {
            if (_saveDepth++ != 0)
                return;

            _resumeCount = _readCount();
            CaptureAcquiredCount();
            _writeCount(_originalCount);
        }

        internal void ResumeAfterSave()
        {
            if (_saveDepth == 0 || --_saveDepth != 0)
                return;

            try
            {
                _writeCount(_resumeCount);
            }
            finally
            {
                _appliedCount = _readCount();
            }
        }

        private void CaptureAcquiredCount()
        {
            // 两次 Mod 写入之间新增或消耗的收纳物品属于游戏进度，应保留在存档里。
            var count = (long)_originalCount + _readCount() - _appliedCount;
            _originalCount = (int)Math.Max(0L, Math.Min(MaxCount, count));
        }
    }
}
