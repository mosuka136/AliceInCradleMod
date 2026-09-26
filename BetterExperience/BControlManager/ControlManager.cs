using BetterExperience.BLogSpace;
using System;
using UnityModBase.HControlSpace;
using UnityModBase.HGuiSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BControlManager
{
    /// <summary>
    /// 声明只影响当前游戏状态的实时控制表和条目，供控制界面在游戏中查看和即时修改数值。
    /// 与 <see cref="BConfigManager.ConfigManager"/> 的分工：配置项跨会话持久化到文件，
    /// 控制条目不落盘，只在当前游戏会话内生效。
    /// 条目本身不实现读写逻辑：读取委托给 HPatches 各补丁类的 Get* 方法，
    /// 用户在界面提交新值时通过 OnValueChanged 转发给对应 Set* 方法写回游戏。
    /// 各 Get* 方法在游戏对象不可用（未进入游戏、未读档等）时返回数值 -1 或布尔 false 作为占位值；
    /// 滑条条目的最小值取对应 Set* 方法接受的最小有效值，占位 -1 只影响滑条显示（重绘仅夹取显示，不写回）。
    /// 生命周期：由插件入口在 Harmony 补丁注册完成后调用一次 <see cref="Initialize"/>，
    /// 之后不再变更结构；ControlService 不提供并发保护，条目刷新与界面写入须由 GUI 宿主在主线程驱动。
    /// 具体控制条目按功能拆分到同名 partial 文件中，避免单个文件过长。
    /// </summary>
    internal static partial class ControlManager
    {
        private static bool _initialized;

        /// <summary>
        /// 创建控制表并绑定全部实时控制条目。
        /// 通过 <see cref="_initialized"/> 保证幂等，重复调用直接返回；
        /// 各分区的建表与绑定在 partial 文件内各自捕获异常，单个分区失败只记录日志，不影响其余分区。
        /// </summary>
        internal static void Initialize()
        {
            if (_initialized)
                return;

            InitializePlayer();
            InitializeCane();
            InitializeMap();
            InitializeWeather();
            InitializeCurrency();
            InitializeMiniGame();
            InitializeGive();
            InitializePortrait();

            _initialized = true;
            BLog.Debug("Runtime control manager initialized.");
        }

        /// <summary>
        /// 绑定一个实时控制条目并接线读写。
        /// 默认刷新策略为“界面可见时每秒读取一次”，避免每帧执行反射扫描；
        /// 给予类文本/开关可改用不刷新或每帧刷新。
        /// <c>OnValueChanged</c> 只在用户通过界面提交新值时触发（定时刷新缓存不触发），
        /// 因此可直接把新值转发给 <paramref name="valueSetter"/> 写回游戏，不会形成回环。
        /// </summary>
        private static ControlEntry<T> Bind<T>(
            string tableKey,
            string key,
            Func<T> valueGetter,
            Action<T> valueSetter,
            Translator name,
            Translator description,
            IUiMetadata metadata = null,
            ControlUpdatePolicy policy = null)
        {
            var controlEntry = BService.Control.Bind(
                tableKey,
                key,
                valueGetter,
                policy ?? ControlUpdatePolicy.WhenVisibleEverySecond,
                name,
                description,
                metadata
                );

            controlEntry.OnValueChanged += (sender, value) => valueSetter(value);
            return controlEntry;
        }

        /// <summary>
        /// 绑定一个脉冲式开关：读取值恒为 false，界面每帧刷新，用户打开开关时触发一次写回。
        /// </summary>
        private static ControlEntry<bool> BindPulse(
            string tableKey,
            string key,
            Action<bool> valueSetter,
            Translator name,
            Translator description)
        {
            return Bind(
                tableKey,
                key,
                () => false,
                valueSetter,
                name,
                description,
                policy: ControlUpdatePolicy.WhenVisibleEveryFrame);
        }

    }
}
