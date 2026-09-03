using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    /// <summary>
    /// Harmony 补丁的分部容器：各功能补丁以嵌套类形式放在独立文件中，
    /// 启动时由插件入口扫描所有带 HarmonyPatch 特性的类型统一注册。
    /// 本文件同时提供跨补丁共享的游戏对象定位与反射访问方法，
    /// 供各补丁类和 ControlManager 的读写委托复用。
    /// 这些方法基于 UnityEngine 场景查找与 Harmony Traverse 反射，只应在主线程调用；
    /// 目标对象未加载（未进入游戏场景）时一律返回 null 或调用方指定的默认值。
    /// </summary>
    public partial class HPatches
    {
        /// <summary>
        /// 获取当前玩家实例；未进入游戏场景时返回 null。
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
        /// M2D 是 SceneGame 的私有字段且无公开访问器，只能经 Traverse 反射读取。
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
        /// 读取当前玩家实例上指定名称的字段；玩家不可用时返回 <paramref name="unavailableValue"/>，
        /// 字段不存在或类型不匹配时 Traverse 返回默认值。字段名以游戏内部命名为准，改名需同步修改调用方。
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
