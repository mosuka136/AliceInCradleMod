using BetterExperience.BConfigManager;
using UnityEngine;
using UnityModBase.HClassAttribute;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BPatchGUI
{
    /// <summary>
    /// 各补丁共用的屏幕提示组件，由启动注册器创建，调用方无需创建组件或转发 OnGUI。
    /// 所有接口限 Unity 主线程调用；计时使用不受暂停影响的 unscaledTime。
    /// 同一 owner 各保留一条限时提示和一条状态，重复发布会更新内容；owner 按引用身份隔离。
    /// 不传 owner 时共用默认通道，建议各模块持有自己的 static readonly object。
    /// </summary>
    [RegisterOnGameBoot]
    public sealed class NoticeGUI : MonoBehaviour
    {
        private static readonly NoticeState State = new NoticeState(() => Time.unscaledTime);
        private GUIStyle _style;

        /// <summary>发布限时提示，默认 3 秒；无效时长回退到默认值，空内容清除该调用方的限时提示。</summary>
        public static void Show(string message, float duration = NoticeState.DefaultDuration, object owner = null)
            => Show(new Translator(message, message), duration, owner);

        /// <summary>发布双语限时提示，显示期间跟随 Translator 的当前语言。</summary>
        public static void Show(Translator message, float duration = NoticeState.DefaultDuration, object owner = null)
            => State.Show(owner, message, duration);

        /// <summary>设置持续显示的状态；再次设置替换该调用方的状态，空内容表示移除。</summary>
        public static void SetStatus(object owner, string message) => SetStatus(owner, new Translator(message, message));

        /// <summary>设置持续显示的双语状态，直到调用方主动移除。</summary>
        public static void SetStatus(object owner, Translator message) => State.SetStatus(owner, message);

        /// <summary>仅移除该调用方的持续状态，保留限时提示。</summary>
        public static void RemoveStatus(object owner) => State.RemoveStatus(owner);

        /// <summary>清除该调用方的状态和限时提示，不影响其他调用方；null 对应默认通道。</summary>
        public static void Clear(object owner = null) => State.Clear(owner);

        /// <summary>清除所有调用方的提示，供插件停用或卸载时使用。</summary>
        public static void ClearAll() => State.ClearAll();

        private void OnGUI()
        {
            if (ConfigManager.EnableBetterExperience?.Value != true)
            {
                ClearAll();
                return;
            }
            if (Event.current.type != EventType.Repaint)
                return;

            var entries = State.GetVisible();
            if (entries.Length == 0)
                return;
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = true,
                    richText = false,
                    padding = new RectOffset(10, 10, 6, 6)
                };
            }

            const float margin = 12f;
            float width = Mathf.Min(580f, Screen.width - margin * 2f);
            if (width <= 0f)
                return;
            float y = margin;
            foreach (var entry in entries)
            {
                string text = entry.Message.ToString();
                if (string.IsNullOrWhiteSpace(text))
                    continue;
                var content = new GUIContent(text);
                float height = Mathf.Max(28f, _style.CalcHeight(content, width));
                if (y + height > Screen.height - margin)
                    break;
                GUI.Box(new Rect(margin, y, width, height), content, _style);
                y += height + 4f;
            }
        }

        private void OnDisable() => ClearAll();
    }
}
