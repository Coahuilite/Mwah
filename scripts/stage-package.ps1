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

# 只暂存运行时/玩家可见内容：About/、LoadFolders.xml、1.6/、1.7/。
# Source/、docs/、scripts/、构建缓存、pdb、PublishedFileId.txt 一律不进包。

function Resolve-NormalizedPath([string]$Path) { [System.IO.Path]::GetFullPath($Path) }

$root = Resolve-NormalizedPath $ProjectRoot
$stageDir = Resolve-NormalizedPath $StageDir
$aboutSource = Join-Path $root 'About'
$loadFoldersSource = Join-Path $root 'LoadFolders.xml'
$versionedSource = Join-Path $root '1.6'
$modName = 'Mwah'
$gameVersions = @('1.6', '1.7')
$distRoot = (Join-Path $root 'dist') + [IO.Path]::DirectorySeparatorChar
if (-not $stageDir.StartsWith($distRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'StageDir must be a generated package directory beneath the repository dist directory.'
}

foreach ($game in $gameVersions) {
    if (-not (Test-Path -LiteralPath (Join-Path $root "$game/Assemblies/$modName.dll") -PathType Leaf)) {
        throw "Missing RimWorld $game assembly. Build both desired flavors before staging."
    }
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
# Keep one editable content source. Each selected minor loads only its own staged copy/DLL.
foreach ($game in $gameVersions) {
    $destination = Join-Path $stageDir $game
    $null = New-Item -ItemType Directory -Path (Join-Path $destination 'Assemblies') -Force
    foreach ($item in Get-ChildItem -LiteralPath $versionedSource -Force | Where-Object Name -ne 'Assemblies') {
        Copy-Item -LiteralPath $item.FullName -Destination $destination -Recurse -Force
    }
    Copy-Item -LiteralPath (Join-Path $root "$game/Assemblies/$modName.dll") `
        -Destination (Join-Path $destination "Assemblies/$modName.dll") -Force
}

# 发布卫生：逐条剔除不可分发内容。
$publishedFileId = Join-Path $stageDir 'About\PublishedFileId.txt'
if (Test-Path -LiteralPath $publishedFileId) { Remove-Item -LiteralPath $publishedFileId -Force }
Get-ChildItem -LiteralPath $stageDir -Recurse -File -Filter *.pdb | Remove-Item -Force
Get-ChildItem -LiteralPath $stageDir -Recurse -File -Filter *.gitkeep | Remove-Item -Force
Get-ChildItem -LiteralPath $stageDir -Recurse -File -Filter 'codemap.md' | Remove-Item -Force

# Check the effective load inputs, not just whether the producer returned successfully.
[xml]$folders = Get-Content -Raw -LiteralPath (Join-Path $stageDir 'LoadFolders.xml')
[xml]$metadata = Get-Content -Raw -LiteralPath (Join-Path $stageDir 'About/About.xml')
$sourceContent = @(Get-ChildItem -LiteralPath $versionedSource -Recurse -File | Where-Object {
    $_.FullName -notlike (Join-Path $versionedSource 'Assemblies/*') -and
    $_.Extension -ne '.pdb' -and $_.Name -notin @('.gitkeep', 'codemap.md')
})
foreach ($game in $gameVersions) {
    if (@($metadata.ModMetaData.supportedVersions.li) -notcontains $game) { throw "Missing supported minor $game." }
    $selection = $folders.loadFolders.SelectSingleNode("v$game")
    if ($null -eq $selection) { throw "Missing load-folder selection for $game." }
    $activeDlls = @()
    foreach ($folder in $selection.SelectNodes('li')) {
        $activeRoot = if ($folder.InnerText -eq '/') { $stageDir } else { Join-Path $stageDir $folder.InnerText }
        $dllDirectory = Join-Path $activeRoot 'Assemblies'
        if (Test-Path -LiteralPath $dllDirectory) { $activeDlls += @(Get-ChildItem -LiteralPath $dllDirectory -Filter *.dll -File -Recurse) }
    }
    $expected = Join-Path $stageDir "$game/Assemblies/$modName.dll"
    if ($activeDlls.Count -ne 1 -or $activeDlls[0].FullName -ne $expected) { throw "Invalid assembly selection for $game." }
    $framework = if ($game -eq '1.6') { '.NETFramework,Version=v4.7.2' } else { '.NETCoreApp,Version=v10.0' }
    $assemblyText = [Text.Encoding]::ASCII.GetString([IO.File]::ReadAllBytes($expected))
    if (-not $assemblyText.Contains($framework)) { throw "Wrong target framework in the RimWorld $game assembly." }
    foreach ($file in $sourceContent) {
        $relative = [IO.Path]::GetRelativePath($versionedSource, $file.FullName)
        $copy = Join-Path (Join-Path $stageDir $game) $relative
        if (-not (Test-Path -LiteralPath $copy -PathType Leaf) -or (Get-Item -LiteralPath $copy).Length -ne $file.Length) {
            throw "Incomplete content for $game/$relative."
        }
    }
}
$dlls = @(Get-ChildItem -LiteralPath $stageDir -Recurse -File -Filter *.dll)
if ($dlls.Count -ne $gameVersions.Count -or @($dlls | Where-Object Name -ne "$modName.dll").Count -gt 0) {
    throw 'Package contains unexpected game, reference or duplicate assemblies.'
}

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
