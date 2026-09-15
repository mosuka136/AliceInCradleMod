# BetterExperience/更好的体验

[![GitHub all releases](https://img.shields.io/github/downloads/mosuka136/AliceInCradleMod/total)](https://github.com/mosuka136/AliceInCradleMod/releases) [![GitHub release (latest by date)](https://img.shields.io/github/v/release/mosuka136/AliceInCradleMod)](https://github.com/mosuka136/AliceInCradleMod/releases) ![platform](https://img.shields.io/badge/platform-Windows-lightgrey)

中文 | [English](README_EN.md)

## 总体介绍

`BetterExperience/更好的体验` 是一个基于 `BepInEx` 与 `Harmony` 的 `Alice In Cradle` Mod（模组）。

该 Mod 通过 `Harmony` 补丁在游戏运行时修改逻辑，提供可配置的体验优化、数值调整、限制解除、便利功能、内置可视化配置界面、战斗统计，以及贴图替换能力。大部分功能都可以通过配置文件或游戏内配置界面按需开启或关闭。

## 实现的功能

- 基础控制：模组总开关、独立配置文件、内置配置界面、中英双语、配置热重载、日志输出等级控制
- 体验便利：一键刷新商店、改良存档点、随时访问仓库、改良钓鱼体验、道场判定放宽与自动命中、调整转轮相关效果
- 料理制作：预览蘑菇随机效果，在确认界面一键重新随机
- 战斗点预览：显示具体魔物、污染体、强化属性及数量范围
- 移动辅助：鼠标指向传送、四方向穿墙飞行、玩家跳跃力度倍率
- 数值调整：HP/MP/EP、货币数量、最大饱食度、饱食不消耗、移动速度、掉落倍率、法杖属性、危险度等
- 容量调整：背包容量、空瓶收纳槽数量、强化插槽数量、过充插槽数量等
- 生存/战斗保护与统计：无 HP/MP/EP 伤害、无地图伤害、免疫异常状态、无限护盾、不被攻击、拘束挣脱自动完成、战斗统计与伤害计数器等
- 陷阱与环境：溺水、挤压伤害、跌倒、蓝条破碎、虫墙、雾视觉效果等可按配置启用或禁用
- 限制解除：木偶商人生成限制、椅子菜单限制、宝箱限制、仓库区域限制等
- 地图与天气：随时快速传送（含夜间与雷暴）、夜间椅子传送、锁定天气、锁定危险度、移除特定区域黑暗效果、天气强制设置（旋风/雷暴/雾/干旱/浓雾/瘟疫）
- 视觉相关：去除马赛克、不沾污渍、不浸湿、贴图替换、外部 Spine 立绘附件替换、敏感内容贴图开关、运行时刷新贴图
- 热键相关：支持组合键、多个备选热键与手柄输入
- 调试相关：debug 开关；控制页可从列表选择物品、技能或配方并给予/解锁

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

- `Alice In Cradle`：`ver030d`
- `BepInEx`：`v5.4.23.5`

## 贴图替换使用说明

### 步骤

1. 使用 `AssetStudioGUI.exe` 导出需要替换的贴图
2. 按需修改贴图，并保存为 PNG 格式文件，扩展名可为 `.png` 或 `.btep`
3. 进入目录：`BepInEx/plugins/BetterExperience/ReplaceTexture/`
4. 将替换贴图复制到该目录；敏感内容贴图可放入 `Sensitive/` 子目录；文件名（不含扩展名）需与原始资源名一致
5. 启动游戏，按 `F1` 打开内置配置界面，启用 `启用替换贴图`；如需加载敏感内容贴图，请确认 `启用敏感内容贴图` 已开启
6. 重启游戏，或在游戏运行中按 `Ctrl+T` 重新加载贴图，使替换生效

### 简要排查

- 替换后无变化：检查目录、文件名、文件格式和配置开关是否正确，并查看 `BepInEx/plugins/BetterExperience/logs/` 中的加载日志
- 同名贴图未加载：不同目录下的同名文件只会加载一个，重复名称会被跳过并写入日志
- 替换后肢体错乱：通常是版本更新导致素材布局不同，需要用当前游戏版本素材作为参照，在图像编辑器中调整遮罩或对齐后重新保存

## 外部立绘附件替换

此功能读取外部 Spine 导出文件，替换 Region、Mesh 和 Linked Mesh 附件。游戏原始骨骼、插槽、约束、皮肤状态及动画保持不变。可修改附件图片和网格形状。

### 文件布局与清单

使用目录 `BetterExperience/ReplaceTexture`：

```text
ReplaceTexture/
    stand_normal.png                 原有整张贴图替换，可选
    stand_normal.portrait.json       附件包入口
    stand_normal.attachments.json   Spine 导出的 JSON
    stand_normal.attachments.atlas  Spine 导出的单页图集
    stand_normal.attachments.png    图集图片
```

`stand_normal.portrait.json` 示例：

```json
{
    "formatVersion": 1,
    "id": "stand-normal-cloth-demo",
    "target": "stand_normal",
    "jsonKey": "stand_normal",
    "json": "stand_normal.attachments.json",
    "atlas": "stand_normal.attachments.atlas"
}
```

`target` 是游戏 `SvTexture.key`，`jsonKey` 是其使用的骨架 JSON 资源键；两者通常相同，但有 JSON 变体的 CG 必须填写实际键。`id` 区分大小写。入口必须以 `.portrait.json` 结尾，其他文件名可以自定义。

JSON 和 atlas 路径相对于清单；PNG 路径由 atlas 首行指定，相对于 atlas。所有路径必须留在 `ReplaceTexture` 内，不接受绝对路径、目录跳转到外部或符号链接/目录联接。原有 `Sensitive` 开关覆盖清单和全部依赖文件：普通目录的清单也不能在该开关关闭时引用 `Sensitive` 内容。

### 资源制作要求

1. 使用对应游戏版本的原始资源作为基础。目前仅接受 Spine **4.1** JSON。
2. 保持骨骼顺序、骨骼初始变换、插槽、约束及皮肤约束定义一致。外部动画不参与合并，运行时使用原版动画。
3. 通过原有 `skin / slot / attachment` 查找键提供同名附件；未提供的附件保留原版。不能新增皮肤、插槽或附件，也不能修改路径、碰撞、裁剪等功能附件。
4. 保留附件原图集区域名（包括 `EM`、`ND` 等效果命名）。导出单页 atlas，包含合并后所有附件引用的区域，不能只打包修改过的图片。
5. 导出 straight-alpha PNG，不使用 PMA 或附件 sequence。PNG 尺寸须与 atlas 声明一致，运行时还会校验设备纹理尺寸限制。旋转切片支持 0°、90°。
6. 没有 Deform 依赖的网格可以更改顶点数量、三角形和权重，但只能绑定原骨骼。存在 Deform 依赖时，必须保留 UV/顶点对应顺序、三角形、hull/edges 和骨骼影响及权重；允许修改对应顶点坐标。Linked Mesh 同时校验父网格、皮肤及变形继承关系。

## 许可证

`BetterExperience/更好的体验` 使用 `LGPL-3.0` 许可证，详见 `LICENSE.txt`。
