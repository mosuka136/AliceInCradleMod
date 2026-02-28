# BetterExperience/更好的体验

[![GitHub all releases](https://img.shields.io/github/downloads/user/repo/total)](https://github.com/user/repo/releases) [![GitHub release (latest by date)](https://img.shields.io/github/v/release/user/repo)](https://github.com/user/repo/releases) ![platform](https://img.shields.io/badge/platform-Windows-lightgrey)

English version: [README_EN.md](README_EN.md)

## 总体介绍

`BetterExperience/更好的体验` 是一个基于 `BepInEx` 的 `Alice In Cradle` Mod（模组）。

该 Mod 通过 `Harmony` 补丁在游戏运行时修改逻辑，提供可配置的体验优化、数值调整、限制解除、便利功能，以及部分贴图替换能力。大部分功能都可以通过 `BepInEx` 配置文件按需开启或关闭。

## 实现的功能

- 基础控制：模组总开关、配置热重载、日志输出等级控制
- 体验便利：一键刷新商店、改良存档点、随时访问仓库、改良钓鱼体验、调整转轮相关效果
- 数值调整：HP/MP/EP、货币数量、最大饱食度、移动速度、掉落倍率、法杖属性等
- 容量调整：背包容量、空瓶收纳槽数量、强化插槽数量、过充插槽数量等
- 生存/战斗保护：无 HP/MP/EP 伤害、无地图伤害、免疫异常状态、无限护盾、不被攻击等
- 陷阱与环境：禁用溺水、无挤压伤害、无跌倒、无蓝条破碎、无效化虫墙等
- 限制解除：木偶商人生成限制、椅子菜单限制、宝箱限制、仓库区域限制等
- 地图与天气：随时快速传送、移除特定区域黑暗效果、天气强制设置（旋风/雷暴/雾/干旱/浓雾/瘟疫）、雾视觉遮挡开关
- 视觉相关：去除马赛克、贴图替换
- 调试相关：debug开关

## 使用方法

### 1. 前置条件

- 已安装 `Alice In Cradle`（[下载链接](https://get.aliceincradle.dev/win/)）
- 已正确安装 `BepInEx`（[下载链接](https://github.com/BepInEx/BepInEx/releases)）
- 推荐同时安装 `ConfigurationManager`，方便管理配置（[下载链接](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases)）

### 2. 安装 BepInEx

1. 前往 `BepInEx` 发布页下载适用于 Windows 的 `BepInEx 5` 压缩包（通常选择 `x64` 版本）
2. 解压压缩包，将其中全部文件复制到 `Alice In Cradle` 游戏根目录（与游戏 `.exe` 同级）
3. 启动游戏一次，等待 `BepInEx` 完成初始化
4. 关闭游戏

### 3. 安装 BetterExperience

1. 构建或下载 `BetterExperience.dll`（[下载链接]()）
2. 将 `BetterExperience.dll` 放入游戏目录下的 `BepInEx/plugins/BetterExperience/`（或 `BepInEx/plugins/`）目录
3. 启动游戏一次，生成 Mod 配置文件
4. 编辑配置文件 `BepInEx/config/com.buele.betterexperience.cfg`
5. 若需在游戏运行时修改配置，请先保存配置文件，然后在游戏中按下你设置的 `重新加载配置` 热键；如未修改，默认热键为 `Ctrl+R`
6. 如需安装 `ConfigurationManager`，解压压缩包后，进入其中的 `BepInEx/plugins` 目录，将 `ConfigurationManager` 文件夹复制到游戏目录的 `BepInEx/plugins` 下即可。在游戏中按下 `F1` 可打开 `ConfigurationManager` 界面

## 支持的版本

- `Alice In Cradle` `ver029f`
- `BepInEx` `v5.4.23.5`
- `ConfigurationManager` `v18.4.1`

## 许可证

`BetterExperience/更好的体验` 使用 `LGPL-3.0` 许可证，详见 `LICENSE.txt`。
