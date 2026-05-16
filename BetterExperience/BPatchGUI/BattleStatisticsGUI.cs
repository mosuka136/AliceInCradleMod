using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
using nel;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static BetterExperience.Patches.HPatches;

namespace BetterExperience.BPatchGUI
{
    /// <summary>
    /// 战斗统计悬浮窗口。
    /// 该组件只读取 <see cref="BattleStatisticsPatch"/> 收集到的计数并绘制 IMGUI，不负责统计数据的采集或清零。
    /// </summary>
    [RegisterOnGameBoot]
    public class BattleStatisticsGUI : MonoBehaviour
    {
        private const float _collapsedWidth = 260f;
        private const float _collapsedHeight = 240f;
        private const float _expandedWidth = 350f;
        private const float _expandedHeight = 500f;
        private const float _windowMargin = 10f;

        private Rect _windowRect;
        private Vector2 _scrollPosition;
        private bool _isExpanded = false;

        public readonly int WindowID = Guid.NewGuid().GetHashCode();


        private void Awake()
        {
            _windowRect = new Rect(Screen.width - _windowMargin, (Screen.height - _collapsedHeight) / 2f, _collapsedWidth, _collapsedHeight);
            ClampWindowToScreen();
        }

        private void Update()
        {
        }

        private void OnGUI()
        {
            if (!ConfigManager.EnableDamageCounter.Value)
                return;

            if (_isExpanded)
            {
                _windowRect.width = _expandedWidth;
                _windowRect.height = _expandedHeight;
            }
            else
            {
                _windowRect.width = _collapsedWidth;
                _windowRect.height = _collapsedHeight;
            }

            _windowRect = GUI.Window(WindowID, _windowRect, DrawWindow, TranslatorResource.BattleStatisticsTitle);

            ClampWindowToScreen();
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginVertical();

            GUILayout.Label(TranslatorResource.BattleDuration + GetBattleDurationText());
            GUILayout.Label(TranslatorResource.AverageDps + GetAverageDpsText());

            GUILayout.Space(8f);

            if (_isExpanded)
            {
                _scrollPosition = GUILayout.BeginScrollView(
                    _scrollPosition,
                    false,
                    false,
                    GUILayout.ExpandHeight(true));

                GUILayout.BeginVertical("box");
            }
            else
                GUILayout.BeginVertical();

            GUILayout.Label(TranslatorResource.CurrentBattleStatistics);
            DrawStatisticsBlock(
                BattleStatisticsPatch.TotalPlayerDamageSingleBattleHpCounter,
                BattleStatisticsPatch.TotalPlayerDamageSingleBattleMpCounter,
                BattleStatisticsPatch.TotalPlayerInjurySingleBattleHpCounter,
                BattleStatisticsPatch.TotalPlayerInjurySingleBattleMpCounter,
                BattleStatisticsPatch.TotalEnemyDamageSingleBattleHpCounter,
                BattleStatisticsPatch.TotalEnemyDamageSingleBattleMpCounter,
                BattleStatisticsPatch.TotalEnemyInjurySingleBattleHpCounter,
                BattleStatisticsPatch.TotalEnemyInjurySingleBattleMpCounter,
                BattleStatisticsPatch.PlayerDamageSingleBattleHpCounter,
                BattleStatisticsPatch.PlayerDamageSingleBattleMpCounter,
                BattleStatisticsPatch.PlayerInjurySingleBattleHpCounter,
                BattleStatisticsPatch.PlayerInjurySingleBattleMpCounter,
                BattleStatisticsPatch.EnemyDamageSingleBattleHpCounter,
                BattleStatisticsPatch.EnemyDamageSingleBattleMpCounter,
                BattleStatisticsPatch.EnemyInjurySingleBattleHpCounter,
                BattleStatisticsPatch.EnemyInjurySingleBattleMpCounter);
            GUILayout.EndVertical();

            if (_isExpanded)
            {
                GUILayout.Space(8f);
                GUILayout.BeginVertical("box");
                GUILayout.Label(TranslatorResource.TotalStatistics);
                DrawStatisticsBlock(
                    BattleStatisticsPatch.TotalPlayerDamageHpCounter,
                    BattleStatisticsPatch.TotalPlayerDamageMpCounter,
                    BattleStatisticsPatch.TotalPlayerInjuryHpCounter,
                    BattleStatisticsPatch.TotalPlayerInjuryMpCounter,
                    BattleStatisticsPatch.TotalEnemyDamageHpCounter,
                    BattleStatisticsPatch.TotalEnemyDamageMpCounter,
                    BattleStatisticsPatch.TotalEnemyInjuryHpCounter,
                    BattleStatisticsPatch.TotalEnemyInjuryMpCounter,
                    BattleStatisticsPatch.PlayerDamageHpCounter,
                    BattleStatisticsPatch.PlayerDamageMpCounter,
                    BattleStatisticsPatch.PlayerInjuryHpCounter,
                    BattleStatisticsPatch.PlayerInjuryMpCounter,
                    BattleStatisticsPatch.EnemyDamageHpCounter,
                    BattleStatisticsPatch.EnemyDamageMpCounter,
                    BattleStatisticsPatch.EnemyInjuryHpCounter,
                    BattleStatisticsPatch.EnemyInjuryMpCounter);
                GUILayout.EndVertical();

                GUILayout.EndScrollView();
            }

            if (GUILayout.Button(_isExpanded ? TranslatorResource.CollapseDetails : TranslatorResource.ExpandDetails, GUILayout.ExpandWidth(true)))
                _isExpanded = !_isExpanded;

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void DrawStatisticsBlock(
            int playerDamageHp,
            int playerDamageMp,
            int playerInjuryHp,
            int playerInjuryMp,
            int enemyDamageHp,
            int enemyDamageMp,
            int enemyInjuryHp,
            int enemyInjuryMp,
            Dictionary<(ENEMYID, ENATTR), int> playerDamageHpByEnemy,
            Dictionary<(ENEMYID, ENATTR), int> playerDamageMpByEnemy,
            Dictionary<(ENEMYID, ENATTR), int> playerInjuryHpByEnemy,
            Dictionary<(ENEMYID, ENATTR), int> playerInjuryMpByEnemy,
            Dictionary<(ENEMYID, ENATTR), int> enemyDamageHpByEnemy,
            Dictionary<(ENEMYID, ENATTR), int> enemyDamageMpByEnemy,
            Dictionary<(ENEMYID, ENATTR), int> enemyInjuryHpByEnemy,
            Dictionary<(ENEMYID, ENATTR), int> enemyInjuryMpByEnemy)
        {
            DrawStatisticsLine(TranslatorResource.PlayerDamage, playerDamageHp, playerDamageMp, playerDamageHpByEnemy, playerDamageMpByEnemy);
            DrawStatisticsLine(TranslatorResource.PlayerInjury, playerInjuryHp, playerInjuryMp, playerInjuryHpByEnemy, playerInjuryMpByEnemy);
            DrawStatisticsLine(TranslatorResource.EnemyDamage, enemyDamageHp, enemyDamageMp, enemyDamageHpByEnemy, enemyDamageMpByEnemy);
            DrawStatisticsLine(TranslatorResource.EnemyInjury, enemyInjuryHp, enemyInjuryMp, enemyInjuryHpByEnemy, enemyInjuryMpByEnemy);
        }

        private void DrawStatisticsLine(string label, int totalHp, int totalMp, Dictionary<(ENEMYID, ENATTR), int> hpByEnemy, Dictionary<(ENEMYID, ENATTR), int> mpByEnemy)
        {
            GUILayout.Label(FormatHpMpLine(label, totalHp, totalMp));

            if (!_isExpanded)
                return;

            // 展开视图需要同时显示 HP 与 MP 维度，两个字典可能只在其中一个维度存在该敌人。
            var enemyKeys = new SortedSet<(ENEMYID, ENATTR)>(hpByEnemy.Keys);
            enemyKeys.UnionWith(mpByEnemy.Keys);

            if (enemyKeys.Count == 0)
                return;

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical("box");
            foreach (var key in enemyKeys)
            {
                int hp, mp;
                hp = hpByEnemy.TryGetValue(key, out hp) ? hp : 0;
                mp = mpByEnemy.TryGetValue(key, out mp) ? mp : 0;

                if (hp == 0 && mp == 0)
                    continue;

                var enemyLabel = TranslatorResource.GetEnemyDisplayName(key.Item1, key.Item2) + ": ";
                GUILayout.Label(FormatHpMpLine(enemyLabel, hp, mp));
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private string FormatHpMpLine(string label, int hp, int mp)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1} {2} / {3} {4}",
                label,
                hp,
                TranslatorResource.Hp,
                mp,
                TranslatorResource.Mp);
        }

