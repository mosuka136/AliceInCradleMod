# 自定义立绘替换说明

本文说明如何使用 BetterExperience 的 v2 资源替换系统替换游戏立绘。该系统支持只替换图片、atlas、部分 Spine 数据或整套自定义 Spine，也支持多个资源包按顺序组合。

## 支持范围

- Spine 4.1 JSON
- 单页 atlas
- straight-alpha PNG；不支持预乘 Alpha（PMA）
- 按 `key + jsonKey` 精确识别立绘目标
- 可替换 `bones`、`slots`、`constraints`、`skins`、`attachments`、`events`、`animations` 或 `all`
- 可映射原版动画、皮肤和骨骼名称
- 可覆盖立绘比例、偏移、宽高和左右站位偏移
- 支持 `auto`、`legacy`、`disabled` 三种污渍效果策略

不支持 Spine 二进制格式、多页 atlas、自定义情绪状态机、散装 PNG 和 v1 `.portrait.json`。

## 安装位置

首次运行 Mod 后使用以下目录：

```text
BepInEx/plugins/BetterExperience/ReplaceTexture/
```

每个资源包至少包含一个 `.replacement.json` 清单。建议将一个资源包的清单、PNG、atlas 和 Spine JSON 放在同一子目录中：

```text
ReplaceTexture/
└─ MyPortrait/
   ├─ my-portrait.replacement.json
   ├─ portrait.png
   ├─ portrait.atlas
   └─ portrait.json
```

敏感内容应完整放在 `ReplaceTexture/Sensitive/` 下。一个清单及其所有依赖不能跨越普通目录和 `Sensitive` 目录，也不能使用绝对路径、越界路径或目录链接。

## 启用资源包

1. 启动游戏并按 `F1` 打开 BetterExperience 配置界面。
2. 开启 `EnableResourceReplacement`。
3. 在 `EnabledReplacementPacks` 中启用清单的 `id`。
4. 如果资源包位于 `Sensitive`，同时开启 `EnableSensitivities`。
5. 按刷新贴图热键重新扫描；默认热键为 `Ctrl+T`。

`EnabledReplacementPacks` 的顺序参与合成：列表越靠后优先级越高。同一目标可以同时启用多个包，不会自动互斥。

每轮扫描都会同步这个列表：新发现的包按发现顺序追加（默认关闭），清单文件已从 `ReplaceTexture` 删除的包对应的行会被自动清除。只要清单文件还在磁盘上，行就会连同开关状态保留，包括关闭 `EnableSensitivities` 时看不到的敏感包、解析失败的清单和 `id` 重复的清单。如果某个清单连 `id` 都读不出来（JSON 损坏、缺少 `id`、文件无法读取），本轮不会删除任何未知行，避免误删。

## 完整清单示例

```json
{
    "formatVersion": 2,
    "id": "my-custom-portrait",
    "targets": [
        {
            "type": "spine",
            "key": "stand_normal",
            "jsonKey": "stand_normal",
            "image": "portrait.png",
            "atlas": "portrait.atlas",
            "spine": {
                "json": "portrait.json",
                "replace": ["all"]
            },
            "compatibility": {
                "animations": {
                    "stand": "idle"
                },
                "skins": {
                    "default": "base"
                },
                "bones": {
                    "face": "head",
                    "follow_hip": "pelvis"
                },
                "animationFallback": "idle",
                "skinFallback": "base"
            },
            "display": {
                "skeletonScale": 0.015625,
                "scaleMultiplier": 1.0,
                "offsetX": 0,
                "offsetY": 0,
                "width": 1024,
                "height": 1024,
                "rightShift": 0
            },
            "effects": {
                "dirt": "auto"
            }
        }
    ]
}
```

`id` 是资源包标识，必须在所有已扫描清单中唯一。一个清单可以包含多个 `targets`，但不能重复声明同一个 `key + jsonKey`。

`key` 标识游戏中的 `SvTexture`，`jsonKey` 标识该纹理使用的 Spine JSON 变体。两者都采用精确匹配；同一个 `key` 下的不同 `jsonKey` 是彼此独立的目标。

## 常见替换方式

### 只替换图片

```json
{
    "type": "spine",
    "key": "stand_normal",
    "jsonKey": "stand_normal",
    "image": "portrait.png"
}
```

系统继续使用原版 atlas 和 Spine 数据。新 PNG 的宽高必须与原版 atlas 页一致，所有原版区域也必须仍位于图片范围内。适合不改变排布、区域名和骨架的重绘。

### 同时替换图片和 atlas

```json
{
    "type": "spine",
    "key": "stand_normal",
    "jsonKey": "stand_normal",
    "image": "portrait.png",
    "atlas": "portrait.atlas"
}
```

适合图片尺寸、部件位置或区域排布发生变化，但仍复用原版 Spine 数据的情况。atlas 必须只有一页，页尺寸必须与 PNG 一致，Spine 使用到的全部区域必须存在。

### 只替换部分 Spine 数据

```json
{
    "type": "spine",
    "key": "stand_normal",
    "jsonKey": "stand_normal",
    "spine": {
        "json": "portrait.json",
        "replace": ["bones", "slots", "constraints"]
    }
}
```

