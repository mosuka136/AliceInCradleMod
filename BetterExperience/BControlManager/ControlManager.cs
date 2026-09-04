using BetterExperience.BLogSpace;
using BetterExperience.Patches;
using nel;
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
    /// 各 Get* 方法在游戏对象不可用（未进入游戏、未读档等）时返回数值 -1 或布尔 false 作为占位值。
    /// 生命周期：由插件入口在 Harmony 补丁注册完成后调用一次 <see cref="Initialize"/>，
    /// 之后不再变更结构；ControlService 不提供并发保护，条目刷新与界面写入须由 GUI 宿主在主线程驱动。
    /// </summary>
    internal static class ControlManager
    {
        private const string SectionPlayer = "Player";
        private const string SectionCane = "Cane";
        private const string SectionMap = "Map";
        private const string SectionWeather = "Weather";
        private const string SectionCurrency = "Currency";

        private static bool _initialized;

        internal static ControlEntry<int> SetBackpackCapacity { get; private set; }
        internal static ControlEntry<int> SetBottleHolderCount { get; private set; }
        internal static ControlEntry<int> SetPlayerHp { get; private set; }
        internal static ControlEntry<int> SetPlayerMp { get; private set; }
        internal static ControlEntry<int> SetPlayerEp { get; private set; }
        internal static ControlEntry<int> SetPlayerMaxHp { get; private set; }
        internal static ControlEntry<int> SetPlayerMaxMp { get; private set; }
        internal static ControlEntry<int> SetPlayerMaxSatiety { get; private set; }
        internal static ControlEntry<int> SetOverChargeSlotCount { get; private set; }
        internal static ControlEntry<int> SetEnhancerSlotCount { get; private set; }

        internal static ControlEntry<float> SetCaneSwingSpeed { get; private set; }
        internal static ControlEntry<float> SetCaneCastSpeed { get; private set; }
        internal static ControlEntry<float> SetCaneBalance { get; private set; }
        internal static ControlEntry<float> SetCaneEfficiency { get; private set; }
        internal static ControlEntry<float> SetCaneRetention { get; private set; }
        internal static ControlEntry<float> SetCaneLockOn { get; private set; }
        internal static ControlEntry<float> SetCaneLongRange { get; private set; }
        internal static ControlEntry<float> SetCaneShortRange { get; private set; }
        internal static ControlEntry<float> SetCaneReach { get; private set; }
        internal static ControlEntry<float> SetCaneNearPower { get; private set; }
        internal static ControlEntry<float> SetCaneNearShotgunPower { get; private set; }
        internal static ControlEntry<float> SetCaneStability { get; private set; }
        internal static ControlEntry<float> SetCaneManaSplashRatio { get; private set; }
        internal static ControlEntry<float> SetCaneCastspeedOverhold { get; private set; }
        internal static ControlEntry<float> SetCaneDrainAfterLock { get; private set; }
        internal static ControlEntry<float> SetCaneCastspeed { get; private set; }
        internal static ControlEntry<float> SetCaneMagicPrepareSpeed { get; private set; }

        internal static ControlEntry<int> SetDangerLevel { get; private set; }

        internal static ControlEntry<bool> SetWeatherWind { get; private set; }
        internal static ControlEntry<bool> SetWeatherThunder { get; private set; }
        internal static ControlEntry<bool> SetWeatherMist { get; private set; }
        internal static ControlEntry<bool> SetWeatherDrought { get; private set; }
        internal static ControlEntry<bool> SetWeatherDenseMist { get; private set; }
        internal static ControlEntry<bool> SetWeatherPlague { get; private set; }

        internal static ControlEntry<long> SetCurrencyGoldCount { get; private set; }
        internal static ControlEntry<long> SetCurrencyCraftsCount { get; private set; }
        internal static ControlEntry<long> SetCurrencyJuiceCount { get; private set; }

        /// <summary>
        /// 创建控制表并绑定全部实时控制条目。
        /// 通过 <see cref="_initialized"/> 保证幂等，重复调用直接返回；
        /// 初始化失败只记录日志不抛出，已成功绑定的条目仍可使用。
        /// </summary>
        internal static void Initialize()
        {
            if (_initialized)
                return;

            try
            {
                CreateTables();
                InitializePlayerControls();
                InitializeCaneControls();
                InitializeMapControls();
                InitializeWeatherControls();
                InitializeCurrencyControls();

                _initialized = true;
                BLog.Debug("Runtime control manager initialized.");
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize runtime control manager.", ex);
            }
        }

        private static void CreateTables()
        {
            BService.Control.CreateTable(
                SectionPlayer,
                new Translator(chinese: "玩家", english: "Player"),
                new Translator(
                    chinese: "查看和修改当前游戏中的玩家状态。",
                    english: "View and modify player state in the current game."
                    )
                );
            BService.Control.CreateTable(
                SectionCane,
                new Translator(chinese: "法杖", english: "Cane"),
                new Translator(
                    chinese: "查看和修改当前装备法杖的属性。",
                    english: "View and modify attributes of the currently equipped cane."
                    )
                );
            BService.Control.CreateTable(
                SectionMap,
                new Translator(chinese: "地图", english: "Map"),
                new Translator(
                    chinese: "查看和修改当前游戏中的地图状态。",
                    english: "View and modify map state in the current game."
                    )
                );
            BService.Control.CreateTable(
                SectionWeather,
                new Translator(chinese: "天气", english: "Weather"),
                new Translator(
                    chinese: "查看和修改当前游戏中的天气状态。",
                    english: "View and modify weather in the current game."
                    )
                );
            BService.Control.CreateTable(
                SectionCurrency,
                new Translator(chinese: "货币", english: "Currency"),
                new Translator(
                    chinese: "查看和修改当前游戏中的货币数量。",
                    english: "View and modify currency amounts in the current game."
                    )
                );
        }

        private static void InitializePlayerControls()
        {
            SetBackpackCapacity = Bind(
                SectionPlayer,
                nameof(SetBackpackCapacity),
                HPatches.SetBackpackCapacityPatch.GetBackpackCapacity,
                HPatches.SetBackpackCapacityPatch.SetBackpackCapacity,
                new Translator(chinese: "设置背包容量", english: "Set Backpack Capacity"),
                new Translator(
                    chinese: "设置当前背包容量。",
                    english: "Set the current backpack capacity."
                    )
                );
            SetBottleHolderCount = Bind(
                SectionPlayer,
                nameof(SetBottleHolderCount),
                HPatches.SetBottleHolderCountPatch.GetBottleHolderCount,
                HPatches.SetBottleHolderCountPatch.SetBottleHolderCount,
                new Translator(chinese: "设置空瓶收纳数量", english: "Set Bottle Holder Count"),
                new Translator(
                    chinese: "设置当前空瓶收纳槽位数量。",
                    english: "Set the current bottle holder count."
                    )
                );
            SetPlayerHp = BindPlayerValue(
                nameof(SetPlayerHp),
                HPatches.SetHpMpEpPatch.GetHp,
                HPatches.SetHpMpEpPatch.SetHp,
                new Translator(chinese: "设置玩家 HP", english: "Set Player HP")
                );
            SetPlayerMp = BindPlayerValue(
                nameof(SetPlayerMp),
                HPatches.SetHpMpEpPatch.GetMp,
                HPatches.SetHpMpEpPatch.SetMp,
                new Translator(chinese: "设置玩家 MP", english: "Set Player MP")
                );
            SetPlayerEp = BindPlayerValue(
                nameof(SetPlayerEp),
                HPatches.SetHpMpEpPatch.GetEp,
                HPatches.SetHpMpEpPatch.SetEp,
                new Translator(chinese: "设置玩家 EP", english: "Set Player EP")
                );
            SetPlayerMaxHp = BindPlayerValue(
                nameof(SetPlayerMaxHp),
                HPatches.SetHpMpEpPatch.GetMaxHp,
                HPatches.SetHpMpEpPatch.SetMaxHp,
                new Translator(chinese: "设置玩家最大 HP", english: "Set Player Max HP")
                );
            SetPlayerMaxMp = BindPlayerValue(
                nameof(SetPlayerMaxMp),
                HPatches.SetHpMpEpPatch.GetMaxMp,
                HPatches.SetHpMpEpPatch.SetMaxMp,
                new Translator(chinese: "设置玩家最大 MP", english: "Set Player Max MP")
                );
            SetPlayerMaxSatiety = Bind(
                SectionPlayer,
                nameof(SetPlayerMaxSatiety),
                HPatches.SetMaxSatietyPatch.GetMaxSatiety,
                HPatches.SetMaxSatietyPatch.SetMaxSatiety,
                new Translator(chinese: "设置玩家最大饱食度", english: "Set Player Max Satiety"),
                new Translator(
                    chinese: "设置当前玩家最大饱食度。",
                    english: "Set the current player max satiety."
                    ),
                new UiSliderMetadata(-1f, 100f, 1f)
                );
            SetOverChargeSlotCount = Bind(
                SectionPlayer,
                nameof(SetOverChargeSlotCount),
                HPatches.SetOverChargeSlotCountPatch.GetOverChargeSlotCount,
                HPatches.SetOverChargeSlotCountPatch.SetOverChargeSlotCount,
                new Translator(chinese: "设置过充插槽数量", english: "Set Over Charge Slot Count"),
                new Translator(
                    chinese: "设置当前过充插槽数量。",
                    english: "Set the current overcharge slot count."
                    ),
                new UiSliderMetadata(-1f, 10f, 1f)
                );
            SetEnhancerSlotCount = Bind(
                SectionPlayer,
                nameof(SetEnhancerSlotCount),
                HPatches.SetEnhancerSlotCountPatch.GetEnhancerSlotCount,
                HPatches.SetEnhancerSlotCountPatch.SetEnhancerSlotCount,
                new Translator(chinese: "设置强化插槽数量", english: "Set Enhancer Slot Count"),
                new Translator(
                    chinese: "设置当前强化插槽数量。",
                    english: "Set the current enhancer slot count."
                    ),
                new UiSliderMetadata(-1f, 20f, 1f)
                );
        }

        private static void InitializeCaneControls()
        {
            SetCaneSwingSpeed = BindCane(
                nameof(SetCaneSwingSpeed),
                HPatches.SetCaneAttributePatch.GetSwingSpeed,
                HPatches.SetCaneAttributePatch.SetSwingSpeed,
                new Translator(chinese: "设置近战攻击速度", english: "Set Cane Swing Speed"),
                255f
                );
            SetCaneCastSpeed = BindCane(
                nameof(SetCaneCastSpeed),
                HPatches.SetCaneAttributePatch.GetCastSpeed,
                HPatches.SetCaneAttributePatch.SetCastSpeed,
                new Translator(chinese: "设置咏唱速度", english: "Set Cane Cast Speed"),
                255f
                );
            SetCaneBalance = BindCane(
                nameof(SetCaneBalance),
                HPatches.SetCaneAttributePatch.GetBalance,
                HPatches.SetCaneAttributePatch.SetBalance,
                new Translator(chinese: "设置魔力亲和性", english: "Set Cane Balance"),
                255f
                );
            SetCaneEfficiency = BindCane(
                nameof(SetCaneEfficiency),
                HPatches.SetCaneAttributePatch.GetEfficiency,
                HPatches.SetCaneAttributePatch.SetEfficiency,
                new Translator(chinese: "设置魔力消耗效率", english: "Set Cane Efficiency"),
                169f
                );
            SetCaneRetention = BindCane(
                nameof(SetCaneRetention),
                HPatches.SetCaneAttributePatch.GetRetention,
                HPatches.SetCaneAttributePatch.SetRetention,
                new Translator(chinese: "设置魔力稳定性", english: "Set Cane Retention"),
                255f
                );
            SetCaneLockOn = BindCane(
                nameof(SetCaneLockOn),
                HPatches.SetCaneAttributePatch.GetLockOn,
                HPatches.SetCaneAttributePatch.SetLockOn,
                new Translator(chinese: "设置锁定性能", english: "Set Cane Lock-On"),
                255f
                );
            SetCaneLongRange = BindCane(
                nameof(SetCaneLongRange),
                HPatches.SetCaneAttributePatch.GetLongRange,
                HPatches.SetCaneAttributePatch.SetLongRange,
                new Translator(chinese: "设置射击威力", english: "Set Cane Long Range"),
                255f
                );
            SetCaneShortRange = BindCane(
                nameof(SetCaneShortRange),
                HPatches.SetCaneAttributePatch.GetShortRange,
                HPatches.SetCaneAttributePatch.SetShortRange,
                new Translator(chinese: "设置近战威力", english: "Set Cane Short Range"),
                255f
                );
            SetCaneReach = BindCane(
                nameof(SetCaneReach),
                HPatches.SetCaneAttributePatch.GetReach,
                HPatches.SetCaneAttributePatch.SetReach,
                new Translator(chinese: "设置近战攻击距离", english: "Set Cane Reach"),
                255f
                );
            SetCaneNearPower = BindCane(
                nameof(SetCaneNearPower),
                HPatches.SetCaneAttributePatch.GetNearPower,
                HPatches.SetCaneAttributePatch.SetNearPower,
                new Translator(chinese: "设置 Near Power", english: "Set Cane Near Power"),
                255f
                );
            SetCaneNearShotgunPower = BindCane(
                nameof(SetCaneNearShotgunPower),
                HPatches.SetCaneAttributePatch.GetNearShotgunPower,
                HPatches.SetCaneAttributePatch.SetNearShotgunPower,
                new Translator(chinese: "设置 Near Shotgun Power", english: "Set Cane Near Shotgun Power"),
                255f
                );
            SetCaneStability = BindCane(
                nameof(SetCaneStability),
                HPatches.SetCaneAttributePatch.GetStability,
                HPatches.SetCaneAttributePatch.SetStability,
                new Translator(chinese: "设置 Stability", english: "Set Cane Stability"),
                255f
                );
            SetCaneManaSplashRatio = BindCane(
                nameof(SetCaneManaSplashRatio),
                HPatches.SetCaneAttributePatch.GetManaSplashRatio,
                HPatches.SetCaneAttributePatch.SetManaSplashRatio,
                new Translator(chinese: "设置 Mana Splash Ratio", english: "Set Cane Mana Splash Ratio"),
                255f
                );
            SetCaneCastspeedOverhold = BindCane(
                nameof(SetCaneCastspeedOverhold),
                HPatches.SetCaneAttributePatch.GetCastspeedOverhold,
                HPatches.SetCaneAttributePatch.SetCastspeedOverhold,
                new Translator(chinese: "设置 Castspeed Overhold", english: "Set Cane Castspeed Overhold"),
                255f
                );
            SetCaneDrainAfterLock = BindCane(
                nameof(SetCaneDrainAfterLock),
                HPatches.SetCaneAttributePatch.GetDrainAfterLock,
                HPatches.SetCaneAttributePatch.SetDrainAfterLock,
                new Translator(chinese: "设置 Drain After Lock", english: "Set Cane Drain After Lock"),
                255f
                );
            SetCaneCastspeed = BindCane(
                nameof(SetCaneCastspeed),
                HPatches.SetCaneAttributePatch.GetCastspeed,
                HPatches.SetCaneAttributePatch.SetCastspeed,
                new Translator(chinese: "设置 Castspeed", english: "Set Cane Castspeed"),
                255f
                );
            SetCaneMagicPrepareSpeed = BindCane(
                nameof(SetCaneMagicPrepareSpeed),
                HPatches.SetCaneAttributePatch.GetMagicPrepareSpeed,
                HPatches.SetCaneAttributePatch.SetMagicPrepareSpeed,
                new Translator(chinese: "设置 Magic Prepare Speed", english: "Set Cane Magic Prepare Speed"),
                255f
                );
        }

        private static void InitializeMapControls()
        {
            SetDangerLevel = Bind(
                SectionMap,
                nameof(SetDangerLevel),
                HPatches.SetDangerLevelPatch.GetDangerLevel,
                HPatches.SetDangerLevelPatch.SetDangerLevel,
                new Translator(chinese: "设置危险度", english: "Set Danger Level"),
                new Translator(
                    chinese: "设置当前游戏的危险度。",
                    english: "Set the danger level in the current game."
                    ),
                new UiSliderMetadata(-1f, 160f, 1f)
                );
        }

        private static void InitializeWeatherControls()
        {
            SetWeatherWind = BindWeather(
                nameof(SetWeatherWind),
                WeatherItem.WEATHER.WIND,
                new Translator(chinese: "设置天气旋风", english: "Set Weather Wind")
                );
            SetWeatherThunder = BindWeather(
                nameof(SetWeatherThunder),
                WeatherItem.WEATHER.THUNDER,
                new Translator(chinese: "设置天气雷暴", english: "Set Weather Thunder")
                );
            SetWeatherMist = BindWeather(
                nameof(SetWeatherMist),
                WeatherItem.WEATHER.MIST,
                new Translator(chinese: "设置天气雾", english: "Set Weather Mist")
                );
            SetWeatherDrought = BindWeather(
                nameof(SetWeatherDrought),
                WeatherItem.WEATHER.DROUGHT,
                new Translator(chinese: "设置天气干旱", english: "Set Weather Drought")
                );
            SetWeatherDenseMist = BindWeather(
                nameof(SetWeatherDenseMist),
                WeatherItem.WEATHER.MIST_DENSE,
                new Translator(chinese: "设置天气浓雾", english: "Set Weather Dense Mist")
                );
            SetWeatherPlague = BindWeather(
                nameof(SetWeatherPlague),
                WeatherItem.WEATHER.PLAGUE,
                new Translator(chinese: "设置天气瘟疫", english: "Set Weather Plague")
                );
        }

        private static void InitializeCurrencyControls()
        {
            SetCurrencyGoldCount = BindCurrency(
                nameof(SetCurrencyGoldCount),
                HPatches.SetCurrencyCountPatch.GetCurrencyGoldCount,
                HPatches.SetCurrencyCountPatch.SetCurrencyGoldCount,
                new Translator(chinese: "设置金币数量", english: "Set Gold Count")
                );
            SetCurrencyCraftsCount = BindCurrency(
                nameof(SetCurrencyCraftsCount),
                HPatches.SetCurrencyCountPatch.GetCurrencyCraftsCount,
                HPatches.SetCurrencyCountPatch.SetCurrencyCraftsCount,
                new Translator(chinese: "设置兑锭数量", english: "Set Crafts Count")
                );
            SetCurrencyJuiceCount = BindCurrency(
                nameof(SetCurrencyJuiceCount),
                HPatches.SetCurrencyCountPatch.GetCurrencyJuiceCount,
                HPatches.SetCurrencyCountPatch.SetCurrencyJuiceCount,
                new Translator(chinese: "设置精萃数量", english: "Set Juice Count")
                );
        }

        /// <summary>
        /// 绑定玩家 HP/MP/EP 等数值属性，统一使用 -1~1000、步进 1 的滑杆。
        /// </summary>
        private static ControlEntry<int> BindPlayerValue(
            string key,
            Func<int> valueGetter,
            Action<int> valueSetter,
            Translator name)
        {
            return Bind(
                SectionPlayer,
                key,
                valueGetter,
                valueSetter,
                name,
                new Translator(
                    chinese: "设置当前玩家属性值。",
                    english: "Set the current player attribute value."
                    ),
                new UiSliderMetadata(-1f, 1000f, 1f)
                );
        }

        /// <summary>
        /// 绑定法杖属性，统一使用下限 -1、步进 0.1 的滑杆；上限因属性而异（多数为 255，魔力消耗效率为 169）。
        /// </summary>
        private static ControlEntry<float> BindCane(
            string key,
            Func<float> valueGetter,
            Action<float> valueSetter,
            Translator name,
            float sliderMax)
        {
            return Bind(
                SectionCane,
                key,
                valueGetter,
                valueSetter,
                name,
                new Translator(
                    chinese: "设置当前装备法杖的属性值。",
                    english: "Set an attribute of the currently equipped cane."
                    ),
                new UiSliderMetadata(-1f, sliderMax, 0.1f)
                );
        }

        /// <summary>
        /// 绑定货币数量，统一使用 -1~1000000、步进 1 的滑杆。
        /// </summary>
        private static ControlEntry<long> BindCurrency(
            string key,
            Func<long> valueGetter,
            Action<long> valueSetter,
            Translator name)
        {
            return Bind(
                SectionCurrency,
                key,
                valueGetter,
                valueSetter,
                name,
                new Translator(
                    chinese: "设置当前游戏中的货币数量。",
                    english: "Set the currency amount in the current game."
                    ),
                new UiSliderMetadata(-1f, 1000000f, 1f)
                );
        }

        /// <summary>
        /// 绑定天气开关，读取当前 NightController 状态并把界面修改即时写回游戏。
        /// </summary>
        private static ControlEntry<bool> BindWeather(
            string key,
            WeatherItem.WEATHER weather,
            Translator name)
        {
            return Bind(
                SectionWeather,
                key,
                () => HPatches.SetWeatherPatch.GetWeather(weather),
                value => HPatches.SetWeatherPatch.SetWeather(weather, value),
                name,
                new Translator(
                    chinese: "设置当前游戏中的天气状态。",
                    english: "Set the weather state in the current game."
                    )
                );
        }

        /// <summary>
        /// 绑定一个实时控制条目并接线读写：
        /// 刷新策略固定为“界面可见时每秒读取一次”，避免每帧执行反射扫描；
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
            IUiMetadata metadata = null)
        {
            var controlEntry = BService.Control.Bind(
                tableKey,
                key,
                valueGetter,
                ControlUpdatePolicy.WhenVisibleEverySecond,
                name,
                description,
                metadata
                );

            controlEntry.OnValueChanged += (sender, value) => valueSetter(value);
            return controlEntry;
        }

    }
}