        private string GetBattleDurationText()
        {
            return GetBattleDuration().ToString("0.0", CultureInfo.InvariantCulture) + "s";
        }

        private string GetAverageDpsText()
        {
            var duration = GetBattleDuration();
            if (duration <= 0f)
                return "0.0";

            var dps = BattleStatisticsPatch.TotalPlayerDamageSingleBattleHpCounter / duration;
            return dps.ToString("0.0", CultureInfo.InvariantCulture);
        }

        private float GetBattleDuration()
        {
            var duration = BattleStatisticsPatch.IsInBattle
                ? Time.time - BattleStatisticsPatch.BattleStartTime
                : BattleStatisticsPatch.BattleEndTime - BattleStatisticsPatch.BattleStartTime;

            if (duration < 0f)
                return 0f;

            return duration;
        }

        private void ClampWindowToScreen()
        {
            var maxX = Mathf.Max(_windowMargin, Screen.width - _windowRect.width - _windowMargin);
            var maxY = Mathf.Max(_windowMargin, Screen.height - _windowRect.height - _windowMargin);

            _windowRect.x = Mathf.Clamp(_windowRect.x, _windowMargin, maxX);
            _windowRect.y = Mathf.Clamp(_windowRect.y, _windowMargin, maxY);
        }
    }
}
