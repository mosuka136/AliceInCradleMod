using BetterExperience.HClassAttribute;
using BetterExperience.HConfigFileSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public partial class ConfigManager
    {
        public static ConfigEntry<bool> EnableBeingAttacked { get; private set; }
        public static ConfigEntry<bool> EnableNoHpDamage { get; private set; }
        public static ConfigEntry<bool> EnableNoMpDamage { get; private set; }
        public static ConfigEntry<bool> EnableNoEpDamage { get; private set; }
        public static ConfigEntry<bool> EnableInfiniteShield { get; private set; }
        public static ConfigEntry<bool> EnableHolyBurstFaint { get; private set; }
        public static ConfigEntry<bool> EnableMpBreak { get; private set; }
        public static ConfigEntry<bool> EnablePressDamage { get; private set; }
        public static ConfigEntry<bool> EnableFallingToGround { get; private set; }
        public static ConfigEntry<bool> EnableAccessWarehouseAnywhere { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalities { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityMpReduce { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityBurstTired { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityClothesBroken { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityOverRunTired { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityShieldBreak { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalitySleep { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityBurned { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityFrozen { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityParalysis { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityConfuse { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityJamming { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityParasitised { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityShamed { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityShamedSplit { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityShamedWet { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityShamedEp { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalitySexercise { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityFrustrated { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityOrgasmAfter { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityEgged { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityLayingEgg { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityDoNotLayEgg { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityNearPee { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityDrunk { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityWebTrapped { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityStone { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityAtkDown { get; private set; }
        public static ConfigEntry<bool> EnablePreloadBackpackCapacity { get; private set; }
        public static ConfigEntry<bool> EnablePreloadBottleHolderCount { get; private set; }
        public static ConfigEntry<bool> EnablePreloadPlayerHp { get; private set; }
        public static ConfigEntry<bool> EnablePreloadPlayerMp { get; private set; }
        public static ConfigEntry<bool> EnablePreloadPlayerEp { get; private set; }
        public static ConfigEntry<bool> EnablePreloadPlayerMaxHp { get; private set; }
        public static ConfigEntry<bool> EnablePreloadPlayerMaxMp { get; private set; }
        public static ConfigEntry<bool> EnablePreloadPlayerMaxSatiety { get; private set; }
        public static ConfigEntry<bool> EnablePreloadOverChargeSlotCount { get; private set; }
        public static ConfigEntry<bool> EnablePreloadEnhancerSlotCount { get; private set; }
        public static ConfigEntry<int> SetBackpackCapacity { get; private set; }
        public static ConfigEntry<int> SetBottleHolderCount { get; private set; }
        [ConfigSlider(-1f, 1000f, 1f)]
        public static ConfigEntry<int> SetPlayerHp { get; private set; }
        [ConfigSlider(-1f, 1000f, 1f)]
        public static ConfigEntry<int> SetPlayerMp { get; private set; }
        [ConfigSlider(-1f, 1000f, 1f)]
        public static ConfigEntry<int> SetPlayerEp { get; private set; }
        [ConfigSlider(-1f, 1000f, 1f)]
        public static ConfigEntry<int> SetPlayerMaxHp { get; private set; }
        [ConfigSlider(-1f, 1000f, 1f)]
        public static ConfigEntry<int> SetPlayerMaxMp { get; private set; }
        [ConfigSlider(-1f, 100f, 1f)]
        public static ConfigEntry<int> SetPlayerMaxSatiety { get; private set; }
        [ConfigSlider(-1f, 10f, 1f)]
        public static ConfigEntry<int> SetOverChargeSlotCount { get; private set; }
        [ConfigSlider(-1f, 20f, 1f)]
        public static ConfigEntry<int> SetEnhancerSlotCount { get; private set; }
        [ConfigSlider(-1f, 10f, 0.1f)]
        public static ConfigEntry<float> SetPlayerWalkSpeed { get; private set; }

        private const string SectionPlayer = "Player";

        public static void InitializePlayerStatus()
        {
            Config.CreateTable(SectionPlayer, new Translator(chinese: "玩家", english: "Player"));

            EnableBeingAttacked = Config.Bind(
                SectionPlayer,
                nameof(EnableBeingAttacked),
                true,
                new Translator(chinese: "启用被攻击", english: "Enable Being Attacked"),
                new Translator(
                    chinese: "启用被攻击。若关闭，玩家将不会受到敌人的攻击，但仍可能触发陷阱。",
                    english: "Enable being attacked. If disabled, players will not be attacked by enemies, but traps may still be triggered."
                )
                );
            EnableNoHpDamage = Config.Bind(
                SectionPlayer,
                nameof(EnableNoHpDamage),
                false,
                new Translator(chinese: "启用无HP伤害", english: "Enable No HP Damage"),
                new Translator(
                    chinese: "启用无 HP 伤害。",
                    english: "Enable no HP damage."
                )
                );
            EnableNoMpDamage = Config.Bind(
                SectionPlayer,
                nameof(EnableNoMpDamage),
                false,
                new Translator(chinese: "启用无MP伤害", english: "Enable No MP Damage"),
                new Translator(
                    chinese: "启用无 MP 伤害。",
                    english: "Enable no MP damage."
                )
                );
            EnableNoEpDamage = Config.Bind(
                SectionPlayer,
                nameof(EnableNoEpDamage),
                false,
                new Translator(chinese: "启用无EP伤害", english: "Enable No EP Damage"),
                new Translator(
                    chinese: "启用无 EP 伤害。玩家“好感度”将不会增加。",
                    english: "Enable no EP damage."
                )
                );
            EnableInfiniteShield = Config.Bind(
                SectionPlayer,
                nameof(EnableInfiniteShield),
                false,
                new Translator(chinese: "启用无限护盾", english: "Enable Infinite Shield"),
                new Translator(
                    chinese: "启用无限护盾。",
                    english: "Enable infinite shield."
                )
                );
            EnableHolyBurstFaint = Config.Bind(
                SectionPlayer,
                nameof(EnableHolyBurstFaint),
                true,
                new Translator(chinese: "启用圣光爆发昏厥", english: "Enable Holy Burst Faint"),
                new Translator(
                    chinese: "启用圣光爆发昏厥。关闭后，玩家将不会因为使用圣光爆发而晕厥。",
                    english: "Enable Holy Burst Faint. When disabled, players will not faint after using Holy Burst."
                )
                );
            EnableMpBreak = Config.Bind(
                SectionPlayer,
                nameof(EnableMpBreak),
                true,
                new Translator(chinese: "启用MP破裂", english: "Enable MP Break"),
                new Translator(
                    chinese: "启用 MP 破裂。关闭后，玩家 MP 槽将不会破裂。",
                    english: "Enable MP break. When disabled, the player's MP slot will not break."
                )
                );
            EnablePressDamage = Config.Bind(
                SectionPlayer,
                nameof(EnablePressDamage),
                true,
                new Translator(chinese: "启用挤压伤害", english: "Enable Press Damage"),
                new Translator(
                    chinese: "启用挤压伤害。关闭后，玩家将不会受到挤压伤害。",
                    english: "Enable falling to ground. When disabled, players will not fall to the ground."
                )
                );
            EnableFallingToGround = Config.Bind(
                SectionPlayer,
                nameof(EnableFallingToGround),
                true,
                new Translator(chinese: "启用摔倒", english: "Enable Falling To Ground"),
                new Translator(
                    chinese: "启用摔倒。关闭后，玩家将不会摔倒。",
                    english: "Enable falling to ground. When disabled, players will not fall to the ground."
                )
                );
            EnableAccessWarehouseAnywhere = Config.Bind(
                SectionPlayer,
                nameof(EnableAccessWarehouseAnywhere),
                false,
                new Translator(chinese: "启用随时访问仓库", english: "Enable Access Warehouse Anywhere"),
                new Translator(
                    chinese: "启用随时访问仓库。它将允许玩家在任何地方访问仓库库存。这将会取代原来的宝箱效果转轮。",
                    english: "Enable access warehouse anywhere. It will allow players to access warehouse inventory anywhere. This will replace the original Chest Reels."
                )
                );
            EnableImmuneAbnormalities = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalities),
                false,
                new Translator(chinese: "启用免疫异常状态", english: "Enable Immune Abnormalities"),
                new Translator(
                    chinese: "启用免疫异常状态。开启后，玩家将免疫所有异常状态。",
                    english: "Enable immune abnormalities. When enabled, players will be immune to all abnormalities."
                )
                );
            EnableImmuneAbnormalityMpReduce = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityMpReduce),
                false,
                new Translator(chinese: "免疫枯竭", english: "Immune MP Reduce"),
                new Translator(
                    chinese: "免疫异常状态：枯竭。",
                    english: "Immune abnormality: MP Reduce."
                )
                );
            EnableImmuneAbnormalityBurstTired = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityBurstTired),
                false,
                new Translator(chinese: "免疫晕厥", english: "Immune Burst Tired"),
                new Translator(
                    chinese: "免疫异常状态：晕厥。",
                    english: "Immune abnormality: Burst Tired."
                )
                );
            EnableImmuneAbnormalityClothesBroken = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityClothesBroken),
                false,
                new Translator(chinese: "免疫服装损坏", english: "Immune Clothes Broken"),
                new Translator(
                    chinese: "免疫异常状态：服装损坏。",
                    english: "Immune abnormality: Clothes Broken."
                )
                );
            EnableImmuneAbnormalityOverRunTired = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityOverRunTired),
                false,
                new Translator(chinese: "免疫疲惫", english: "Immune Tired"),
                new Translator(
                    chinese: "免疫异常状态：疲惫。",
                    english: "Immune abnormality: Tired."
                )
                );
            EnableImmuneAbnormalityShieldBreak = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityShieldBreak),
                false,
                new Translator(chinese: "免疫破盾", english: "Immune Shield Break"),
                new Translator(
                    chinese: "免疫异常状态：破盾。",
                    english: "Immune abnormality: Shield Break."
                )
                );
            EnableImmuneAbnormalitySleep = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalitySleep),
                false,
                new Translator(chinese: "免疫睡眠", english: "Immune Sleep"),
                new Translator(
                    chinese: "免疫异常状态：睡眠。",
                    english: "Immune abnormality: Sleep."
                )
                );
            EnableImmuneAbnormalityBurned = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityBurned),
                false,
                new Translator(chinese: "免疫燃烧", english: "Immune Burned"),
                new Translator(
                    chinese: "免疫异常状态：燃烧。",
                    english: "Immune abnormality: Burned."
                )
                );
            EnableImmuneAbnormalityFrozen = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityFrozen),
                false,
                new Translator(chinese: "免疫冻结", english: "Immune Frozen"),
                new Translator(
                    chinese: "免疫异常状态：冻结。",
                    english: "Immune abnormality: Frozen."
                )
                );
            EnableImmuneAbnormalityParalysis = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityParalysis),
                false,
                new Translator(chinese: "免疫麻痹", english: "Immune Paralysis"),
                new Translator(
                    chinese: "免疫异常状态：麻痹。",
                    english: "Immune abnormality: Paralysis."
                )
                );
            EnableImmuneAbnormalityConfuse = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityConfuse),
                false,
                new Translator(chinese: "免疫混乱", english: "Immune Confuse"),
                new Translator(
                    chinese: "免疫异常状态：混乱。",
                    english: "Immune abnormality: Confuse."
                )
                );
            EnableImmuneAbnormalityJamming = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityJamming),
                false,
                new Translator(chinese: "免疫杂念", english: "Immune Jamming"),
                new Translator(
                    chinese: "免疫异常状态：杂念。",
                    english: "Immune abnormality: Jamming."
                )
                );
            EnableImmuneAbnormalityParasitised = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityParasitised),
                false,
                new Translator(chinese: "免疫植物寄生", english: "Immune Parasitised"),
                new Translator(
                    chinese: "免疫异常状态：植物寄生。",
                    english: "Immune abnormality: Parasitised."
                )
                );
            EnableImmuneAbnormalityShamed = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityShamed),
                false,
                new Translator(chinese: "免疫羞耻", english: "Immune Shamed"),
                new Translator(
                    chinese: "免疫异常状态：羞耻。",
                    english: "Immune abnormality: Shamed."
                )
                );
            EnableImmuneAbnormalityShamedSplit = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityShamedSplit),
                false,
                new Translator(chinese: "免疫羞耻（魔力流失）", english: "Immune Shamed Split"),
                new Translator(
                    chinese: "免疫异常状态：羞耻（魔力流失）。",
                    english: "Immune abnormality: Shamed Split."
                )
                );
            EnableImmuneAbnormalityShamedWet = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityShamedWet),
                false,
                new Translator(chinese: "免疫羞耻（濡湿）", english: "Immune Shamed Wet"),
                new Translator(
                    chinese: "免疫异常状态：羞耻（濡湿）。",
                    english: "Immune abnormality: Shamed Wet."
                )
                );
            EnableImmuneAbnormalityShamedEp = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityShamedEp),
                false,
                new Translator(chinese: "免疫羞耻（兴奋）", english: "Immune Shamed EP"),
                new Translator(
                    chinese: "免疫异常状态：羞耻（兴奋）。",
                    english: "Immune abnormality: Shamed EP."
                )
                );
            EnableImmuneAbnormalitySexercise = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalitySexercise),
                false,
                new Translator(chinese: "免疫催淫", english: "Immune Sexercise"),
                new Translator(
                    chinese: "免疫异常状态：催淫。",
                    english: "Immune abnormality: Sexercise."
                )
                );
            EnableImmuneAbnormalityFrustrated = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityFrustrated),
                false,
                new Translator(chinese: "免疫欲火中烧", english: "Immune Frustrated"),
                new Translator(
                    chinese: "免疫异常状态：欲火中烧。",
                    english: "Immune abnormality: Frustrated."
                )
                );
            EnableImmuneAbnormalityOrgasmAfter = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityOrgasmAfter),
                false,
                new Translator(chinese: "免疫恍惚", english: "Immune Orgasm After"),
                new Translator(
                    chinese: "免疫异常状态：恍惚。",
                    english: "Immune abnormality: Orgasm After."
                )
                );
            EnableImmuneAbnormalityEgged = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityEgged),
                false,
                new Translator(chinese: "免疫怀卵", english: "Immune Egged"),
                new Translator(
                    chinese: "免疫异常状态：怀卵。",
                    english: "Immune abnormality: Egged."
                )
                );
            EnableImmuneAbnormalityLayingEgg = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityLayingEgg),
                false,
                new Translator(chinese: "免疫产卵", english: "Immune Laying Egg"),
                new Translator(
                    chinese: "免疫异常状态：产卵。",
                    english: "Immune abnormality: Laying Egg."
                )
                );
            EnableImmuneAbnormalityDoNotLayEgg = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityDoNotLayEgg),
                false,
                new Translator(chinese: "免疫无法产卵", english: "Immune Do Not Lay Egg"),
                new Translator(
                    chinese: "免疫异常状态：无法产卵。",
                    english: "Immune abnormality: Do Not Lay Egg."
                )
                );
            EnableImmuneAbnormalityNearPee = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityNearPee),
                false,
                new Translator(chinese: "免疫尿意", english: "Immune Near Pee"),
                new Translator(
                    chinese: "免疫异常状态：尿意。",
                    english: "Immune abnormality: Near Pee."
                )
                );
            EnableImmuneAbnormalityDrunk = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityDrunk),
                false,
                new Translator(chinese: "免疫晕乎乎", english: "Immune Drunk"),
                new Translator(
                    chinese: "免疫异常状态：晕乎乎。",
                    english: "Immune abnormality: Drunk."
                )
                );
            EnableImmuneAbnormalityWebTrapped = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityWebTrapped),
                false,
                new Translator(chinese: "免疫黏糊糊", english: "Immune Web Trapped"),
                new Translator(
                    chinese: "免疫异常状态：黏糊糊。",
                    english: "Immune abnormality: Web Trapped."
                )
                );
            EnableImmuneAbnormalityStone = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityStone),
                false,
                new Translator(chinese: "免疫石化", english: "Immune Stone"),
                new Translator(
                    chinese: "免疫异常状态：石化。",
                    english: "Immune abnormality: Stone."
                )
                );
            EnableImmuneAbnormalityAtkDown = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityAtkDown),
                false,
                new Translator(chinese: "免疫攻击力下降", english: "Immune Atk Down"),
                new Translator(
                    chinese: "免疫异常状态：攻击力下降。",
                    english: "Immune abnormality: Atk Down."
                )
                );
            EnablePreloadBackpackCapacity = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadBackpackCapacity),
                false,
                new Translator(chinese: "预加载背包容量", english: "Preload Backpack Capacity"),
                new Translator(
                    chinese: "启用预加载背包容量。开启后，背包容量将在存档读取后自动设置一次，使用设置值覆盖原始的背包容量。",
                    english: "Enable preload backpack capacity. When enabled, the backpack capacity will be automatically set once after loading a save, " +
                    "using the configured value to override the original backpack capacity."
                )
                );
            EnablePreloadBottleHolderCount = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadBottleHolderCount),
                false,
                new Translator(chinese: "预加载空瓶收纳数量", english: "Preload Bottle Holder Count"),
                new Translator(
                    chinese: "启用预加载空瓶收纳槽位数量。开启后，空瓶收纳槽位数量将在存档读取后自动设置一次，使用设置值覆盖原始的空瓶收纳槽位数量。",
                    english: "Enable preload bottle holder count. When enabled, the bottle holder count will be automatically set once after loading a save, " +
                    "using the configured value to override the original bottle holder count."
                )
                );
            EnablePreloadPlayerHp = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadPlayerHp),
                false,
                new Translator(chinese: "预加载玩家HP", english: "Preload Player HP"),
                new Translator(
                    chinese: "启用预加载玩家 HP。开启后，玩家 HP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家 HP。",
                    english: "Enable preload player HP. When enabled, the player HP will be automatically set once after loading a save, " +
                    "using the configured value to override the original player HP."
                )
                );
            EnablePreloadPlayerMp = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadPlayerMp),
                false,
                new Translator(chinese: "预加载玩家MP", english: "Preload Player MP"),
                new Translator(
                    chinese: "启用预加载玩家 MP。开启后，玩家 MP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家 MP。",
                    english: "Enable preload player MP. When enabled, the player MP will be automatically set once after loading a save, " +
                    "using the configured value to override the original player MP."
                )
                );
            EnablePreloadPlayerEp = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadPlayerEp),
                false,
                new Translator(chinese: "预加载玩家EP", english: "Preload Player EP"),
                new Translator(
                    chinese: "启用预加载玩家 EP。开启后，玩家 EP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家 EP。",
                    english: "Enable preload player EP. When enabled, the player EP will be automatically set once after loading a save, " +
                    "using the configured value to override the original player EP."
                )
                );
            EnablePreloadPlayerMaxHp = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadPlayerMaxHp),
                false,
                new Translator(chinese: "预加载玩家最大HP", english: "Preload Player Max HP"),
                new Translator(
                    chinese: "启用预加载玩家最大 HP。开启后，玩家最大 HP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家最大 HP。",
                    english: "Enable preload player max HP. When enabled, the player max HP will be automatically set once after loading a save, " +
                    "using the configured value to override the original player max HP."
                )
                );
            EnablePreloadPlayerMaxMp = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadPlayerMaxMp),
                false,
                new Translator(chinese: "预加载玩家最大MP", english: "Preload Player Max MP"),
                new Translator(
                    chinese: "启用预加载玩家最大 MP。开启后，玩家最大 MP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家最大 MP。",
                    english: "Enable preload player max MP. When enabled, the player max MP will be automatically set once after loading a save, " +
                    "using the configured value to override the original player max MP."
                )
                );
            EnablePreloadPlayerMaxSatiety = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadPlayerMaxSatiety),
                false,
                new Translator(chinese: "预加载玩家最大饱食度", english: "Preload Player Max Satiety"),
                new Translator(
                    chinese: "启用预加载玩家最大饱食度。开启后，玩家最大饱食度将在存档读取后自动设置一次，使用设置值覆盖原始的玩家最大饱食度。",
                    english: "Enable preload player max satiety. When enabled, the player max satiety will be automatically set once after loading a save, " +
                    "using the configured value to override the original player max satiety."
                )
                );
            EnablePreloadOverChargeSlotCount = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadOverChargeSlotCount),
                false,
                new Translator(chinese: "预加载过充插槽数量", english: "Preload Over Charge Slot Count"),
                new Translator(
                    chinese: "启用预加载过充插槽数量。开启后，过充插槽数量将在存档读取后自动设置一次，使用设置值覆盖原始的过充插槽数量。",
                    english: "Enable preload over charge slot count. When enabled, the over charge slot count will be automatically set once after loading a save, " +
                    "using the configured value to override the original over charge slot count."
                )
                );
            EnablePreloadEnhancerSlotCount = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadEnhancerSlotCount),
                false,
                new Translator(chinese: "预加载强化插槽数量", english: "Preload Enhancer Slot Count"),
                new Translator(
                    chinese: "启用预加载强化插槽数量。开启后，强化插槽数量将在存档读取后自动设置一次，使用设置值覆盖原始的强化插槽数量。",
                    english: "Enable preload enhancer slot count. When enabled, the enhancer slot count will be automatically set once after loading a save, " +
                    "using the configured value to override the original enhancer slot count."
                )
                );
            SetBackpackCapacity = Config.Bind(
                SectionPlayer,
                nameof(SetBackpackCapacity),
                -1,
                new Translator(chinese: "设置背包容量", english: "Set Backpack Capacity"),
                new Translator(
                    chinese: "设置背包容量。将覆盖原始的背包容量。设为 -1 可保持为当前值。",
                    english: "Set backpack capacity. It will override the original backpack capacity. Set to -1 to keep the capacity at its current value."
                )
                );
            SetBottleHolderCount = Config.Bind(
                SectionPlayer,
                nameof(SetBottleHolderCount),
                -1,
                new Translator(chinese: "设置空瓶收纳数量", english: "Set Bottle Holder Count"),
                new Translator(
                    chinese: "设置空瓶收纳槽位数量。将覆盖原始的空瓶收纳槽位数量。设为 -1 可保持为当前值。",
                    english: "Set bottle holder count. It will override the original bottle holder count. Set to -1 to keep the bottle holder count at its current value."
                )
                );
            SetPlayerHp = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerHp),
                -1,
                new Translator(chinese: "设置玩家HP", english: "Set Player HP"),
                new Translator(
                    chinese: "设置玩家 HP。将覆盖原始的玩家 HP。设为 -1 可保持为当前值。",
                    english: "Set player HP. It will override the original player HP. Set to -1 to keep the HP at its current value."
                )
                );
            SetPlayerMp = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerMp),
                -1,
                new Translator(chinese: "设置玩家MP", english: "Set Player MP"),
                new Translator(
                    chinese: "设置玩家 MP。将覆盖原始的玩家 MP。设为 -1 可保持为当前值。",
                    english: "Set player MP. It will override the original player MP. Set to -1 to keep the MP at its current value."
                )
                );
            SetPlayerEp = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerEp),
                -1,
                new Translator(chinese: "设置玩家EP", english: "Set Player EP"),
                new Translator(
                    chinese: "设置玩家 EP。将覆盖原始的玩家 EP。设为 -1 可保持为当前值。",
                    english: "Set player EP. It will override the original player EP. Set to -1 to keep the EP at its current value."
                )
                );
            SetPlayerMaxHp = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerMaxHp),
                -1,
                new Translator(chinese: "设置玩家最大HP", english: "Set Player Max HP"),
                new Translator(
                    chinese: "设置玩家最大 HP。将覆盖原始的玩家最大 HP。设为 -1 可保持为当前值。",
                    english: "Set player max HP. It will override the original player max HP. Set to -1 to keep the max HP at its current value."
                )
                );
            SetPlayerMaxMp = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerMaxMp),
                -1,
                new Translator(chinese: "设置玩家最大MP", english: "Set Player Max MP"),
                new Translator(
                    chinese: "设置玩家最大 MP。将覆盖原始的玩家最大 MP。设为 -1 可保持为当前值。",
                    english: "Set player max MP. It will override the original player max MP. Set to -1 to keep the max MP at its current value."
                )
                );
            SetPlayerMaxSatiety = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerMaxSatiety),
                -1,
                new Translator(chinese: "设置玩家最大饱食度", english: "Set Player Max Satiety"),
                new Translator(
                    chinese: "设置玩家最大饱食度。将覆盖原始的玩家最大饱食度。设为 -1 可保持为当前值。",
                    english: "Set player max satiety. It will override the original player max satiety. Set to -1 to keep the max satiety at its current value."
                )
                );
            SetOverChargeSlotCount = Config.Bind(
                SectionPlayer,
                nameof(SetOverChargeSlotCount),
                -1,
                new Translator(chinese: "设置过充插槽数量", english: "Set Over Charge Slot Count"),
                new Translator(
                    chinese: "设置过充插槽数量。将覆盖原始的过充插槽数量。设为 -1 可保持为当前值。",
                    english: "Set over charge slot count. It will override the original over charge slot count. Set to -1 to keep the over charge slot count at its current value."
                )
                );
            SetEnhancerSlotCount = Config.Bind(
                SectionPlayer,
                nameof(SetEnhancerSlotCount),
                -1,
                new Translator(chinese: "设置强化插槽数量", english: "Set Enhancer Slot Count"),
                new Translator(
                    chinese: "设置强化插槽数量。将覆盖原始的强化插槽数量。设为 -1 可保持为当前值。",
                    english: "Set enhancer slot count. It will override the original enhancer slot count. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetPlayerWalkSpeed = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerWalkSpeed),
                -1f,
                new Translator(chinese: "设置玩家行走速度", english: "Set Player Walk Speed"),
                new Translator(
                    chinese: "设置玩家行走速度。设置的值为倍率，即 0 - 1 内玩家速度减小，大于 1 速度增大。",
                    english: "Set player walk speed. The set value is a multiplier, where values between 0 and 1 decrease player speed, and values greater than 1 increase speed."
                )
                );
        }
    }
}
