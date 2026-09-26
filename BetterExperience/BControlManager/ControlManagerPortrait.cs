using BetterExperience.BLogSpace;
using BetterExperience.Patches;
using System;
using System.Collections.Generic;
using UnityModBase.HControlSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BControlManager
{
    internal static partial class ControlManager
    {
        internal static ControlEntry<string> PortraitFilter { get; private set; }
        internal static ControlEntry<List<(string Display, bool Selected)>> PortraitPoses { get; private set; }
        internal static ControlEntry<List<(string Display, bool Selected)>> PortraitPresets { get; private set; }
        internal static ControlEntry<List<(string Display, bool Selected)>> PortraitStates { get; private set; }
        internal static ControlEntry<List<(string Display, bool Selected)>> PortraitAdditional { get; private set; }
        internal static ControlEntry<bool> ApplyPortrait { get; private set; }
        internal static ControlEntry<bool> LockPortrait { get; private set; }

        private const string SectionPortrait = "Portrait";

        internal static void InitializePortrait()
        {
            try
            {
                BService.Control.CreateTable(
                    SectionPortrait,
                    new Translator("立绘", "Portrait"),
                    new Translator("控制主界面角色立绘的姿态与显示状态。",
                        "Control the HUD portrait pose and appearance.")
                    );

                PortraitFilter = Bind(
                    SectionPortrait,
                    nameof(PortraitFilter),
                    PortraitControlRuntime.GetFilter,
                    PortraitControlRuntime.SetFilter,
                    new Translator("姿态筛选", "Pose Filter"),
                    new Translator(
                        "按中文名称或原始标识筛选。",
                        "Filter by name or original key.")
                    );
                PortraitPoses = Bind(
                    SectionPortrait,
                    nameof(PortraitPoses),
                    PortraitControlRuntime.GetPoses,
                    PortraitControlRuntime.SetPoses,
                    new Translator("选择姿态", "Select Pose"),
                    new Translator(
                        "勾选一项；改选时切换到该姿态的首个预设。",
                        "Tick one pose; changing poses selects its first preset.")
                    );
                PortraitPresets = Bind(
                    SectionPortrait,
                    nameof(PortraitPresets),
                    PortraitControlRuntime.GetPresets,
                    PortraitControlRuntime.SetPresets,
                    new Translator("状态预设", "State Preset"),
                    new Translator(
                        "游戏定义的状态组合。可在下方继续调整。",
                        "Game-defined state combinations. Adjust individual flags below.")
                    );
                PortraitStates = Bind(
                    SectionPortrait,
                    nameof(PortraitStates),
                    PortraitControlRuntime.GetStates,
                    PortraitControlRuntime.SetStates,
                    new Translator("主状态", "Main States"),
                    new Translator(
                        "可多选；全部取消表示普通。姿态不支持的标记会在应用时调整。",
                        "Select multiple flags; clear all for normal. Unsupported flags are adjusted when applying.")
                    );
                PortraitAdditional = Bind(
                    SectionPortrait,
                    nameof(PortraitAdditional),
                    PortraitControlRuntime.GetAdditional,
                    PortraitControlRuntime.SetAdditional,
                    new Translator("附加状态", "Additional States"),
                    new Translator(
                        "只控制外观标记，不会改变角色异常状态。",
                        "Appearance flags only; player conditions are unchanged.")
                    );
                ApplyPortrait = BindPulse(
                    SectionPortrait,
                    nameof(ApplyPortrait),
                    PortraitControlRuntime.ApplyOnce,
                    new Translator("应用一次", "Apply Once"),
                    new Translator(
                        "应用所选姿态与状态。未锁定时，游戏随后可以正常切换立绘。",
                        "Apply the selected pose and states. Without locking, the game may change the portrait afterwards.")
                    );
                LockPortrait = Bind(
                    SectionPortrait,
                    nameof(LockPortrait),
                    PortraitControlRuntime.GetLocked,
                    PortraitControlRuntime.SetLocked,
                    new Translator("锁定立绘", "Lock Portrait"),
                    new Translator(
                        "开启后应用选择并保持。锁定期间调整即时生效；关闭后恢复游戏立绘。",
                        "Apply and hold the selection. Edits apply live while locked; turn off to restore game control.")
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize portrait controls.", ex);
            }
        }
    }
}