`replace` 可使用：

| 值 | 行为 |
| --- | --- |
| `bones` | 替换骨骼定义 |
| `slots` | 替换插槽定义 |
| `constraints` | 同时替换 IK、Transform、Path 和 Physics 约束 |
| `skins` | 替换整个 skins 段 |
| `attachments` | 按 `skin/slot/attachment` 合并附件 |
| `events` | 替换事件定义 |
| `animations` | 替换动画段 |
| `all` | 替换以上全部数据 |

`all` 不能与其他值同时使用。同一目标层不能同时声明 `skins` 和 `attachments`：前者是整段替换，后者是按附件键合并。

### 只替换附件

```json
{
    "type": "spine",
    "key": "stand_normal",
    "jsonKey": "stand_normal",
    "image": "portrait.png",
    "atlas": "portrait.atlas",
    "spine": {
        "json": "portrait.json",
        "replace": ["attachments"]
    }
}
```

多个资源包可以继续向同一个目标追加或覆盖附件。系统会记录每个导入附件的来源骨骼表，并按最终骨骼名称重写加权顶点索引。

### 完全自定义 Spine

使用 `replace: ["all"]`，并通常同时提供 PNG 和 atlas。完全自定义 Spine 不要求沿用原版动画、皮肤和骨骼名称，但必须在 `compatibility` 中保证游戏使用的原版名称能够解析。

## 兼容映射

```json
"compatibility": {
    "animations": {
        "stand": "idle",
        "walk": "move"
    },
    "skins": {
        "default": "base"
    },
    "bones": {
        "face": "head",
        "follow_hip": "pelvis"
    },
    "animationFallback": "idle",
    "skinFallback": "base"
}
```

- `animations`：原版动画名到自定义动画名的映射。
- `skins`：原版皮肤名到自定义皮肤名的映射。
- `bones`：游戏查询的原版骨骼名到自定义骨骼名的映射；用于脸部、粒子、马赛克和脚本锚点等入口。
- `animationFallback`：没有显式映射且不存在同名动画时使用的动画。
- `skinFallback`：没有显式映射且不存在同名皮肤时使用的皮肤。

动画和皮肤的解析顺序是：显式映射、同名资源、fallback。原版所有动画和皮肤名称都必须能够解析，否则整套候选会被拒绝。生成的别名会保留游戏原有的动画切换、附加轨道和皮肤合并逻辑。

## 显示参数

所有字段均可省略；未提供时继续使用游戏原始值或 CSV 配置。

| 字段 | 说明 |
| --- | --- |
| `skeletonScale` | 覆盖 `SkeletonDataAsset.scale`，必须大于 0 |
| `scaleMultiplier` | 在游戏立绘比例上追加倍率，必须大于 0 |
| `offsetX` | 水平偏移，单位为游戏像素 |
| `offsetY` | 垂直偏移，单位为游戏像素 |
| `width` | 覆盖立绘逻辑宽度，必须大于 0 |
| `height` | 覆盖立绘逻辑高度，必须大于 0 |
| `rightShift` | 左右站位时的横向偏移基准 |

最终 Spine 含 clipping attachment 时，系统会自动启用 Spine clipping。

## 污渍效果

```json
"effects": {
    "dirt": "auto"
}
```

- `auto`：检测 atlas 中兼容的 `EM/ND` 区域；存在时沿用游戏污渍效果，不存在时关闭并记录日志。
- `legacy`：强制使用原有效果；缺少兼容区域时拒绝该候选。
- `disabled`：关闭该立绘的污渍绘制。

只替换图片且继续使用原版 atlas 时，默认仍可使用原版效果区域。

## 分层组合

每个 Spine 候选始终从原版资源开始构建，然后按照 `EnabledReplacementPacks` 从上到下应用：

1. 后层提供的图片、atlas 或完整段覆盖前层。
2. `attachments` 按 `skin/slot/attachment` 逐键累积，后层同键覆盖前层。
3. 兼容映射、显示参数和效果设置按字段由后层覆盖。
4. 不同 `jsonKey` 分别构建，不会互相覆盖。

因此可以将图片、骨架、动画或局部附件拆成独立资源包，按需启用和排列。

## 验证、刷新与回退

候选切换前会验证：

- Spine 版本和 JSON 结构
- atlas 页数、Alpha 模式、页尺寸和区域边界
- 骨骼、插槽、附件和约束引用
- Linked Mesh、Deform 和加权顶点骨骼索引
- 动画与皮肤兼容别名
- 游戏 Spine `SkeletonJson` 能否解析最终结果

每个目标独立构建并原子切换：

- 已启用资源包临时损坏时，保留该目标上一次可用的组合。
- 首次加载失败时继续使用原版。
- 禁用或删除资源包时重新组合剩余层；没有剩余层时恢复原版。
- 关闭 `EnableSensitivities` 后，`Sensitive` 中的资源不会被继续保留。
- 连续修改文件后按 `Ctrl+T` 刷新，无需重启游戏。

失败原因会写入 BepInEx 日志。排错时先检查目标的 `key + jsonKey`、PNG/atlas 尺寸、atlas 区域名，以及兼容映射是否覆盖所有原版动画和皮肤。

