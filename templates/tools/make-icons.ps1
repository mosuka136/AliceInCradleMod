# 生成模板图标资源（VSIX 清单图标 + 模板 zip 内的 __TemplateIcon.ico/__PreviewImage.png）。
# 重新设计图标时改本脚本再运行即可。
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$resourcesDir = Join-Path $root 'AicModTemplate.Vsix\Resources'
$templateDir = Join-Path $root 'AicModTemplate.Vsix\ProjectTemplates\AicBepInExMod'

New-Item -ItemType Directory -Force -Path $resourcesDir | Out-Null

function New-TemplateBitmap([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

    # 深紫底 + 圆角矩形
    $g.Clear([System.Drawing.Color]::Transparent)
    $bgBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.Rectangle(0, 0, $size, $size)),
        [System.Drawing.Color]::FromArgb(255, 74, 44, 125),
        [System.Drawing.Color]::FromArgb(255, 38, 24, 69),
        ([System.Drawing.Drawing2D.LinearGradientMode]::Vertical))
    $r = [int]($size * 0.18)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc(0, 0, $r * 2, $r * 2, 180, 90)
    $path.AddArc($size - $r * 2, 0, $r * 2, $r * 2, 270, 90)
    $path.AddArc($size - $r * 2, $size - $r * 2, $r * 2, $r * 2, 0, 90)
    $path.AddArc(0, $size - $r * 2, $r * 2, $r * 2, 90, 90)
    $path.CloseFigure()
    $g.FillPath($bgBrush, $path)

    # 文字
    $fontSize = [int]($size * 0.34)
    $font = New-Object System.Drawing.Font('Segoe UI', $fontSize, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 240, 234, 255))
    $fmt = New-Object System.Drawing.StringFormat
    $fmt.Alignment = [System.Drawing.StringAlignment]::Center
    $fmt.LineAlignment = [System.Drawing.StringAlignment]::Center
    $g.DrawString('AiC', $font, $textBrush, (New-Object System.Drawing.RectangleF(0, 0, $size, $size)), $fmt)

    $g.Dispose()
    return $bmp
}

function Save-Png([System.Drawing.Bitmap]$bmp, [string]$path) {
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Host "PNG -> $path"
}

# ICO：单帧 PNG 压缩格式（Vista+，VS 可用）
function Save-PngIco([System.Drawing.Bitmap]$bmp, [string]$path) {
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $png = $ms.ToArray()

    $out = New-Object System.IO.MemoryStream
    $bw = New-Object System.IO.BinaryWriter($out)
    $bw.Write([uint16]0)      # reserved
    $bw.Write([uint16]1)      # type: icon
    $bw.Write([uint16]1)      # count
    $bw.Write([byte]0)        # width (0 = 256)
    $bw.Write([byte]0)        # height
    $bw.Write([byte]0)        # palette
    $bw.Write([byte]0)        # reserved
    $bw.Write([uint16]1)      # color planes
    $bw.Write([uint16]32)     # bits per pixel
    $bw.Write([uint32]$png.Length)
    $bw.Write([uint32]22)     # data offset (6 + 16)
    $bw.Write($png)
    [System.IO.File]::WriteAllBytes($path, $out.ToArray())
    $bw.Dispose()
    Write-Host "ICO -> $path"
}

$icon = New-TemplateBitmap 256
Save-PngIco $icon (Join-Path $resourcesDir 'TemplateIcon.ico')
$icon.Dispose()

$templateIcon = New-TemplateBitmap 256
Save-PngIco $templateIcon (Join-Path $templateDir '__TemplateIcon.ico')
$templateIcon.Dispose()

$previewBmp = New-Object System.Drawing.Bitmap(400, 300)
$g = [System.Drawing.Graphics]::FromImage($previewBmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::FromArgb(255, 30, 30, 40))
$titleFont = New-Object System.Drawing.Font('Segoe UI', 20, [System.Drawing.FontStyle]::Bold)
$lineFont = New-Object System.Drawing.Font('Segoe UI', 12)
$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$accent = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 177, 156, 255))
$g.DrawString('Alice in Cradle BepInEx Mod', $titleFont, $accent, 16, 16)
$lines = @(
    '+ $safeprojectname$           (BepInEx / net472)',
    '+ $safeprojectname$.Test      (xunit / net8.0)',
    '',
    'PatchInfo / ConfigManager / BLog',
    'ControlManager / NoticeGUI',
    'Patches + sample Harmony patch'
)
$y = 70
foreach ($line in $lines) {
    $g.DrawString($line, $lineFont, $brush, 20, $y)
    $y += 30
}
$g.Dispose()

Save-Png $previewBmp (Join-Path $resourcesDir 'TemplatePreview.png')
$previewBmp.Dispose()

$preview2 = New-Object System.Drawing.Bitmap(400, 300)
$g2 = [System.Drawing.Graphics]::FromImage($preview2)
$g2.DrawImage([System.Drawing.Image]::FromFile((Join-Path $resourcesDir 'TemplatePreview.png')), 0, 0)
$g2.Dispose()
Save-Png $preview2 (Join-Path $templateDir '__PreviewImage.png')
$preview2.Dispose()

Write-Host '图标资源生成完毕。'
