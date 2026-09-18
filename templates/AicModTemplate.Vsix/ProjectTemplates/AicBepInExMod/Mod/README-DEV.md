# 开发须知（模板创建后请先读这里）

本解决方案由「Alice in Cradle BepInEx Mod」模板创建，包含：

- **主工程**（BepInEx + Harmony，net472）：`$safeprojectname$`
- **测试工程**（xunit，net8.0）：`$safeprojectname$.Test`

## 第一次构建前：填充 ReferenceLibrary

两个工程的引用 DLL 都来自解决方案根的 `ReferenceLibrary\` 文件夹（不进版本库）。
在本工程目录运行：

```powershell
.\setup-references.ps1 -GameDir "你的游戏安装目录"
```

脚本会从游戏目录拷贝所需的 BepInEx / Unity / 游戏 / UnityModBase DLL。
要求：游戏已装 BepInEx与 UnityModBase（UMB）前置 Mod。

## 目录结构速览

| 目录 | 用途 |
| --- | --- |
| `PatchInfo.cs` | 插件 GUID / 版本 / 路径常量，**改名先改这里** |
| `BService.cs` | UMB 服务（配置/日志/控制）的初始化入口 |
| `BConfigManager\` | 持久化配置项，按功能拆 partial 文件 |
| `BControlManager\` | 会话内实时控制项，按功能拆 partial 文件 |
| `BLogSpace\` | 插件日志（独立文件 + BepInEx 同步） |
| `BPatchGUI\` | 屏幕提示 NoticeGUI 等通用 UI 组件 |
| `Patches\HPatches.cs` | 共享的游戏对象定位 / 反射访问工具 |
| `Patches\xxxPatch.cs` | 具体功能补丁，入口启动时自动扫描注册 |
| 测试工程 | 与主工程目录镜像，纯逻辑放 `*Logic.cs` 便于测试 |

## 新增一个功能补丁的推荐流程

1. `BConfigManager\ConfigManagerXxx.cs` 新建 partial，声明开关配置。
2. `Patches\XxxPatch.cs` 新建补丁类（可参考 `DisableDrowningPatch.cs`）。
3. 涉及纯逻辑时拆 `XxxLogic.cs`，测试写到测试工程同名目录。
4. 需要游戏内实时读写时在 `BControlManager\ControlManagerXxx.cs` 声明控制条目。

## 发布前检查

- `PatchInfo.cs`：把 GUID 前缀 `com.example` 改成你自己的（如 `com.你的名字.模组名`）。
- `PatchInfo.cs` + `Properties\AssemblyInfo.cs`：版本号保持一致。
- 构建产物在主工程 `bin\Debug\`，拷贝到游戏 `BepInEx\plugins\` 即可。
