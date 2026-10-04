param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
    [Parameter(Mandatory = $true)][string]$StageDir,
    [string]$VersionLabel,
    [string]$BuildFlavor = 'unknown',
    [string]$CommitLabel = 'unknown',
    [switch]$CreateZip
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# 只暂存运行时/玩家可见内容：About/、LoadFolders.xml、1.6/。
# Source/、docs/、scripts/、构建缓存、pdb、PublishedFileId.txt 一律不进包。

function Resolve-NormalizedPath([string]$Path) { [System.IO.Path]::GetFullPath($Path) }

$root = Resolve-NormalizedPath $ProjectRoot
$stageDir = Resolve-NormalizedPath $StageDir
$aboutSource = Join-Path $root 'About'
$loadFoldersSource = Join-Path $root 'LoadFolders.xml'
$versionedSource = Join-Path $root '1.6'
$assemblyPath = Join-Path $versionedSource 'Assemblies\Mwah.dll'
$modName = 'Mwah'

if (-not (Test-Path -LiteralPath $assemblyPath -PathType Leaf)) {
    throw "Missing built assembly: 1.6/Assemblies/$modName.dll. Build the desired flavor before staging."
}

# 版本纪律：包标签里的产品版本必须与 About.xml <modVersion> 一致，先校验后暂存，避免留下半截包。
if (-not [string]::IsNullOrWhiteSpace($VersionLabel)) {
    [xml]$aboutXml = Get-Content -LiteralPath (Join-Path $aboutSource 'About.xml') -Raw
    $modVersionNode = $aboutXml.SelectSingleNode('/ModMetaData/modVersion')
    if ($null -eq $modVersionNode -or [string]::IsNullOrWhiteSpace($modVersionNode.InnerText)) {
        throw "About.xml is missing <modVersion>; product version source must stay in sync."
    }
    # 标签里若带预发布尾缀（如 0.1.0-beta），基准版本取 - 之前的部分。
    $labelBase = ($VersionLabel -replace '-.*$', '')
    if ($modVersionNode.InnerText.Trim() -ne $labelBase) {
        throw "About.xml <modVersion> ($($modVersionNode.InnerText.Trim())) does not match package base version ($labelBase) from label ($VersionLabel)."
    }
}

if (Test-Path -LiteralPath $stageDir) { Remove-Item -LiteralPath $stageDir -Recurse -Force }
$null = New-Item -ItemType Directory -Path $stageDir -Force

Copy-Item -LiteralPath $aboutSource -Destination (Join-Path $stageDir 'About') -Recurse -Force
Copy-Item -LiteralPath $loadFoldersSource -Destination (Join-Path $stageDir 'LoadFolders.xml') -Force
Copy-Item -LiteralPath $versionedSource -Destination (Join-Path $stageDir '1.6') -Recurse -Force

# 发布卫生：逐条剔除不可分发内容。
$publishedFileId = Join-Path $stageDir 'About\PublishedFileId.txt'
if (Test-Path -LiteralPath $publishedFileId) { Remove-Item -LiteralPath $publishedFileId -Force }
Get-ChildItem -LiteralPath $stageDir -Recurse -File -Filter *.pdb | Remove-Item -Force
Get-ChildItem -LiteralPath $stageDir -Recurse -File -Filter *.gitkeep | Remove-Item -Force
Get-ChildItem -LiteralPath $stageDir -Recurse -File -Filter 'codemap.md' | Remove-Item -Force

# 包身份标签：不拆 DLL 也能确认包的新旧；写在所有剔除步骤之后，避免被误删。
if (-not [string]::IsNullOrWhiteSpace($VersionLabel)) {
    $labelContent = "$modName $VersionLabel`r`nbuild=$BuildFlavor`r`ncommit=$CommitLabel`r`n"
    [System.IO.File]::WriteAllText((Join-Path $stageDir 'version.txt'), $labelContent)
}

# zip 只为点名要它的调用方存在（pack-dev -Zip，目前唯一用户是 CI 的 artifact 上传）。
# 两个坑照抄 UniversalSqueaker 的评审教训：
#   S2 —— 从 stage 目录的 *内容* 打 zip（Compress-Archive 'stage\*'）解压到 Mods/ 会撒出
#         散件的 LoadFolders.xml；正确形态是根在唯一顶层目录 Mwah/ 下，解压即合法模组目录。
#   S4 —— Compress-Archive 拿暂存过程刚重写过的文件 mtime 给条目盖章，同一 commit 两次
#         打包哈希不同，产物不可比对；故手写归档：条目排序、名字归一 `/`、目录条目入档、
#         全部盖 commit author date（含最后补的根条目——不盖章的条目默认"现在"，正是漂移源）。
$zipName = ''
if ($CreateZip) {
    $commitDate = [DateTimeOffset]::Parse((& git -C $root log -1 --format=%aI)).ToUniversalTime()
    $zipPath = Join-Path (Split-Path -Parent $stageDir) "$modName-$BuildFlavor-v$VersionLabel-$CommitLabel.zip"
    if (Test-Path -LiteralPath $zipPath -PathType Leaf) { Remove-Item -LiteralPath $zipPath -Force }
    $top = Split-Path -Leaf $stageDir
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::Open($zipPath, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        $entries = @(Get-ChildItem -LiteralPath $stageDir -Recurse -Force | ForEach-Object {
            $rel = $_.FullName.Substring($stageDir.Length + 1).Replace('\', '/')
            if ($_.PSIsContainer) { "$rel/" } else { $rel }
        } | Sort-Object)
        foreach ($rel in $entries) {
            $entry = $archive.CreateEntry("$top/$rel", [System.IO.Compression.CompressionLevel]::Optimal)
            $entry.LastWriteTime = $commitDate
            if ($rel.EndsWith('/')) { continue }
            $in = [System.IO.File]::OpenRead((Join-Path $stageDir ($rel.Replace('/', '\'))))
            $out = $entry.Open()
            try { $in.CopyTo($out) } finally { $out.Dispose(); $in.Dispose() }
        }
        $rootEntry = $archive.CreateEntry("$top/", [System.IO.Compression.CompressionLevel]::NoCompression)
        $rootEntry.LastWriteTime = $commitDate
    } finally { $archive.Dispose() }
    $zipName = $zipPath.Substring($root.Length + 1)
}
$fileCount = (Get-ChildItem -LiteralPath $stageDir -Recurse -File | Measure-Object).Count
$relStage = $stageDir.Substring($root.Length + 1)
# 成功路径只打一行、只用仓库相对路径（输出纪律见 build-dev.ps1 头注释）。
$produced = if ($zipName) { "$zipName ($relStage)" } else { $relStage }
Write-Host "[stage-package] $produced  ($fileCount files, build=$BuildFlavor commit=$CommitLabel)"
