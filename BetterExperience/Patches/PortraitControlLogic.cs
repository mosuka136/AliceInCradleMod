using nel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterExperience.Patches
{
    internal struct PortraitSelection : IEquatable<PortraitSelection>
    {
        internal readonly UIEMOT Pose;
        internal readonly UIPictureBase.EMSTATE State;
        internal readonly UIPictureBase.EMSTATE_ADD Additional;

        internal PortraitSelection(UIEMOT pose, UIPictureBase.EMSTATE state = 0,
            UIPictureBase.EMSTATE_ADD additional = 0)
        {
            Pose = pose;
            State = state;
            Additional = additional;
        }

        internal string Key => ((int)Pose) + ":" + (uint)State + ":" + (uint)Additional;
        public bool Equals(PortraitSelection other) => Pose == other.Pose && State == other.State && Additional == other.Additional;
        public override bool Equals(object obj) => obj is PortraitSelection other && Equals(other);
        public override int GetHashCode() => ((int)Pose * 397 ^ (int)State) * 397 ^ (int)Additional;
    }

    /// <summary>立绘目录和选择规则。只处理数据，不访问 Unity 生命周期。</summary>
    internal static class PortraitControlLogic
    {
        internal const UIPictureBase.EMSTATE_ADD SystemAdditional =
            UIPictureBase.EMSTATE_ADD.SENSITIVE | UIPictureBase.EMSTATE_ADD.SP_SENSITIVE;
        internal static readonly uint MainMask = FlagMask(typeof(UIPictureBase.EMSTATE), "SP_SENSITIVE");
        internal static readonly uint AdditionalMask = FlagMask(typeof(UIPictureBase.EMSTATE_ADD), "SENSITIVE", "SP_SENSITIVE");

        private static readonly Dictionary<string, string> Names = new Dictionary<string, string>
        {
            { "STAND", "站立" }, { "BENCH", "坐在长椅上" }, { "MASTURBATE", "自慰" },
            { "DAMAGE_0", "受伤姿态 1" }, { "DAMAGE_1", "受伤姿态 2" }, { "CROUCH", "跌坐" },
            { "WETTEN", "失禁" }, { "LAYING_EGG", "产卵" }, { "DOWN", "倒地" }, { "DOWN_B", "背面倒地" },
            { "DAMAGE_GAS", "毒气" }, { "DAMAGE_GAS_HIT", "毒气受击" }, { "WETTEN_OSGM", "高潮湿身" },
            { "WETTEN_MILK", "乳汁湿身" }, { "TORTURE_SLIME_0", "史莱姆拘束 1" }, { "TORTURE_SLIME_1", "史莱姆拘束 2" },
            { "TORTURE_TENTACLE_0", "触手拘束 1" }, { "TORTURE_TENTACLE_1", "触手拘束 2" }, { "TORTURE_TENTACLE_2", "触手拘束 3" },
            { "TORTURE_SNAKE_0", "土蛇拘束 1" }, { "TORTURE_SNAKE_1", "土蛇拘束 2" },
            { "TORTURE_KETSUDASI", "肛门后入" }, { "TORTURE_KETSUDASI_FINISH", "肛门后入结束" },
            { "TORTURE_GOLEM_INJECT", "木偶侵入" }, { "TORTURE_MKB", "三角木马拘束" }, { "TORTURE_MKB_ACME", "三角木马拘束高潮" },
            { "TORTURE_ROMERO", "幼犬拘束" }, { "TORTURE_GROUNDBURY", "埋地拘束" }, { "TORTURE_SWALLOWED", "吞食拘束" },
            { "TORTURE_DRILLN", "钻头拘束 1" }, { "TORTURE_DRILLN_2", "钻头拘束 2" },
            { "TORTURE_MKB_URCHIN", "剑山拘束" }, { "TORTURE_BACKINJ", "五足侵入" },
            { "TORTURE_SMT", "特殊拘束 SMT" }, { "TORTURE_BACKVORE", "背面吞食" },
            { "TORTURE_LEECHVORE", "蚂蝗吞食 1" }, { "TORTURE_LEECHVORE_2", "蚂蝗吞食 2" },
            { "INSECTED", "虫群附着" }, { "BURNED", "灼烧" }, { "DAMAGE_THUNDER", "雷击" },
            { "DAMAGE_PRESS", "挤压" }, { "DAMAGE_PRESS_T", "上方挤压" }, { "DAMAGE_SYABON", "泡泡拘束" },
            { "DAMAGE_ROPE", "绳索拘束" }, { "ROPE_ORGASM", "绳索高潮" }, { "SHRIMP", "蜷缩" },
            { "PAJAMA", "睡衣" }, { "BUNNY", "兔女郎" }, { "STRIDE_ROPE", "跨坐绳索" },
            { "WEB_TRAPPED", "蛛网拘束" }, { "WEB_TRAPPED_DOWN", "蛛网倒地拘束" },
            { "TORTURE_BSSPIDER", "蜘蛛首领拘束" }, { "VORECOMP", "吞食完成" },
            { "NORMAL", "普通" }, { "DIRT", "污损" }, { "PROG0", "阶段 1" }, { "PROG1", "阶段 2" }, { "PROG2", "阶段 3" },
            { "LOWHP", "低血量表情" }, { "BATTLE", "战斗" }, { "SER", "异常表情" }, { "SHAMED", "羞耻" },
            { "SMASH", "强烈受击" }, { "ABSORBED", "被拘束" }, { "STUNNED", "眩晕" }, { "LOWMP", "低魔力表情" },
            { "EGGED", "怀卵" }, { "WET", "湿润" }, { "BOTE", "腹部膨胀" }, { "ORGASM", "高潮" },
            { "CONFUSED", "混乱" }, { "OSGM", "高潮余韵" }, { "TORNED", "破衣" }, { "SLEEP", "睡眠" },
            { "DEAD", "死亡表情" }, { "STONEOVER", "石化覆盖" }, { "DIRT0", "附加污损" }, { "FROZEN", "冰冻外观" },
            { "FEAR", "恐惧" }, { "CANE_ITEMDROP", "法杖掉落" }
        };

        internal static string Display(string key) => Names.TryGetValue(key, out var name) ? name + " [" + key + "]" : key;

        internal static bool IsMainPose(UIEMOT pose)
        {
            string name = Enum.GetName(typeof(UIEMOT), pose);
            return name != null && !name.StartsWith("_", StringComparison.Ordinal)
                && !name.StartsWith("CUTS_", StringComparison.Ordinal);
        }

        internal static uint FlagMask(Type type, params string[] excluded)
        {
            uint mask = 0;
            foreach (string name in Enum.GetNames(type))
            {
                if (name.StartsWith("_", StringComparison.Ordinal) || excluded.Contains(name)) continue;
                uint bit = Convert.ToUInt32(Enum.Parse(type, name));
                if (bit != 0 && (bit & (bit - 1)) == 0) mask |= bit;
            }
            return mask;
        }

        internal static PortraitSelection Editable(PortraitSelection selection) => new PortraitSelection(selection.Pose,
            (UIPictureBase.EMSTATE)((uint)selection.State & MainMask),
            (UIPictureBase.EMSTATE_ADD)((uint)selection.Additional & AdditionalMask));

        internal static List<PortraitSelection> Presets(UIEMOT pose, IEnumerable<PortraitSelection> source)
            => (source ?? Enumerable.Empty<PortraitSelection>()).Where(value => value.Pose == pose)
                .Select(Editable).Distinct().OrderBy(value => (uint)value.State).ThenBy(value => (uint)value.Additional).ToList();

        internal static string Describe(PortraitSelection value) => Display(value.Pose.ToString()) + ": "
            + DescribeFlags(typeof(UIPictureBase.EMSTATE), (uint)value.State) + " / "
            + DescribeFlags(typeof(UIPictureBase.EMSTATE_ADD), (uint)value.Additional);

        private static string DescribeFlags(Type type, uint bits)
        {
            if (bits == 0) return Display("NORMAL");
            return string.Join(" + ", Enum.GetNames(type).Where(name => !name.StartsWith("_", StringComparison.Ordinal))
                .Where(name => { uint bit = Convert.ToUInt32(Enum.Parse(type, name)); return bit != 0 && (bits & bit) == bit; }).Select(Display));
        }

        // 元组列表的文字列也可编辑：按完整目录标签校验，拒绝篡改、过期和重排的提交。
        internal static bool ValidRows(IReadOnlyList<(string Display, bool Selected)> rows, IReadOnlyList<string> labels)
        {
            if (rows == null || labels == null || rows.Count != labels.Count) return false;
            for (int i = 0; i < rows.Count; i++)
                if (!string.Equals(rows[i].Display, labels[i], StringComparison.Ordinal)) return false;
            return true;
        }

        internal static int SelectOne(IReadOnlyList<(string Display, bool Selected)> rows, IReadOnlyList<string> labels, int previous)
        {
            if (!ValidRows(rows, labels)) return previous;
            for (int i = 0; i < rows.Count; i++)
                if (rows[i].Selected && i != previous) return i;
            return previous >= 0 && previous < rows.Count && rows[previous].Selected ? previous : -1;
        }

        internal static List<(string Display, bool Selected)> FlagRows(Type type, uint allowed, uint selected)
            => Enum.GetNames(type).Select(name => (Name: name, Bit: Convert.ToUInt32(Enum.Parse(type, name))))
                .Where(value => value.Bit != 0 && (value.Bit & (value.Bit - 1)) == 0 && (allowed & value.Bit) != 0)
                .OrderBy(value => value.Bit).Select(value => (Display(value.Name), (selected & value.Bit) != 0)).ToList();

        internal static uint ReadFlags(Type type, uint allowed, uint previous, IReadOnlyList<(string Display, bool Selected)> rows)
        {
            var expected = FlagRows(type, allowed, previous);
            if (!ValidRows(rows, expected.Select(row => row.Display).ToList())) return previous;
            var bits = Enum.GetValues(type).Cast<object>().Select(Convert.ToUInt32)
                .Where(bit => bit != 0 && (bit & (bit - 1)) == 0 && (allowed & bit) != 0).Distinct().OrderBy(bit => bit).ToArray();
            uint result = 0;
            for (int i = 0; i < rows.Count; i++) if (rows[i].Selected) result |= bits[i];
            return result;
        }
    }
}
