# Spine 换装工具与 Agent 执行手册

适用于 Alice In Cradle ver030d、游戏自带 Spine 4.1.24 和当前 BetterExperience 外部附件加载器。工具直接复用项目的附件合并器，以原骨架和动画制作服装包。首期任务为 `stand_normal`、`stand_weak` 的哥特修女服。

## 安装与入口

需要 Windows、Python 3.12、可用的宿主 pip、.NET 8 或更新 SDK、.NET Framework 4.7.2 开发包，以及项目 `ReferenceLibrary` 中的游戏程序集。`setup.ps1` 安装锁定依赖到独立 `.venv`，构建离线运行库与可选游戏 QA 插件，输出均在本工具目录内。

```powershell
./tools/spine-wardrobe/setup.ps1
$wardrobePython = './tools/spine-wardrobe/.venv/Scripts/python.exe'
$wardrobeCli = './tools/spine-wardrobe/wardrobe.py'
$wardrobeJob = './tools/spine-wardrobe/job.example.json'
& $wardrobePython $wardrobeCli inspect --job $wardrobeJob
& $wardrobePython $wardrobeCli extract --job $wardrobeJob
& $wardrobePython $wardrobeCli prepare --job $wardrobeJob
```

复制 `job.example.json` 创建任务。`gameDir` 使用绝对路径，`outputDir` 相对于任务文件，参考图片使用绝对路径。资源 ID 仅使用 ASCII 字母、数字、连字符和下划线。修改服装描述、目标或参考图将使原设计确认失效。

不要直接构建项目的 Release 目录来安装本工具；当前游戏的 BetterExperience DLL 可能通过符号链接指向它。本工具不会替换游戏原始资源包、Spine DLL 或已有 Mod 源码。

## 工作目录与断点

| 目录/文件 | 内容 |
|---|---|
| `original/<target>` | 原 PNG、atlas、骨架 JSON、资源来源及哈希 |
| `prepared/<target>` | 还原的逻辑裁片、附件映射、变形依赖、原版预览 |
| `concept` | 设计图和实际用户确认记录 |
| `references`、`generated` | 生成用固定布局与生成结果 |
| `packages/<target>` | 四文件正式候选包 |
| `baseline-packages/<target>` | 原版往返包，仅测试，不允许发布 |
| `qa/<target>/candidate` | 动画帧、结构报告、预览、覆盖索引 |
| `game-qa`、`observations` | 实际游戏截图、材料状态、观察到的皮肤/动画组合 |
| `transactions/<id>` | 安装前字节备份、写入哈希和回滚状态 |
| `checkpoint.json`、`issues.json` | 阶段结果及最多三次自动修复记录 |

工作目录默认被 Git 忽略。所有命令失败都返回非零退出码并保存原因。`extract` 只在来源和输出哈希均一致时复用提取结果。已有素材可继续使用；候选变化后必须重新验证和审阅，不可复用旧验收结论。相同任务不要同时运行多个修改命令。

## Agent 制作流程

### 1. 建立原版基线

`inspect` 检查包头、运行库哈希与实际插件链接。`extract` 读取 UnityFS 的 Texture2D 和 TextAsset，不直接把 `.dat` 当作 JSON。`prepare` 按游戏 atlas 解析器给出的旋转、偏移和逻辑尺寸拆件，并进行 RGBA 无损往返检查。

`mapping.json` 的 region 使用 `path → name → attachment key` 查找顺序，列出所有 `skin/slot/attachment` 使用者以及 Linked Mesh 父链。`default` 皮肤可能只有脸部，不能据此判定身体丢失；实际姿势须叠加相应皮肤。骨骼组装预览使用游戏的实际运行库。

```powershell
& $wardrobePython $wardrobeCli assemble --baseline --job $wardrobeJob
& $wardrobePython $wardrobeCli validate --baseline --job $wardrobeJob
& $wardrobePython $wardrobeCli preview --baseline --job $wardrobeJob
```

### 2. 生成概念图并确认

使用 imagegen：原版组装图为人物和姿态参考，用户参考图仅为服装风格参考。首先检查原插槽绘制顺序与网格轮廓；不得承诺原骨架不能支持的披风或新关节。

展示概念图并等待用户明确确认。保存 `approval-evidence.json`：

```json
{
    "approvedBy": "user",
    "userMessage": "实际收到的设计确认原文",
    "designIdentity": "由 wardrobe.pipeline.design_identity(job) 计算",
    "conceptFiles": {"概念图绝对路径": "文件 SHA-256"},
    "allowedRegions": {
        "stand_normal": ["cloth", "bust", "hat"],
        "stand_weak": ["mainA", "mainA_arm2"]
    }
}
```

