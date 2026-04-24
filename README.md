# BetterExperience/更好的体验

[![GitHub all releases](https://img.shields.io/github/downloads/mosuka136/AliceInCradleMod/total)](https://github.com/mosuka136/AliceInCradleMod/releases) [![GitHub release (latest by date)](https://img.shields.io/github/v/release/mosuka136/AliceInCradleMod)](https://github.com/mosuka136/AliceInCradleMod/releases) ![platform](https://img.shields.io/badge/platform-Windows-lightgrey)

English version: [README_EN.md](README_EN.md)

## 总体介绍

`BetterExperience/更好的体验` 是一个基于 `BepInEx` 与 `Harmony` 的 `Alice In Cradle` Mod（模组）。

该 Mod 通过 `Harmony` 补丁在游戏运行时修改逻辑，提供可配置的体验优化、数值调整、限制解除、便利功能、内置可视化配置界面，以及贴图替换能力。大部分功能都可以通过配置文件或游戏内配置界面按需开启或关闭。

## 实现的功能

- 基础控制：模组总开关、独立配置文件、内置配置界面、中英双语、配置热重载、日志输出等级控制
- 体验便利：一键刷新商店、改良存档点、随时访问仓库、改良钓鱼体验、调整转轮相关效果
- 数值调整：HP/MP/EP、货币数量、最大饱食度、移动速度、掉落倍率、法杖属性、危险度等
- 容量调整：背包容量、空瓶收纳槽数量、强化插槽数量、过充插槽数量等
- 生存/战斗保护：无 HP/MP/EP 伤害、无地图伤害、免疫异常状态、无限护盾、不被攻击等
- 陷阱与环境：溺水、挤压伤害、跌倒、蓝条破碎、虫墙、雾视觉效果等可按配置启用或禁用
- 限制解除：木偶商人生成限制、椅子菜单限制、宝箱限制、仓库区域限制等
- 地图与天气：随时快速传送、移除特定区域黑暗效果、天气强制设置（旋风/雷暴/雾/干旱/浓雾/瘟疫）
- 视觉相关：去除马赛克、贴图替换、敏感内容贴图开关、运行时刷新贴图
- 热键相关：支持组合键、多个备选热键与手柄输入

## 使用方法

### 1. 前置条件

- 已安装 `Alice In Cradle`（[下载链接](https://get.aliceincradle.dev/win/)）
- 已正确安装 `BepInEx`（[下载链接](https://github.com/BepInEx/BepInEx/releases)）

### 2. 安装 BepInEx

1. 前往 `BepInEx` 发布页下载适用于 Windows 的 `BepInEx 5` 压缩包（通常选择 `x64` 版本）
2. 解压压缩包，将其中全部文件复制到 `Alice In Cradle` 游戏根目录（与游戏 `.exe` 同级）
3. 启动游戏一次，等待 `BepInEx` 完成初始化
4. 关闭游戏

### 3. 安装 BetterExperience

1. 构建或下载 `BetterExperience.dll`（[下载链接](https://github.com/mosuka136/AliceInCradleMod/releases)）
2. 将 `BetterExperience.dll` 放入游戏目录下的 `BepInEx/plugins/BetterExperience/`（或 `BepInEx/plugins/`）目录
3. 启动游戏一次，生成 Mod 配置文件、日志目录与贴图目录
4. 编辑配置文件 `BepInEx/plugins/BetterExperience/BetterExperience.cfg`，或在游戏中按下 `F1` 打开内置配置界面
5. 若需在游戏运行时手动修改配置文件，请先保存文件，然后在游戏中按下你设置的 `重新加载配置` 热键；如未修改，默认热键为 `Ctrl+R`
6. 如需使用贴图替换，将 `.png` 或 `.btep` 文件放入 `BepInEx/plugins/BetterExperience/ReplaceTexture/`；敏感内容贴图放入 `BepInEx/plugins/BetterExperience/ReplaceTexture/Sensitive/`；在游戏中按下 `Ctrl+T` 可重新加载贴图

## 支持的版本

- `Alice In Cradle`：`ver029j2`
- `BepInEx`：`v5.4.23.5`

## 许可证

`BetterExperience/更好的体验` 使用 `LGPL-3.0` 许可证，详见 `LICENSE.txt`。
