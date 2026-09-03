using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    /// <summary>
    /// Harmony 补丁的分部容器。
    /// </summary>
    public partial class HPatches
    {
        /// <summary>
        /// 获取当前玩家实例。
        /// </summary>
        public static PR GetPR()
        {
            return UnityEngine.Object.FindAnyObjectByType<PR>();
        }

        /// <summary>
        /// 获取当前游戏场景实例。
        /// </summary>
        public static SceneGame GetSceneGame()
        {
            return UnityEngine.Object.FindAnyObjectByType<SceneGame>();
        }

        /// <summary>
        /// 获取当前游戏的二维地图管理器。
        /// </summary>
        public static NelM2DBase GetM2D()
        {
            var sceneGame = GetSceneGame();
            return sceneGame == null
                ? null
                : Traverse.Create(sceneGame).Field("M2D").GetValue<NelM2DBase>();
        }

        /// <summary>
        /// 获取当前游戏的物品管理器。
        /// </summary>
        public static NelItemManager GetIMNG()
        {
            return GetM2D()?.IMNG;
        }

        /// <summary>
        /// 获取当前玩家的反射访问器。
        /// </summary>
        public static Traverse GetPRTraverse()
        {
            return TryGetPRTraverse(out _, out var playerTraverse) ? playerTraverse : null;
        }

        /// <summary>
        /// 获取当前玩家及其反射访问器。
        /// </summary>
        public static bool TryGetPRTraverse(out PR player, out Traverse playerTraverse)
        {
            player = GetPR();
            playerTraverse = player == null ? null : Traverse.Create(player);
            return playerTraverse != null;
        }

        /// <summary>
        /// 获取当前玩家技能数据的反射访问器。
        /// </summary>
        public static Traverse GetPRSkillTraverse()
        {
            var player = GetPR();
            return player?.Skill == null ? null : Traverse.Create(player.Skill);
        }

        /// <summary>
        /// 读取当前玩家字段；玩家不可用时返回调用方指定的默认值。
        /// </summary>
        public static T GetPRFieldValue<T>(string fieldName, T unavailableValue = default(T))
        {
            var playerTraverse = GetPRTraverse();
            return playerTraverse == null
                ? unavailableValue
                : playerTraverse.Field(fieldName).GetValue<T>();
        }

        /// <summary>
        /// 获取当前存档的主背包。
        /// </summary>
        public static ItemStorage GetInventory()
        {
            return GetIMNG()?.getInventory();
        }

        /// <summary>
        /// 获取当前存档的贵重品背包。
        /// </summary>
        public static ItemStorage GetPreciousInventory()
        {
            return GetIMNG()?.getInventoryPrecious();
        }

        /// <summary>
        /// 获取当前存档的夜晚控制器。
        /// </summary>
        public static NightController GetNightController()
        {
            return GetM2D()?.NightCon;
        }
    }
}