`allowedRegions` 应包括实际批准修改的所有区域，按任务的最终设计填写。不得伪造确认原文。`approve-design --evidence <文件> --job <任务>` 保存并校验确认记录。

### 3. 制作透明裁片

使用 `make-sheet --target stand_normal --regions cloth bust hat hairB --output <PNG> --job <任务>` 生成固定布局及同名 JSON 坐标表。调用 imagegen 前查看图片，将裁片表明确标记为编辑对象，概念图明确标记为风格参考。

提示词必须要求：原位置、尺寸、方向、连接点不变；只修改指定服装；脸和前发不变；输出真实透明背景。生成完成后保存原始输出及提示词，不把生成图当作已验收图集。

```powershell
& $wardrobePython $wardrobeCli ingest-sheet --job $wardrobeJob --image <生成PNG> --layout <坐标JSON> --output <裁片目录>
```

导入拒绝无 Alpha 的 RGB 图、比例变化、空裁片和已变化的源图。模型可能把棋盘格画在图里；这种结果必须修复，不能直接进入游戏。导入只解决坐标缩放，不能证明关节、衣领和袖口已经对齐。必须在组装后查看实际动画。映射中的画布参考点不是自动识别的解剖连接点，Agent 应根据原图标注实际接缝再制作。

若多次生成仍无真实透明通道，可要求 imagegen 输出纯 `#00FF00` 背景，再用 `--key-green` 显式提取透明度并去除绿色边缘。此选项不能处理棋盘格，也不适合含绿色服装的设计。若模型放大了裁片，可用 `--fit-cell` 将每个独立单元格的内容拟合回原部件范围；它要求坐标表含 `cellBox`，不能保证单元格内多个裁片的相对位置正确。导入记录会标记仍需视觉检查。

### 4. 组装和适度轮廓调整

`edits.json` 的目标集合必须与任务完全一致，裁片路径相对于该文件：

```json
{
    "stand_normal": {
        "parts": {"cloth": "parts/cloth.png"},
        "geometry": [{
            "attachment": "default/cloth/cloth",
            "slot": "cloth",
            "deltas": [{"vertex": 0, "dx": 0, "dy": -4}]
        }]
    },
    "stand_weak": {"parts": {"mainA": "parts/mainA.png"}, "geometry": []}
}
```

上述顶点仅展示格式，不能直接作为服装调整参数。位移单位为原 JSON 的 Spine 世界坐标，Y 向上，基于 setup pose。工具对每个影响骨骼分别计算逆线性变换，保留权重和拓扑。Linked Mesh 的形状应修改其允许修改的根网格。不要新增顶点、改 UV、动画或功能附件。

裁片必须保留原逻辑画布大小。现有裁边区域能容纳时保持原图集布局；新增透明覆盖超出原裁边范围时重打包完整图集，保留逻辑尺寸、名称及四周 padding。超出逻辑画布的造型需要通过网格轮廓调整，不能静默裁掉。新包使用 straight alpha，最多单页 8192 像素，实际设备限制还须游戏验证。

头饰与前发共用区域时，可在目标配置增加 `preserveMasks: {"hair": "masks/front-hair.png"}`。同尺寸灰度图的白色像素恢复原资源，黑色像素采用新素材；遮罩文件参与制作记录哈希。备用皮肤如果没有可承载头饰的附件，应保留原前发并明确记录设计限制。

```powershell
& $wardrobePython $wardrobeCli assemble --job $wardrobeJob --edits <edits.json>
& $wardrobePython $wardrobeCli validate --job $wardrobeJob
& $wardrobePython $wardrobeCli preview --job $wardrobeJob
```

校验器保留完整附件集，逐皮肤、逐动画采样 9 帧（`--samples` 可设置 2–121），比较原始骨骼运动、附件选择、三角形翻转和拉伸。超过原边长 2.5 倍的变化判定失败。原版已存在的退化三角形不一概误报。剪裁导致的拓扑差异列入人工视觉审阅项。

离线预览实现基础颜色、插槽混色和 clipping，不模拟游戏污渍、石化或材料特效。GIF、帧图和索引用于视觉检查；帧抽样不等于连续时间的数学证明。

### 5. 视觉审阅和游戏 QA

AI 实际查看原版/候选对比帧和动画后，填写 `visual-review.json` 的候选证据文件，再运行 `record-review --evidence <文件>`。字段为 `reviewer: "agent"`、`targets`；每个目标包含 `passed: true`、`package`（候选四文件哈希）、`images`（实际查看的图片哈希）和 `checks`（具体检查结论）。未查看的图不能标记已查看；未解决的接缝、黑边或遮挡不得写为通过。

