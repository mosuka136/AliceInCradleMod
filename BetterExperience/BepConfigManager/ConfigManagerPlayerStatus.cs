using BetterExperience.ConfigFileSpace;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
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
        public static ConfigEntry<int> SetPlayerHp { get; private set; }
        public static ConfigEntry<int> SetPlayerMp { get; private set; }
        public static ConfigEntry<int> SetPlayerEp { get; private set; }
        public static ConfigEntry<int> SetPlayerMaxHp { get; private set; }
        public static ConfigEntry<int> SetPlayerMaxMp { get; private set; }
        public static ConfigEntry<int> SetPlayerMaxSatiety { get; private set; }
        public static ConfigEntry<int> SetOverChargeSlotCount { get; private set; }
        public static ConfigEntry<int> SetEnhancerSlotCount { get; private set; }
        public static ConfigEntry<float> SetPlayerWalkSpeed { get; private set; }

        private const string SectionPlayer = "Player";

        public static void InitializePlayerStatus()
        {
            Config.CreateTable(SectionPlayer);

            EnableBeingAttacked = Config.Bind(
                SectionPlayer,
                nameof(EnableBeingAttacked),
                true,
                "Enable being attacked. If disabled, players will not be attacked by enemies, but traps may still be triggered.\n" +
                "启用被攻击。若关闭，玩家将不会受到敌人的攻击，但仍可能触发陷阱。"
                );
            EnableNoHpDamage = Config.Bind(
                SectionPlayer,
                nameof(EnableNoHpDamage),
                false,
                "Enable no HP damage.\n启用无 HP 伤害。"
                );
            EnableNoMpDamage = Config.Bind(
                SectionPlayer,
                nameof(EnableNoMpDamage),
                false,
                "Enable no MP damage.\n启用无 MP 伤害。"
                );
            EnableNoEpDamage = Config.Bind(
                SectionPlayer,
                nameof(EnableNoEpDamage),
                false,
                "Enable no EP damage.\n启用无 EP 伤害。玩家“好感度”将不会增加。"
                );
            EnableInfiniteShield = Config.Bind(
                SectionPlayer,
                nameof(EnableInfiniteShield),
                false,
                "Enable infinite shield.\n启用无限护盾。"
                );
            EnableHolyBurstFaint = Config.Bind(
                SectionPlayer,
                nameof(EnableHolyBurstFaint),
                true,
                "Enable Holy Burst Faint. When disabled, players will not faint after using Holy Burst.\n" +
                "启用圣光爆发昏厥。关闭后，玩家将不会因为使用圣光爆发而晕厥。"
                );
            EnableMpBreak = Config.Bind(
                SectionPlayer,
                nameof(EnableMpBreak),
                true,
                "Enable MP break. When disabled, the player's MP slot will not break.\n" +
                "启用 MP 破裂。关闭后，玩家 MP 槽将不会破裂。"
                );
            EnablePressDamage = Config.Bind(
                SectionPlayer,
                nameof(EnablePressDamage),
                true,
                "Enable falling to ground. When disabled, players will not fall to the ground.\n" +
                "启用挤压伤害。关闭后，玩家将不会受到挤压伤害。"
                );
            EnableFallingToGround = Config.Bind(
                SectionPlayer,
                nameof(EnableFallingToGround),
                true,
                "Enable falling to ground. When disabled, players will not fall to the ground.\n" +
                "启用摔倒。关闭后，玩家将不会摔倒。"
                );
            EnableAccessWarehouseAnywhere = Config.Bind(
                SectionPlayer,
                nameof(EnableAccessWarehouseAnywhere),
                false,
                "Enable access warehouse anywhere. It will allow players to access warehouse inventory anywhere. This will replace the original Chest Reels.\n" +
                "启用随时访问仓库。它将允许玩家在任何地方访问仓库库存。这将会取代原来的宝箱效果转轮。"
                );
            EnableImmuneAbnormalities = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalities),
                false,
                "Enable immune abnormalities. When enabled, players will be immune to all abnormalities. " +
                "启用免疫异常状态。开启后，玩家将免疫所有异常状态。"
                );
            EnableImmuneAbnormalityMpReduce = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityMpReduce),
                false,
                "Immune abnormality: MP Reduce.\n免疫异常状态：枯竭。"
                );
            EnableImmuneAbnormalityBurstTired = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityBurstTired),
                false,
                "Immune abnormality: Burst Tired.\n免疫异常状态：晕厥。"
                );
            EnableImmuneAbnormalityClothesBroken = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityClothesBroken),
                false,
                "Immune abnormality: Clothes Broken.\n免疫异常状态：服装损坏。"
                );
            EnableImmuneAbnormalityOverRunTired = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityOverRunTired),
                false,
                "Immune abnormality: Tired.\n免疫异常状态：疲惫。"
                );
            EnableImmuneAbnormalityShieldBreak = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityShieldBreak),
                false,
                "Immune abnormality: Shield Break.\n免疫异常状态：破盾。"
                );
            EnableImmuneAbnormalitySleep = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalitySleep),
                false,
                "Immune abnormality: Sleep.\n免疫异常状态：睡眠。"
                );
            EnableImmuneAbnormalityBurned = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityBurned),
                false,
                "Immune abnormality: Burned.\n免疫异常状态：燃烧。"
                );
            EnableImmuneAbnormalityFrozen = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityFrozen),
                false,
                "Immune abnormality: Frozen.\n免疫异常状态：冻结。"
                );
            EnableImmuneAbnormalityParalysis = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityParalysis),
                false,
                "Immune abnormality: Paralysis.\n免疫异常状态：麻痹。"
                );
            EnableImmuneAbnormalityConfuse = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityConfuse),
                false,
                "Immune abnormality: Confuse.\n免疫异常状态：混乱。"
                );
            EnableImmuneAbnormalityJamming = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityJamming),
                false,
                "Immune abnormality: Jamming.\n免疫异常状态：杂念。"
                );
            EnableImmuneAbnormalityParasitised = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityParasitised),
                false,
                "Immune abnormality: Parasitised.\n免疫异常状态：植物寄生。"
                );
            EnableImmuneAbnormalityShamed = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityShamed),
                false,
                "Immune abnormality: Shamed.\n免疫异常状态：羞耻。"
                );
            EnableImmuneAbnormalityShamedSplit = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityShamedSplit),
                false,
                "Immune abnormality: Shamed Split.\n免疫异常状态：羞耻（魔力流失）。"
                );
            EnableImmuneAbnormalityShamedWet = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityShamedWet),
                false,
                "Immune abnormality: Shamed Wet.\n免疫异常状态：羞耻（濡湿）。"
                );
            EnableImmuneAbnormalityShamedEp = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityShamedEp),
                false,
                "Immune abnormality: Shamed EP.\n免疫异常状态：羞耻（兴奋）。"
                );
            EnableImmuneAbnormalitySexercise = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalitySexercise),
                false,
                "Immune abnormality: Sexercise.\n免疫异常状态：催淫。"
                );
            EnableImmuneAbnormalityFrustrated = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityFrustrated),
                false,
                "Immune abnormality: Frustrated.\n免疫异常状态：欲火中烧。"
                );
            EnableImmuneAbnormalityOrgasmAfter = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityOrgasmAfter),
                false,
                "Immune abnormality: Orgasm After.\n免疫异常状态：恍惚。"
                );
            EnableImmuneAbnormalityEgged = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityEgged),
                false,
                "Immune abnormality: Egged.\n免疫异常状态：怀卵。"
                );
            EnableImmuneAbnormalityLayingEgg = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityLayingEgg),
                false,
                "Immune abnormality: Laying Egg.\n免疫异常状态：产卵。"
                );
            EnableImmuneAbnormalityDoNotLayEgg = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityDoNotLayEgg),
                false,
                "Immune abnormality: Do Not Lay Egg.\n免疫异常状态：无法产卵。"
                );
            EnableImmuneAbnormalityNearPee = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityNearPee),
                false,
                "Immune abnormality: Near Pee.\n免疫异常状态：尿意。"
                );
            EnableImmuneAbnormalityDrunk = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityDrunk),
                false,
                "Immune abnormality: Drunk.\n免疫异常状态：晕乎乎。"
                );
            EnableImmuneAbnormalityWebTrapped = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityWebTrapped),
                false,
                "Immune abnormality: Web Trapped.\n免疫异常状态：黏糊糊。"
                );
            EnableImmuneAbnormalityStone = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityStone),
                false,
                "Immune abnormality: Stone.\n免疫异常状态：石化。"
                );
            EnableImmuneAbnormalityAtkDown = Config.Bind(
                SectionPlayer,
                nameof(EnableImmuneAbnormalityAtkDown),
                false,
                "Immune abnormality: Atk Down.\n免疫异常状态：攻击力下降。"
                );
            EnablePreloadBackpackCapacity = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadBackpackCapacity),
                false,
                "Enable preload backpack capacity. When enabled, the backpack capacity will be automatically set once after loading a save, " +
                "using the configured value to override the original backpack capacity.\n" +
                "启用预加载背包容量。开启后，背包容量将在存档读取后自动设置一次，使用设置值覆盖原始的背包容量。"
                );
            EnablePreloadBottleHolderCount = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadBottleHolderCount),
                false,
                "Enable preload bottle holder count. When enabled, the bottle holder count will be automatically set once after loading a save, " +
                "using the configured value to override the original bottle holder count.\n" +
                "启用预加载空瓶收纳槽位数量。开启后，空瓶收纳槽位数量将在存档读取后自动设置一次，使用设置值覆盖原始的空瓶收纳槽位数量。"
                );
            EnablePreloadPlayerHp = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadPlayerHp),
                false,
                "Enable preload player HP. When enabled, the player HP will be automatically set once after loading a save, " +
                "using the configured value to override the original player HP.\n" +
                "启用预加载玩家 HP。开启后，玩家 HP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家 HP。"
                );
            EnablePreloadPlayerMp = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadPlayerMp),
                false,
                "Enable preload player MP. When enabled, the player MP will be automatically set once after loading a save, " +
                "using the configured value to override the original player MP.\n" +
                "启用预加载玩家 MP。开启后，玩家 MP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家 MP。"
                );
            EnablePreloadPlayerEp = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadPlayerEp),
                false,
                "Enable preload player EP. When enabled, the player EP will be automatically set once after loading a save, " +
                "using the configured value to override the original player EP.\n" +
                "启用预加载玩家 EP。开启后，玩家 EP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家 EP。"
                );
            EnablePreloadPlayerMaxHp = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadPlayerMaxHp),
                false,
                "Enable preload player max HP. When enabled, the player max HP will be automatically set once after loading a save, " +
                "using the configured value to override the original player max HP.\n" +
                "启用预加载玩家最大 HP。开启后，玩家最大 HP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家最大 HP。"
                );
            EnablePreloadPlayerMaxMp = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadPlayerMaxMp),
                false,
                "Enable preload player max MP. When enabled, the player max MP will be automatically set once after loading a save, " +
                "using the configured value to override the original player max MP.\n" +
                "启用预加载玩家最大 MP。开启后，玩家最大 MP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家最大 MP。"
                );
            EnablePreloadPlayerMaxSatiety = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadPlayerMaxSatiety),
                false,
                "Enable preload player max satiety. When enabled, the player max satiety will be automatically set once after loading a save, " +
                "using the configured value to override the original player max satiety.\n" +
                "启用预加载玩家最大饱食度。开启后，玩家最大饱食度将在存档读取后自动设置一次，使用设置值覆盖原始的玩家最大饱食度。"
                );
            EnablePreloadOverChargeSlotCount = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadOverChargeSlotCount),
                false,
                "Enable preload over charge slot count. When enabled, the over charge slot count will be automatically set once after loading a save, " +
                "using the configured value to override the original over charge slot count.\n" +
                "启用预加载过充插槽数量。开启后，过充插槽数量将在存档读取后自动设置一次，使用设置值覆盖原始的过充插槽数量。"
                );
            EnablePreloadEnhancerSlotCount = Config.Bind(
                SectionPlayer,
                nameof(EnablePreloadEnhancerSlotCount),
                false,
                "Enable preload enhancer slot count. When enabled, the enhancer slot count will be automatically set once after loading a save, " +
                "using the configured value to override the original enhancer slot count.\n" +
                "启用预加载强化插槽数量。开启后，强化插槽数量将在存档读取后自动设置一次，使用设置值覆盖原始的强化插槽数量。"
                );
            SetBackpackCapacity = Config.Bind(
                SectionPlayer,
                nameof(SetBackpackCapacity),
                -1,
                "Set backpack capacity. It will override the original backpack capacity. Set to -1 to keep the capacity at its current value.\n" +
                "设置背包容量。将覆盖原始的背包容量。设为 -1 可保持为当前值。"
                );
            SetBottleHolderCount = Config.Bind(
                SectionPlayer,
                nameof(SetBottleHolderCount),
                -1,
                "Set bottle holder count. It will override the original bottle holder count. Set to -1 to keep the bottle holder count at its current value.\n" +
                "设置空瓶收纳槽位数量。将覆盖原始的空瓶收纳槽位数量。设为 -1 可保持为当前值。"
                );
            SetPlayerHp = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerHp),
                -1,
                "Set player HP. It will override the original player HP. Set to -1 to keep the HP at its current value.\n" +
                "设置玩家 HP。将覆盖原始的玩家 HP。设为 -1 可保持为当前值。"
                );
            SetPlayerMp = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerMp),
                -1,
                "Set player MP. It will override the original player MP. Set to -1 to keep the MP at its current value.\n" +
                "设置玩家 MP。将覆盖原始的玩家 MP。设为 -1 可保持为当前值。"
                );
            SetPlayerEp = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerEp),
                -1,
                "Set player EP. It will override the original player EP. Set to -1 to keep the EP at its current value.\n" +
                "设置玩家 EP。将覆盖原始的玩家 EP。设为 -1 可保持为当前值。"
                );
            SetPlayerMaxHp = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerMaxHp),
                -1,
                "Set player max HP. It will override the original player max HP. Set to -1 to keep the max HP at its current value.\n" +
                "设置玩家最大 HP。将覆盖原始的玩家最大 HP。设为 -1 可保持为当前值。"
                );
            SetPlayerMaxMp = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerMaxMp),
                -1,
                "Set player max MP. It will override the original player max MP. Set to -1 to keep the max MP at its current value.\n" +
                "设置玩家最大 MP。将覆盖原始的玩家最大 MP。设为 -1 可保持为当前值。"
                );
            SetPlayerMaxSatiety = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerMaxSatiety),
                -1,
                "Set player max satiety. It will override the original player max satiety. Set to -1 to keep the max satiety at its current value.\n" +
                "设置玩家最大饱食度。将覆盖原始的玩家最大饱食度。设为 -1 可保持为当前值。"
                );
            SetOverChargeSlotCount = Config.Bind(
                SectionPlayer,
                nameof(SetOverChargeSlotCount),
                -1,
                "Set over charge slot count. It will override the original over charge slot count. Set to -1 to keep the over charge slot count at its current value.\n" +
                "设置过充插槽数量。将覆盖原始的过充插槽数量。设为 -1 可保持为当前值。"
                );
            SetEnhancerSlotCount = Config.Bind(
                SectionPlayer,
                nameof(SetEnhancerSlotCount),
                -1,
                "Set enhancer slot count. It will override the original enhancer slot count. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置强化插槽数量。将覆盖原始的强化插槽数量。设为 -1 可保持为当前值。"
                );
            SetPlayerWalkSpeed = Config.Bind(
                SectionPlayer,
                nameof(SetPlayerWalkSpeed),
                -1f,
                "Set player walk speed. The set value is a multiplier, where values between 0 and 1 decrease player speed, and values greater than 1 increase speed.\n" +
                "设置玩家行走速度。设置的值为倍率，即 0 - 1 内玩家速度减小，大于 1 速度增大。"
                );
        }
    }
}
