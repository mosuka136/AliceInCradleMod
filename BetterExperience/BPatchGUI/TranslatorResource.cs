using BetterExperience.HTranslatorSpace;
using nel;
using System.Collections.Generic;

namespace BetterExperience.BPatchGUI
{
    public static class TranslatorResource
    {
        public readonly static Translator BattleStatisticsTitle = new Translator("战斗统计", "Battle Statistics");
        public readonly static Translator BattleDuration = new Translator("战斗时长：", "Battle Duration:");
        public readonly static Translator AverageDps = new Translator("平均DPS：", "Average DPS:");
        public readonly static Translator TotalStatistics = new Translator("累计统计", "Total Statistics");
        public readonly static Translator CurrentBattleStatistics = new Translator("本场统计", "Current Battle Statistics");
        public readonly static Translator PlayerDamage = new Translator("玩家造成：", "Player Damage:");
        public readonly static Translator PlayerInjury = new Translator("玩家承伤：", "Player Injury:");
        public readonly static Translator TotalDamage = new Translator("总伤害：", "Total Damage:");
        public readonly static Translator EnemyDamage = new Translator("敌人伤害：", "Enemy Damage:");
        public readonly static Translator EnemyInjury = new Translator("敌人承伤：", "Enemy Injury:");
        public readonly static Translator ContaminatedSuffix = new Translator("（污染体）", "(Contaminated)");
        public readonly static Translator AttributeLeftBracket = new Translator("（", " (");
        public readonly static Translator AttributeRightBracket = new Translator("）", ")");
        public readonly static Translator Hp = new Translator("HP", "HP");
        public readonly static Translator Mp = new Translator("MP", "MP");
        public readonly static Translator CollapseDetails = new Translator("收起详细信息", "Collapse Details");
        public readonly static Translator ExpandDetails = new Translator("展开详细信息", "Expand Details");

        public readonly static Dictionary<ENEMYID, Translator> EnemyNames = new Dictionary<ENEMYID, Translator>
        {
            { ENEMYID.SLIME_0, new Translator("史莱姆", "Slime") },
            { ENEMYID.SLIME_TUTORIAL, new Translator("史莱姆", "Slime") },
            { ENEMYID.SLIME_0_FLW, new Translator("史莱姆", "Slime") },
            { ENEMYID.SLIME_TUTORIAL_GARAGE, new Translator("史莱姆", "Slime") },
            { ENEMYID.MUSH_0, new Translator("蘑菇", "Mushroom") },
            { ENEMYID.MUSH_0_FLW, new Translator("蘑菇", "Mushroom") },
            { ENEMYID.MAGE_0, new Translator("愚者", "The Fool") },
            { ENEMYID.PUPPY_0, new Translator("幼犬", "Puppy") },
            { ENEMYID.PUPPY_EVENT_0, new Translator("幼犬", "Puppy") },
            { ENEMYID.GOLEM_0, new Translator("木偶", "Puppet") },
            { ENEMYID.GOLEM_0_NM, new Translator("木偶（静止）", "Puppet (Non-builder)") },
            { ENEMYID.SNAKE_0, new Translator("土蛇", "Mole Snake") },
            { ENEMYID.SNAKE_TUTORIAL, new Translator("土蛇", "Mole Snake") },
            { ENEMYID.SPONGE_0, new Translator("海绵", "Porifera") },
            { ENEMYID.SPONGE_1, new Translator("海绵", "Porifera") },
            { ENEMYID.UNI_0, new Translator("剑山", "Urchin") },
            { ENEMYID.FOX_0, new Translator("妖狐", "Nine-tailed Fox") },
            { ENEMYID.GOLEMTOY_MKB, new Translator("木马", "Wooden Horse") },
            { ENEMYID.GOLEMTOY_RM, new Translator("杂波干扰器", "Rainmaker") },
            { ENEMYID.GOLEMTOY_POD, new Translator("导弹发射架", "Missile Launcher") },
            { ENEMYID.GOLEMTOY_BOW, new Translator("光束发射台", "Laser") },
            { ENEMYID.GOLEMTOY_CATAPULT, new Translator("发射台", "Catapult") },
            { ENEMYID.GOLEMTOY_CATAPULT_0, new Translator("发射台", "Catapult") },
            { ENEMYID.GECKO_0, new Translator("壁虎", "Lizard") },
            { ENEMYID.GECKO_0_FLW, new Translator("壁虎", "Lizard") },
            { ENEMYID.FROG_0, new Translator("沼蛙", "Frog") },
            { ENEMYID.BOSS_NUSI_0, new Translator("森之领主", "Lord of the Forest") },
            { ENEMYID.BOSS_NUSI_CAGE, new Translator("森之领主牢笼", "Lord of the Forest Cage") },
            { ENEMYID.BOSS_NUSI_TENTACLE, new Translator("森之领主触手", "Lord of the Forest Tentacle") },
            { ENEMYID.MECHGOLEM_0, new Translator("机甲木偶", "Armored Puppet") },
            { ENEMYID.MECHGOLEM_1, new Translator("机甲木偶", "Armored Puppet") },
            { ENEMYID.PENTAPOD_0, new Translator("五足", "Pentapod") },
            { ENEMYID.PENTAPOD_0_NM, new Translator("五足", "Pentapod") },
            { ENEMYID.PENTAPOD_HEAD_0, new Translator("五足头部", "Pentapod Head") },
            { ENEMYID.PIG_0, new Translator("野猪", "Boar") },
            { ENEMYID.ROAPER_0, new Translator("触须怪", "Roper") },
            { ENEMYID.RAMDA_0, new Translator("酒壶", "Lambda") },
            { ENEMYID.EHOME_0, new Translator("巢厄", "Hive") },
            { ENEMYID.HONEYCOMB_0, new Translator("蜂巢", "Honeycomb") },
            { ENEMYID.LEECH_0, new Translator("蚂蟥", "Leech") },
            { ENEMYID.LEECHWCNC_0, new Translator("蚂蟥", "Leech") },
            { ENEMYID.LEECHQUEEN_0, new Translator("女王蚂蟥", "Queen Leech") },
            { ENEMYID.LEECHQUEENWCNC_0, new Translator("女王蚂蟥", "Queen Leech") },
            { ENEMYID.EMPRESS_0, new Translator("皇后", "Empress") },
            { ENEMYID.BOSS_SPIDER_0, new Translator("山蜘蛛", "Mountain Spider") },
        };