游戏 QA 插件编译产物位于 `GameQa/bin/Debug/Wardrobe.QA.dll`。将它放到游戏 `BepInEx/plugins/WardrobeQA`，在同目录建立 `bridge.json`，内容为 `{"queueRoot":"本任务 game-qa 目录的绝对路径"}`，重启游戏加载。删除该可选插件及 bridge 即可停用。目录外写入须遵守当前工作环境的文件权限。

QA 1.0.2 等待 Nel 游戏资源就绪后才创建人物；标题界面已有 Spine 材质不代表污渍 shader 已加载。请求有效期为一小时，过期或没有时间戳的旧请求不会在启动时重放，需要重新运行 `qa-request`。测试人物和采集相机跨场景保留，停止模块时释放；源人物与采集相机使用不同图层，避免相互叠入截图。

通过离线和视觉检查后，`qa-stage` 临时安装候选，返回回滚事务 ID；它不是最终发布。重新载入配置并刷新，在下一次安全立绘切换后观察包生效。

为定位只有实际 shader 才能揭示的视觉问题，可明确使用 `qa-stage --diagnostic` 和 `qa-request --diagnostic --isolated`。诊断模式仍要求设计已确认、候选通过结构和动画几何检查，只允许临时安装；`deploy` 的完整视觉及游戏验收门槛保持不变。每次临时安装保存当时的配置，回滚时不得覆盖用户后来修改的文件。

`qa-request --qa-operation observe|render|refresh` 写入任务文件，`qa-collect` 收集实际结果：

- `observe`：捕获真实显示中的立绘截图、实际皮肤/动画组合、活动包 ID、shader 和资源数量。未出现的状态不计为完成。
- `render`：在已加载的游戏立绘数据与材料上创建独立 SkeletonAnimation/Camera，遍历皮肤和动画输出截图；不改变玩家当前动作。此结果不能替代实际 UI 遮挡和特效测试。
- `refresh`：提出十次附件目录刷新请求并观察随后的实际安全切换；请求次数不能当作完成十次资源替换的证明。

`--isolated` 通过游戏的 `SpineViewerNel` 和已安装加载器创建独立对象，使用独立污渍管理器。刷新会对这些对象调用真实安全切换。独立对象的组合不会冒充实际 UI 观察到的组合。独立 `observe` / `refresh` 可仅产生资源与状态记录，采集结果标为 `metadata-only`；没有截图的 `render` 仍会被拒绝。

运行 `qa-collect` 后再执行 `validate`，会加入实际观察到的组合。切换配置、真实污渍、清洗、冻结/石化、附件显隐、冲突与损坏资源测试，由 Agent 使用游戏界面或已验证的调试入口触发；不得仅写标签伪造状态。原始组合中若含未知皮肤/动画，先核查采集记录。

最终 `game-qa/acceptance.json` 必须有 `passed: true`、`evidenceFiles` 哈希表及每个目标的 `package` 和 `checks`。所需检查项为 `animations`、`skins`、`observedCombinations`、`effects`、`refresh10`、`disableRestore`、`switch`、`conflict`、`corruptionFallback`、`resourceLifetime`。只在对应事实和截图/日志证据充分时填 `true`。资源数量需在同场景预热后比较，不能以一次增量推断泄漏。

### 6. 安装、恢复与失败诊断

`deploy` 仅接受当前设计、当前候选及完整游戏验收，备份原配置和同名文件，保持其他目标的开关不变。`rollback --transaction <ID>` 恢复原字节；发现用户在安装后改过文件时拒绝覆盖。安装不强制重置当前动画，生效必须通过后续安全切换和画面验证。

若候选已通过 `qa-stage` 安装，最终使用 `deploy --transaction <原 QA 事务 ID>` 提升该事务为已验收，保留安装前的原始备份。采集器拒绝候选哈希已变化、活动服装 ID 不符或带错误的游戏结果。

每次自动修复前运行 `retry-issue --issue <稳定ID> --stage image|geometry|occlusion|protocol --description <原因>`。同一问题最多三轮；达到上限保留诊断和当前候选，报告阻塞原因，不把失败包作为成品。新设计修订须重新获得用户确认。

## 测试

```powershell
dotnet test tools/spine-wardrobe/Runtime.Tests/Runtime.Tests.csproj
./tools/spine-wardrobe/.venv/Scripts/python.exe -m unittest discover -s tools/spine-wardrobe/tests -v
dotnet test BetterExperience.Test/BetterExperience.Test.csproj --no-restore
```

新增测试覆盖加权坐标转换、非默认皮肤、契约锁定、旋转与裁边往返、Alpha 扩容、安装异常回滚、并发配置修改保护和审批/重试门槛。文本源文件采用 UTF-8、空格缩进和 CRLF。
