# BetterExperience/更好的体验

[![GitHub all releases](https://img.shields.io/github/downloads/mosuka136/AliceInCradleMod/total)](https://github.com/mosuka136/AliceInCradleMod/releases) [![GitHub release (latest by date)](https://img.shields.io/github/v/release/mosuka136/AliceInCradleMod)](https://github.com/mosuka136/AliceInCradleMod/releases) ![platform](https://img.shields.io/badge/platform-Windows-lightgrey)

中文 | [English](README_EN.md)

## 总体介绍

`BetterExperience/更好的体验` 是一个基于 `BepInEx`、`Harmony` 与 `UnityModBase` 的 `Alice In Cradle` Mod（模组）。

该 Mod 通过 `Harmony` 补丁在游戏运行时修改逻辑，提供可配置的体验优化、数值调整、限制解除、小游戏辅助与战斗统计。通过 `UnityModBase` 提供中英双语的配置、日志和实时控制界面，可按需开启功能或直接调整当前游戏状态。

## 实现的功能

- 体验便利：商店页面内一键刷新商品、商店购买/出售价格倍率、酒店餐券不消耗与数量设置、改良存档点、随时访问仓库
- 钓鱼辅助：自动瞄准、抛竿、起竿、跟随鱼标并确认结算；可调整鱼标移动、套圈大小与判定容差、失误掉条倍率和收杆次数
- 挤奶辅助：自动寻找奶牛并完成满级挤奶、满级自动松手、放宽过压判定、奶量不减、奔跑不扰牛、解除牛疲劳开局限制
- 酒吧辅助：自动配送酒水、禁止客人扑人与 QTE、低体力时酒款不模糊且行走不减速、端酒时可吃零食
- 道场辅助：放宽出拳判定、自动打出克制手势
- 转轮调整：更好的转轮效果、指定福袋效果、转轮速度与战斗后可抽取转轮数量倍率
- 战斗换杖：配置次数、无限立即切换次数、切换期间免疫攻击不被打断、法杖收纳槽数量可调
- 料理制作：预览蘑菇随机效果，在确认界面一键重新随机；战斗后食物不腐败，并保留咖啡机餐券与酒店餐券
- 战斗点预览：显示具体魔物、污染体、强化属性及数量范围
- 移动辅助：鼠标指向传送、四方向穿墙飞行、玩家跳跃力度倍率
- 数值调整：HP/MP/EP、货币数量、酒吧积分、公会积分与等级、最大饱食度、饱食不消耗、物品成组不消耗（快捷栏图标以彩虹流动标记）、移动速度、掉落倍率、法杖属性、危险度、怀卵数量与种类等；可锁定货币、酒吧积分与公会积分
- 容量调整：背包容量、空瓶收纳槽数量、宝箱收纳槽数量、手雷收纳槽数量、强化插槽数量、过充插槽数量等
- 生存/战斗保护与统计：无 HP/MP/EP 伤害、无地图伤害、免疫异常状态、无限护盾、不被攻击、免疫敌怪拘束攻击、拘束挣脱自动完成、防止败北（原地恢复继续游戏）、秒杀敌人、出现即秒杀敌人、战斗统计与伤害计数器等
- 陷阱与环境：溺水、挤压伤害、跌倒、蓝条破碎、虫墙、雾视觉效果等可按配置启用或禁用
- 限制解除：木偶商人生成限制、椅子菜单限制、宝箱限制、仓库区域限制等
- 地图与天气：随时快速传送、夜间椅子传送、锁定天气、锁定危险度、出门可用曾达最高危险度、移除特定区域黑暗效果、天气强制设置、流浪商人传唤到当前地图、商人地图常显与必定出现、夜间魔物阵白天常开、魔力草恢复时间上限
- 公会任务：公会柜台内一键刷新任务板
- 视觉相关：去除马赛克、不沾污渍、不浸湿
- 调试与给予：debug 开关；实时控制的“给予”页可按名称筛选物品、技能或配方，设置物品品级与数量并给予/解锁

## 使用方法

### 1. 前置条件

- 已安装 `Alice In Cradle`（[下载链接](https://get.aliceincradle.dev/win/)）
- 已正确安装 `BepInEx`（[下载链接](https://github.com/BepInEx/BepInEx/releases)）
- 已安装 `UnityModBase`（UMB）（[下载链接](https://github.com/mosuka136/UnityModBase/releases)）

### 2. 安装 BepInEx

1. 前往 `BepInEx` 发布页下载适用于 Windows 的 `BepInEx 5` 压缩包（通常选择 `x64` 版本）
2. 解压压缩包，将其中全部文件复制到 `Alice In Cradle` 游戏根目录（与游戏 `.exe` 同级）
3. 启动游戏一次，等待 `BepInEx` 完成初始化
4. 关闭游戏

### 3. 安装 UnityModBase

1. 构建或下载 `UnityModBase.dll` 与 `UnityModBase.BepInExLauncher.dll`
2. 将两个文件放入游戏目录下的 `BepInEx/plugins/`，已有其他模组共用时保持一份即可

`UnityModBase` 为本 Mod 提供配置、日志、热键与实时控制界面，两个 DLL 都需要安装。

### 4. 安装 BetterExperience

1. 构建或下载 `BetterExperience.dll`（[下载链接](https://github.com/mosuka136/AliceInCradleMod/releases)）
2. 将 `BetterExperience.dll` 放入游戏目录下的 `BepInEx/plugins/BetterExperience/`（或 `BepInEx/plugins/`）目录
3. 启动游戏一次，生成 Mod 配置文件与日志目录
4. 在游戏中按下 `F1` 打开配置界面，选择 `BetterExperience/更好的体验` 后按需启用功能；也可编辑 `BepInEx/plugins/BetterExperience/BetterExperience.cfg`
5. 读档后按下 `F3` 打开实时控制界面，调整当前数值，或开启自动钓鱼、自动挤奶、自动配送酒水等功能
6. 若在游戏运行时手动修改配置文件，请先保存文件，再按 `Ctrl+R` 重新加载；标注需在游戏启动前设置的项目应重启游戏后生效

### 5. 配置与实时控制

- **配置界面（`F1`）**：保存功能开关、倍率与偏好，界面修改会自动写入配置文件。
- **实时控制界面（`F3`）**：直接查看和修改当前游戏状态。控制项自身不写入 Mod 配置文件
- **日志界面（`F2`）**：查看运行日志；日志文件位于 `BepInEx/plugins/BetterExperience/logs/`

### 6. 默认热键

| 热键 | 功能 |
| --- | --- |
| `F1` | 打开配置界面 |
| `F2` | 打开日志界面 |
| `F3` | 打开实时控制界面 |
| `Ctrl+G` | 传送到鼠标指向的位置（需启用“鼠标传送”） |
| `Ctrl+N` | 切换穿墙飞行（需启用“穿墙热键”） |

界面与配置重载热键在 `UnityModBase` 配置中修改；鼠标传送与穿墙热键在本 Mod 的“热键”配置中修改。热键支持键盘组合、手柄输入和用逗号分隔的多个备选组合。

## 支持的版本

- `Alice In Cradle`: `ver030h`
- `BepInEx`: `v5.4.23.5`
- `UnityModBase`: `v1.1.0`

## Mod 开发模板

仓库提供适用于 Visual Studio 2022/2026 的 VSIX 项目模板，可生成基于 BepInEx 与 UnityModBase 的 Mod 工程（.NET Framework 4.7.2）及 xUnit 测试工程（.NET 8）。构建、安装与使用方法见 [模板说明](templates/README.md)。

## 许可证

`BetterExperience/更好的体验` 使用 `LGPL-3.0` 许可证，详见 `LICENSE.txt`。
