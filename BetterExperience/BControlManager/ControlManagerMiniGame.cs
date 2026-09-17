using BetterExperience.BLogSpace;
using BetterExperience.Patches;
using System;
using UnityModBase.HControlSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BControlManager
{
    internal static partial class ControlManager
    {
        internal static ControlEntry<bool> SetAutoFishing { get; private set; }
        internal static ControlEntry<bool> SetAutoMilk { get; private set; }
        internal static ControlEntry<bool> SetAutoBunServe { get; private set; }

        private const string SectionMiniGame = "MiniGame";

        /// <summary>
        /// 创建小游戏控制表并绑定小游戏辅助开关条目。
        /// </summary>
        internal static void InitializeMiniGame()
        {
            try
            {
                BService.Control.CreateTable(
                    SectionMiniGame,
                    new Translator(chinese: "小游戏", english: "Mini Game"),
                    new Translator(
                        chinese: "查看和修改当前游戏中的小游戏状态。",
                        english: "View and modify mini game state in the current game."
                        )
                    );

                SetAutoFishing = Bind(
                    SectionMiniGame,
                    nameof(SetAutoFishing),
                    HPatches.AutoFishing.GetAutoFishing,
                    HPatches.AutoFishing.SetAutoFishing,
                    new Translator(chinese: "自动钓鱼", english: "Auto Fishing"),
                    new Translator(
                        chinese: "开启后，开始钓鱼会自动瞄准、抛竿、起竿、跟随鱼标并确认结算。",
                        english: "After you start fishing it will aim, cast, hook, follow the marker and confirm the result."
                        )
                    );
                SetAutoMilk = Bind(
                    SectionMiniGame,
                    nameof(SetAutoMilk),
                    HPatches.AutoMilk.GetAutoMilk,
                    HPatches.AutoMilk.SetAutoMilk,
                    new Translator(chinese: "自动挤奶", english: "Auto Milking"),
                    new Translator(
                        chinese: "开启后，挤奶小游戏会自动寻找奶量最多的奶牛、对话并完成满级挤奶，循环到计时结束。",
                        english: "During the milking minigame it will find the fullest cow, talk and milk at full charge repeatedly until time is up."
                        )
                    );
                SetAutoBunServe = Bind(
                    SectionMiniGame,
                    nameof(SetAutoBunServe),
                    HPatches.AutoBunServe.GetAutoBunServe,
                    HPatches.AutoBunServe.SetAutoBunServe,
                    new Translator(chinese: "自动配送酒水", english: "Auto Drink Serving"),
                    new Translator(
                        chinese: "开启后，酒吧小游戏会按原版走路去吧台取酒，再送到点了对应酒款的客人面前。",
                        english: "During the bar minigame it will walk to the counter, pick up drinks and deliver them to the matching customers."
                        )
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize control manager for mini game.", ex);
            }
        }
    }
}
