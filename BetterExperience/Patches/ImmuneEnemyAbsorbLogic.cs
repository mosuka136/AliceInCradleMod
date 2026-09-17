namespace BetterExperience.Patches
{
    /// <summary>
    /// 敌怪拘束免疫规则。不触碰 Unity 或 AbsorbManager 实例。
    /// 牧场动物拘束不拦截；剧情 QTE 和自慰小游戏不走玩家 initAbsorb。
    /// </summary>
    internal static class ImmuneEnemyAbsorbLogic
    {
        /// <summary>
        /// 仅拦截敌怪发布的拘束。牧场挤奶动物除外。
        /// </summary>
        internal static bool ShouldBlockEnemyAbsorb(bool immuneOn, bool isEnemy, bool isFarmAnimal)
        {
            return immuneOn && isEnemy && !isFarmAnimal;
        }

        /// <summary>
        /// 拆掉全部敌怪拘束且没有剩余活动项时，走原版 GACHA 释放让玩家立即脱困。
        /// </summary>
        internal static bool ShouldFinishContainerRelease(
            bool immuneOn,
            bool releasedAnyEnemy,
            bool anyActiveRemaining)
        {
            return immuneOn && releasedAnyEnemy && !anyActiveRemaining;
        }
    }
}
