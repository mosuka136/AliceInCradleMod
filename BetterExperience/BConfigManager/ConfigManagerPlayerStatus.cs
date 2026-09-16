using BetterExperience.BLogSpace;
using System;
using UnityModBase.HClassAttribute;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public static partial class ConfigManager
    {
        // 玩家配置同时包含即时开关和双值“读档后预加载”项：Value1 为是否读档后自动应用，Value2 为设置值。
        // Set* 双值项的 Value2 以 -1 约定为保持游戏当前值。
        public static ConfigEntry<bool> EnableBeingAttacked { get; private set; }
        public static ConfigEntry<bool> EnableNoHpDamage { get; private set; }
        public static ConfigEntry<bool> EnableNoMpDamage { get; private set; }
        public static ConfigEntry<bool> EnableNoEpDamage { get; private set; }
        public static ConfigEntry<bool> EnableInfiniteShield { get; private set; }
        public static ConfigEntry<bool> EnableHolyBurstFaint { get; private set; }
        public static ConfigEntry<bool> EnableMpBreak { get; private set; }
        public static ConfigEntry<bool> EnablePressDamage { get; private set; }
        public static ConfigEntry<bool> EnableFallingToGround { get; private set; }
        public static ConfigEntry<bool> EnablePreventGameOver { get; private set; }
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
        public static ConfigEntry<bool> EnableImmuneAbnormalityHpReduce { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityPoison { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityMilky { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityStrongHold { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityEaten { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityFainted { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityWormTrapped { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityDefDown { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityForbiddenOrgasm { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityOrgasmInitialize { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityOrgasmStack { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityCocoon { get; private set; }
        public static ConfigEntry<bool> EnableImmuneAbnormalityDeath { get; private set; }
        public static ConfigEntry<bool> EnableMouseTeleport { get; private set; }
        public static ConfigEntry<bool> EnableNoclip { get; private set; }
        public static ConfigEntry<bool> EnableNoSatietyDrain { get; private set; }
        public static ConfigEntry<bool> EnableNoItemConsume { get; private set; }
        public static ConfigEntry<bool> EnableNoDirtStains { get; private set; }
        public static ConfigEntry<bool> EnableNoWetten { get; private set; }
        public static ConfigEntry<bool> EnableAbsorbGachaSkip { get; private set; }
        [EntrySlider(-1f, 10f, 0.1f)]
        public static ConfigEntry<float> SetPlayerWalkSpeed { get; private set; }
        [EntrySlider(0.1f, 5f, 0.1f)]
        public static ConfigEntry<float> PlayerJumpMultiplier { get; private set; }
        [EntrySlider(1f, 30f, 1f)]
        public static ConfigEntry<float> NoclipSpeed { get; private set; }
        [EntryGui(2)]
        public static ConfigEntry<bool, int> SetBackpackCapacity { get; private set; }
        [EntryGui(2)]
        public static ConfigEntry<bool, int> SetBottleHolderCount { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 1000f, 1f)]
        public static ConfigEntry<bool, int> SetPlayerHp { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 1000f, 1f)]
        public static ConfigEntry<bool, int> SetPlayerMp { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 1000f, 1f)]
        public static ConfigEntry<bool, int> SetPlayerEp { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 1000f, 1f)]
        public static ConfigEntry<bool, int> SetPlayerMaxHp { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 1000f, 1f)]
        public static ConfigEntry<bool, int> SetPlayerMaxMp { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 100f, 1f)]
        public static ConfigEntry<bool, int> SetPlayerMaxSatiety { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 10f, 1f)]
        public static ConfigEntry<bool, int> SetOverChargeSlotCount { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 20f, 1f)]
        public static ConfigEntry<bool, int> SetEnhancerSlotCount { get; private set; }
        [EntrySlider(0f, 20f, 1f)]
        public static ConfigEntry<int> SetNoItemConsumeCount { get; private set; }

        private const string SectionPlayer = "Player";

        /// <summary>
        /// 初始化玩家相关配置，包括伤害开关、异常状态免疫、背包/槽位和基础属性预加载设置。
        /// </summary>
        public static void InitializePlayerStatus()
        {
            try
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
                        english: "Enable press damage. When disabled, players will not take press damage."
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
                EnablePreventGameOver = Config.Bind(
                    SectionPlayer,
                    nameof(EnablePreventGameOver),
                    false,
                    new Translator(chinese: "启用防止败北", english: "Enable Prevent Game Over"),
                    new Translator(
                        chinese: "触发败北（含 HP 归零与事件强制败北）时不进入败北画面，玩家以 1 HP 原地恢复继续游戏。战败导致的公会任务失败不再发生；部分剧情强制演出可能受影响。",
                        english: "When a game over triggers (HP reaching zero or forced event defeats), no defeat screen appears; the player recovers in place with 1 HP and keeps playing. Guild quests no longer fail on defeat; some forced story scenes may be affected."
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
                EnableImmuneAbnormalityHpReduce = Config.Bind(
                    SectionPlayer,
                    nameof(EnableImmuneAbnormalityHpReduce),
                    false,
                    new Translator(chinese: "免疫HP枯竭", english: "Immune HP Reduce"),
                    new Translator(
                        chinese: "免疫异常状态：HP 枯竭。",
                        english: "Immune abnormality: HP Reduce."
                        )
                    );
                EnableImmuneAbnormalityPoison = Config.Bind(
                    SectionPlayer,
                    nameof(EnableImmuneAbnormalityPoison),
                    false,
                    new Translator(chinese: "免疫中毒", english: "Immune Poison"),
                    new Translator(
                        chinese: "免疫异常状态：中毒。",
                        english: "Immune abnormality: Poison."
                        )
                    );
                EnableImmuneAbnormalityMilky = Config.Bind(
                    SectionPlayer,
                    nameof(EnableImmuneAbnormalityMilky),
                    false,
                    new Translator(chinese: "免疫泌乳", english: "Immune Milky"),
                    new Translator(
                        chinese: "免疫异常状态：泌乳。",
                        english: "Immune abnormality: Milky."
                        )
                    );
                EnableImmuneAbnormalityStrongHold = Config.Bind(
                    SectionPlayer,
                    nameof(EnableImmuneAbnormalityStrongHold),
                    false,
                    new Translator(chinese: "免疫拘束", english: "Immune Strong Hold"),
                    new Translator(
                        chinese: "免疫异常状态：拘束。",
                        english: "Immune abnormality: Strong Hold."
                        )
                    );
                EnableImmuneAbnormalityEaten = Config.Bind(
                    SectionPlayer,
                    nameof(EnableImmuneAbnormalityEaten),
                    false,
                    new Translator(chinese: "免疫被吞", english: "Immune Eaten"),
                    new Translator(
                        chinese: "免疫异常状态：被吞。",
                        english: "Immune abnormality: Eaten."
                        )
                    );
                EnableImmuneAbnormalityFainted = Config.Bind(
                    SectionPlayer,
                    nameof(EnableImmuneAbnormalityFainted),
                    false,
                    new Translator(chinese: "免疫昏倒", english: "Immune Fainted"),
                    new Translator(
                        chinese: "免疫异常状态：昏倒。与晕厥不是同一状态。",
                        english: "Immune abnormality: Fainted. This is not Burst Tired."
                        )
                    );
                EnableImmuneAbnormalityWormTrapped = Config.Bind(
                    SectionPlayer,
                    nameof(EnableImmuneAbnormalityWormTrapped),
                    false,
                    new Translator(chinese: "免疫虫困", english: "Immune Worm Trapped"),
                    new Translator(
                        chinese: "免疫异常状态：虫困。与地图「启用虫墙」开关相互独立。",
                        english: "Immune abnormality: Worm Trapped. Independent of the map worm-trap switch."
                        )
                    );
                EnableImmuneAbnormalityDefDown = Config.Bind(
                    SectionPlayer,
                    nameof(EnableImmuneAbnormalityDefDown),
                    false,
                    new Translator(chinese: "免疫防御力下降", english: "Immune Def Down"),
                    new Translator(
                        chinese: "免疫异常状态：防御力下降。",
                        english: "Immune abnormality: Def Down."
                        )
                    );
                EnableImmuneAbnormalityForbiddenOrgasm = Config.Bind(
                    SectionPlayer,
                    nameof(EnableImmuneAbnormalityForbiddenOrgasm),
                    false,
                    new Translator(chinese: "免疫禁止高潮", english: "Immune Forbidden Orgasm"),
                    new Translator(
                        chinese: "免疫异常状态：禁止高潮。",
                        english: "Immune abnormality: Forbidden Orgasm."
                        )
                    );
                EnableImmuneAbnormalityOrgasmInitialize = Config.Bind(
                    SectionPlayer,
                    nameof(EnableImmuneAbnormalityOrgasmInitialize),
                    false,
                    new Translator(chinese: "免疫高潮发动", english: "Immune Orgasm Initialize"),
                    new Translator(
                        chinese: "免疫异常状态：高潮发动。",
                        english: "Immune abnormality: Orgasm Initialize."
                        )
                    );
                EnableImmuneAbnormalityOrgasmStack = Config.Bind(
                    SectionPlayer,
                    nameof(EnableImmuneAbnormalityOrgasmStack),
                    false,
                    new Translator(chinese: "免疫高潮蓄积", english: "Immune Orgasm Stack"),
                    new Translator(
                        chinese: "免疫异常状态：高潮蓄积。",
                        english: "Immune abnormality: Orgasm Stack."
                        )
                    );
                EnableImmuneAbnormalityCocoon = Config.Bind(
                    SectionPlayer,
                    nameof(EnableImmuneAbnormalityCocoon),
                    false,
                    new Translator(chinese: "免疫茧", english: "Immune Cocoon"),
                    new Translator(
                        chinese: "免疫异常状态：茧。",
                        english: "Immune abnormality: Cocoon."
                        )
                    );
                EnableImmuneAbnormalityDeath = Config.Bind(
                    SectionPlayer,
                    nameof(EnableImmuneAbnormalityDeath),
                    false,
                    new Translator(chinese: "免疫死亡状态", english: "Immune Death"),
                    new Translator(
                        chinese: "免疫异常状态：死亡状态。",
                        english: "Immune abnormality: Death."
                        )
                    );
                EnableMouseTeleport = Config.Bind(
                    SectionPlayer,
                    nameof(EnableMouseTeleport),
                    false,
                    new Translator(chinese: "启用鼠标传送", english: "Enable Mouse Teleport"),
                    new Translator(
                        chinese: "鼠标指向游戏画面内的空地，按传送热键跨墙传送。默认 Ctrl+G；菜单、剧情或不能操作玩家时不可用。",
                        english: "Point at clear space in the game view and press the teleport hotkey (Ctrl+G by default). Unavailable in menus, events or while player control is blocked."
                        )
                    );
                EnableNoclip = Config.Bind(
                    SectionPlayer,
                    nameof(EnableNoclip),
                    false,
                    new Translator(chinese: "启用穿墙热键", english: "Enable Noclip Hotkey"),
                    new Translator(
                        chinese: "允许用热键切换穿墙飞行，默认 Ctrl+N。方向键/WASD 或手柄左摇杆/方向键移动；每次读档、切图后需重新开启。",
                        english: "Allow toggling noclip flight (Ctrl+N by default). Move with arrows/WASD or gamepad left stick/D-pad. Toggle again after loading or changing maps."
                        )
                    );
                EnableNoSatietyDrain = Config.Bind(
                    SectionPlayer,
                    nameof(EnableNoSatietyDrain),
                    false,
                    new Translator(chinese: "启用饱食不消耗", english: "Enable No Satiety Drain"),
                    new Translator(
                        chinese: "开启后不再因战斗结算、事件等消化胃里的食物。",
                        english: "Food in the stomach is no longer digested by battles or events."
                        )
                    );
                EnableNoItemConsume = Config.Bind(
                    SectionPlayer,
                    nameof(EnableNoItemConsume),
                    false,
                    new Translator(chinese: "启用物品成组不消耗", english: "Enable No Item Consume Above Rows"),
                    new Translator(
                        chinese: "以背包格（组）为单位：物品装满的格子数（各品级合计）达到阈值时，快捷使用与背包菜单使用不再消耗数量。",
                        english: "Counted in inventory rows (stacks): while an item's full rows across grades reach the threshold, quick use and menu use no longer consume it."
                        )
                    );
                EnableNoDirtStains = Config.Bind(
                    SectionPlayer,
                    nameof(EnableNoDirtStains),
                    false,
                    new Translator(chinese: "启用不沾污渍", english: "No dirt stains"),
                    new Translator(
                        chinese: "不再叠加上去新的泥土、液体、精液等污渍。已有污渍需椅子清洗。",
                        english: "Blocks new mud, liquid and similar stains. Existing stains need a bench wash."
                        )
                    );
                EnableNoWetten = Config.Bind(
                    SectionPlayer,
                    nameof(EnableNoWetten),
                    false,
                    new Translator(chinese: "启用不浸湿", english: "No wettening"),
                    new Translator(
                        chinese: "不再进入浸湿外观。已湿需椅子清理。",
                        english: "Blocks the wet appearance. Already wet cloth still needs a bench."
                        )
                    );
                EnableAbsorbGachaSkip = Config.Bind(
                    SectionPlayer,
                    nameof(EnableAbsorbGachaSkip),
                    false,
                    new Translator(chinese: "拘束挣脱：自动完成", english: "Absorb gacha: auto complete"),
                    new Translator(
                        chinese: "被拘束时自动完成挣脱按键/钟摆。剧情事件 QTE 和自慰小游戏不跳过。",
                        english: "Automatically finishes the escape mash/pendulum while absorbed. Story event QTEs and the masturbation minigame are not skipped."
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
                PlayerJumpMultiplier = Config.Bind(
                    SectionPlayer,
                    nameof(PlayerJumpMultiplier),
                    1f,
                    new Translator(chinese: "玩家跳跃力度倍率", english: "Player Jump Strength Multiplier"),
                    new Translator(
                        chinese: "通过起跳速度调整跳跃高度，范围 0.1–5，1 为原值。影响普通和水中跳跃，保留异常状态修正；力度倍率不等于高度倍率。",
                        english: "Adjust jump height through launch speed (0.1–5; 1 is unchanged). Applies on land and in water, preserving status modifiers. Strength and height multipliers differ."
                        )
                    );
                NoclipSpeed = Config.Bind(
                    SectionPlayer,
                    nameof(NoclipSpeed),
                    8f,
                    new Translator(chinese: "穿墙飞行速度", english: "Noclip Flight Speed"),
                    new Translator(
                        chinese: "每秒移动的地图格数，范围 1–30。",
                        english: "Map units moved per second, from 1 to 30."
                        )
                    );
                SetBackpackCapacity = BindPreloadValue(
                    SectionPlayer,
                    nameof(SetBackpackCapacity),
                    -1,
                    new Translator(chinese: "设置背包容量", english: "Set Backpack Capacity"),
                    new Translator(
                        chinese: "设置背包容量。将覆盖原始的背包容量。",
                        english: "Set backpack capacity. It will override the original backpack capacity."
                        )
                    );
                SetBottleHolderCount = BindPreloadValue(
                    SectionPlayer,
                    nameof(SetBottleHolderCount),
                    -1,
                    new Translator(chinese: "设置空瓶收纳数量", english: "Set Bottle Holder Count"),
                    new Translator(
                        chinese: "设置空瓶收纳槽位数量。将覆盖原始的空瓶收纳槽位数量。",
                        english: "Set bottle holder count."
                        )
                    );
                SetPlayerHp = BindPreloadValue(
                    SectionPlayer,
                    nameof(SetPlayerHp),
                    -1,
                    new Translator(chinese: "设置玩家HP", english: "Set Player HP"),
                    new Translator(
                        chinese: "设置玩家 HP。将覆盖原始的玩家 HP。",
                        english: "Set player HP. It will override the original player HP."
                        )
                    );
                SetPlayerMp = BindPreloadValue(
                    SectionPlayer,
                    nameof(SetPlayerMp),
                    -1,
                    new Translator(chinese: "设置玩家MP", english: "Set Player MP"),
                    new Translator(
                        chinese: "设置玩家 MP。将覆盖原始的玩家 MP。",
                        english: "Set player MP. It will override the original player MP."
                        )
                    );
                SetPlayerEp = BindPreloadValue(
                    SectionPlayer,
                    nameof(SetPlayerEp),
                    -1,
                    new Translator(chinese: "设置玩家EP", english: "Set Player EP"),
                    new Translator(
                        chinese: "设置玩家 EP。将覆盖原始的玩家 EP。",
                        english: "Set player EP. It will override the original player EP."
                        )
                    );
                SetPlayerMaxHp = BindPreloadValue(
                    SectionPlayer,
                    nameof(SetPlayerMaxHp),
                    -1,
                    new Translator(chinese: "设置玩家最大HP", english: "Set Player Max HP"),
                    new Translator(
                        chinese: "设置玩家最大 HP。将覆盖原始的玩家最大 HP。",
                        english: "Set player max HP. It will override the original player max HP."
                        )
                    );
                SetPlayerMaxMp = BindPreloadValue(
                    SectionPlayer,
                    nameof(SetPlayerMaxMp),
                    -1,
                    new Translator(chinese: "设置玩家最大MP", english: "Set Player Max MP"),
                    new Translator(
                        chinese: "设置玩家最大 MP。将覆盖原始的玩家最大 MP。",
                        english: "Set player max MP. It will override the original player max MP."
                        )
                    );
                SetPlayerMaxSatiety = BindPreloadValue(
                    SectionPlayer,
                    nameof(SetPlayerMaxSatiety),
                    -1,
                    new Translator(chinese: "设置玩家最大饱食度", english: "Set Player Max Satiety"),
                    new Translator(
                        chinese: "设置玩家最大饱食度。将覆盖原始的玩家最大饱食度。",
                        english: "Set player max satiety. It will override the original player max satiety."
                        )
                    );
                SetOverChargeSlotCount = BindPreloadValue(
                    SectionPlayer,
                    nameof(SetOverChargeSlotCount),
                    -1,
                    new Translator(chinese: "设置过充插槽数量", english: "Set Over Charge Slot Count"),
                    new Translator(
                        chinese: "设置过充插槽数量。将覆盖原始的过充插槽数量。",
                        english: "Set over charge slot count. It will override the original over charge slot count."
                        )
                    );
                SetEnhancerSlotCount = BindPreloadValue(
                    SectionPlayer,
                    nameof(SetEnhancerSlotCount),
                    -1,
                    new Translator(chinese: "设置强化插槽数量", english: "Set Enhancer Slot Count"),
                    new Translator(
                        chinese: "设置强化插槽数量。将覆盖原始的强化插槽数量。",
                        english: "Set enhancer slot count. It will override the original enhancer slot count."
                        )
                    );
                SetNoItemConsumeCount = Config.Bind(
                    SectionPlayer,
                    nameof(SetNoItemConsumeCount),
                    1,
                    new Translator(chinese: "设置不消耗的组数阈值", english: "Set No Item Consume Rows"),
                    new Translator(
                        chinese: "物品装满的背包格数达到该值时使用不消耗。默认 1 组，设为 0 表示只要有库存就不消耗。",
                        english: "Using an item is free once its full inventory rows reach this value. Defaults to 1; 0 means always free while in stock."
                        )
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize config manager for player status.", ex);
            }
        }
    }
}
