# Alice in Cradle BepInEx Mod — VS 项目模板（VSIX）

把本仓库的 Mod 开发框架打包成 Visual Studio 项目模板：安装 VSIX 后，
在「新建项目」里一次生成「主 Mod 工程（BepInEx / net472）+ xunit 测试工程（net8.0）」的解决方案骨架，
包含入口、PatchInfo、配置管理、日志、GUI 通知、示例补丁与配套测试。

## 目录结构

```
templates/
├── AicModTemplate.Vsix/                 # VSIX 扩展工程
│   ├── AicModTemplate.Vsix.csproj       # 用 VS 的 MSBuild 构建（见下）
│   ├── source.extension.vsixmanifest    # 安装目标：VS 2022 与 VS 2026
│   ├── zip-template.ps1                 # 构建时自动打包模板 zip（VSSDK 校验对多工程模板不适用）
│   ├── Resources/                       # VSIX 图标（tools/make-icons.ps1 生成）
│   └── ProjectTemplates/AicBepInExMod/  # 多工程模板源（根 vstemplate + Mod/ + Mod.Test/）
└── tools/
    ├── make-icons.ps1                   # 重新生成图标资源
    ├── test-template.ps1                # 离线自检：模拟实例化→编译→测试，不需要 VS UI
    └── test-template-vs.ps1             # 检查 VS 实际生成的源码、工程引用与编译/测试
```

## 构建 VSIX

要求：VS 2022+ 且安装了「Visual Studio 扩展开发」工作负载（提供 VsSDK 构建目标）。
在仓库根目录执行（用 VS 自带的 MSBuild，不要用 dotnet build）：

```powershell
& "D:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" `
    templates\AicModTemplate.Vsix\AicModTemplate.Vsix.csproj -t:Rebuild -p:Configuration=Release -nr:false
```

产物：`templates\AicModTemplate.Vsix\bin\Release\net472\AicModTemplate.Vsix.vsix`

修改模板内容后重新构建即可；构建过程会自动重新打包模板 zip（zip-template.ps1）。

## 安装

双击 `.vsix`，或：

```powershell
& "D:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\VSIXInstaller.exe" <vsix 路径>
```

- 安装到正在运行的 VS 实例需要先关闭该实例。
- 多实例机器可加 `/instanceIds:<实例ID>` 定向安装（实例 ID 用 vswhere 查询）。
- 本模板同时支持 VS 2022（17.x）与 VS 2026（18.x）。

## 使用模板新建 Mod

1. VS「新建项目」→ 搜索 **Alice in Cradle** → 选「Alice in Cradle BepInEx Mod」。
2. 输入项目名（如 `MyFirstMod`），生成解决方案：`MyFirstMod` + `MyFirstMod.Test`。
3. 在主工程目录运行 `.\setup-references.ps1 -GameDir "<游戏安装目录>"` 填充 `ReferenceLibrary\`。
   （要求游戏已装 BepInEx 与 UnityModBase（UMB）前置 Mod）
4. 构建主工程，把 `bin\Debug\<项目名>.dll` 拷到游戏 `BepInEx\plugins\`。
5. 详细开发指引见模板自带的 `README-DEV.md`（随主工程落地）。

发布前记得把 `PatchInfo.cs` 里的 GUID 前缀 `com.example` 改成自己的。

## 自检脚本

- **离线自检（推荐，改动模板后必跑）**：`templates\tools\test-template.ps1`
  按 `.vstemplate` 清单模拟文件生成和父、子模板参数替换，检查无占位符残留及引用路径，
  然后用本机 `ReferenceLibrary` 编译主工程并跑测试工程的全部测试。
- 加 `-ValidateOnly` 可仅检查实例化，无需引用 DLL；加 `-Keep` 保留临时产物，默认成功后清理。`-MsBuild` 可指定构建工具路径。
- VS 实际创建验证：`templates\tools\test-template-vs.ps1 -DteProgId VisualStudio.DTE.18.0 -TemplatePath "<根 vstemplate 路径>"`；逐一检查主工程与测试工程的全部 C# 文件，避免空工程构建成功。`-ValidateOnly` 可只检查生成结果。

## 已知设计要点（改模板前先读）

- 两个 SDK 子模板均显式设置 `<CreateInPlace>true</CreateInPlace>`，直接在最终工程目录创建文件，避免复制项目时遗漏隐式 `Compile` 源码。
- 主工程是 SDK 风格 csproj（net472 + `GenerateAssemblyInfo=false`，程序集信息仍由 `Properties\AssemblyInfo.cs` 提供）。
- 根模板的两个 `ProjectTemplateLink` 均启用 `CopyParameters="true"`。
  子模板通过 `$ext_safeprojectname$` 获取用户输入的根名；子工程自身的参数按各自工程名替换。
- 测试工程使用 `$ext_safeprojectname$.Test` 生成文件名、程序集名和命名空间，
  主工程引用直接生成为 `..\$ext_safeprojectname$\$ext_safeprojectname$.csproj`。
  自检按 `.vstemplate` 清单复制文件并分别替换父、子参数，不再手动重写生成的引用。
- 根 vstemplate 用 `ProjectTemplateLink ProjectName="$safeprojectname$"` 和 `$safeprojectname$.Test` 命名两个子工程；
  子模板标记了 `<Hidden>true</Hidden>`，不会单独出现在新建项目对话框。
- VSIX 工程把 VsSDK targets 放在 `Sdk.targets` 之后导入（手动 Import Sdk.props/Sdk.targets），
  否则 VsSDK 的 `ZipIntermediatePath` 等属性会在求值期拿到空值。
