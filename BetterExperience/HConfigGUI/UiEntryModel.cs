using BetterExperience.HConfigSpace;
using BetterExperience.HTranslatorSpace;
using System;

namespace BetterExperience.HConfigGUI
{
    /// <summary>
    /// 配置项在 GUI 层的适配模型。
    /// 它把非泛型配置接口包装为可统一渲染的对象，并保存编辑过程中的临时缓存。
    /// </summary>
    public class UiEntryModel
    {
        private IConfigEntry _entry;

        public Translator Name => _entry.Name;
        public Translator Description => _entry.Description;
        public Type ValueType => _entry.ValueType;
        /// <summary>
        /// 当前配置值。赋值成功后会清理 GUI 缓存，确保下一帧显示真实配置状态。
        /// </summary>
        public object Value
        {
            get => _entry.BoxedValue;
            set
            {
                if (value == null)
                    return;
                _entry.BoxedValue = value;
                CacheValue = null;
                CacheValueString = string.Empty;
            }
        }
        public object DefaultValue => _entry.BoxedDefaultValue;
        /// <summary>
        /// 控件编辑中的临时值；延迟提交和复杂控件会先写入这里。
        /// </summary>
        public object CacheValue { get; set; } = null;
        /// <summary>
        /// 文本输入缓存。数值控件需要保留“暂时不可解析”的输入文本，避免用户输入过程中被强制还原。
        /// </summary>
        public string CacheValueString {  get; set; } = string.Empty;

        /// <summary>
        /// 与该配置项关联的 UI 元数据，例如滑条范围。
        /// </summary>
        public IUiMetadata Metadata { get; }

        public UiEntryModel(IConfigEntry entry)
        {
            _entry = entry;
            Metadata = UiMetadataHelper.GetMetadata(entry);
            _entry.OnValueChangedBase += (s, e) =>
            {
                CacheValue = null;
                CacheValueString = string.Empty;
            };
        }
    }
}