        public readonly static Dictionary<ENATTR, Translator> EnemyAttributes = new Dictionary<ENATTR, Translator>
        {
            { ENATTR.ATK, new Translator("攻击强化", "ATK Up") },
            { ENATTR.DEF, new Translator("防御强化", "DEF Up") },
            { ENATTR.MP_STABLE, new Translator("魔力稳态", "MP Stable") },
            { ENATTR.FIRE, new Translator("野火", "Fire") },
            { ENATTR.ICE, new Translator("冰霜", "Ice") },
            { ENATTR.THUNDER, new Translator("雷电", "Elec") },
            { ENATTR.SLIMY, new Translator("黏液", "Slimy") },
            { ENATTR.ACME, new Translator("媚毒", "Aphro.Gas") },
            { ENATTR.INVISIBLE, new Translator("隐身", "Invisible") },
            { ENATTR.BIG, new Translator("巨大化", "Giant") },
        };

        public static string GetEnemyDisplayName(ENEMYID enemyId, ENATTR attribute)
        {
            var name = GetEnemyName(enemyId);
            var attributeName = GetEnemyAttributeName(attribute);

            if (string.IsNullOrEmpty(attributeName))
                return name;

            return name + AttributeLeftBracket + attributeName + AttributeRightBracket;
        }

        public static string GetEnemyName(ENEMYID enemyId)
        {
            var isContaminated = (enemyId & ENEMYID._OVERDRIVE_FLAG) != 0;
            var baseEnemyId = enemyId & ~ENEMYID._OVERDRIVE_FLAG;

            Translator translator;
            var name = EnemyNames.TryGetValue(baseEnemyId, out translator)
                ? translator.ToString()
                : baseEnemyId.ToString();

            return isContaminated ? name + ContaminatedSuffix : name;
        }

        public static string GetEnemyAttributeName(ENATTR attribute)
        {
            attribute &= ~ENATTR.__OPTIONAL;

            if (attribute == ENATTR.NORMAL)
                return string.Empty;

            var parts = new List<string>();

            AppendEnemyAttributeName(parts, attribute, ENATTR.ATK);
            AppendEnemyAttributeName(parts, attribute, ENATTR.DEF);
            AppendEnemyAttributeName(parts, attribute, ENATTR.MP_STABLE);
            AppendEnemyAttributeName(parts, attribute, ENATTR.FIRE);
            AppendEnemyAttributeName(parts, attribute, ENATTR.ICE);
            AppendEnemyAttributeName(parts, attribute, ENATTR.THUNDER);
            AppendEnemyAttributeName(parts, attribute, ENATTR.SLIMY);
            AppendEnemyAttributeName(parts, attribute, ENATTR.ACME);
            AppendEnemyAttributeName(parts, attribute, ENATTR.INVISIBLE);
            AppendEnemyAttributeName(parts, attribute, ENATTR.BIG);

            var knownAttributes = ENATTR._AATTR | ENATTR._MATTR | ENATTR._KIND2;
            var unknownAttributes = attribute & ~knownAttributes;
            if (unknownAttributes != ENATTR.NORMAL)
                parts.Add(unknownAttributes.ToString());

            return string.Join("+", parts.ToArray());
        }

        private static void AppendEnemyAttributeName(List<string> parts, ENATTR attribute, ENATTR targetAttribute)
        {
            if ((attribute & targetAttribute) == ENATTR.NORMAL)
                return;

            Translator translator;
            parts.Add(EnemyAttributes.TryGetValue(targetAttribute, out translator)
                ? translator.ToString()
                : targetAttribute.ToString());
        }
    }
}
