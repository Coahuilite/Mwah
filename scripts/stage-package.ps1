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

# 只暂存运行时/玩家可见内容：About/、LoadFolders.xml、Common/、Compatibility/ 与两版原生 DLL。
# Source/、docs/、scripts/、构建缓存、pdb、PublishedFileId.txt 一律不进包。
# 共同内容各拷一份，两 minor 只带自己的 Mwah.dll；条件兼容根随包发行，是否生效由 Loader 判断。

function Resolve-NormalizedPath([string]$Path) { [System.IO.Path]::GetFullPath($Path) }

$root = Resolve-NormalizedPath $ProjectRoot
$stageDir = Resolve-NormalizedPath $StageDir
$aboutSource = Join-Path $root 'About'
$loadFoldersSource = Join-Path $root 'LoadFolders.xml'
$modName = 'Mwah'
$gameVersions = @('1.6', '1.7')
# 唯一共同内容源；顺序与 LoadFolders 声明一致（Common 先于条件兼容根）。
$contentRoots = @('Common', 'Compatibility/VanillaNutrientPasteExpanded')
$distRoot = (Join-Path $root 'dist') + [IO.Path]::DirectorySeparatorChar
if (-not $stageDir.StartsWith($distRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'StageDir must be a generated package directory beneath the repository dist directory.'
}
foreach ($relative in $contentRoots) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $relative) -PathType Container)) {
        throw "Missing shared content root $relative."
    }
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
# 唯一共同内容源＝唯一分发副本：Common 与条件兼容根各拷一次，两 minor 只放各自的原生 DLL。
foreach ($relative in $contentRoots) {
    $destination = Join-Path $stageDir $relative
    $null = New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force
    Copy-Item -LiteralPath (Join-Path $root $relative) -Destination $destination -Recurse -Force
}
foreach ($game in $gameVersions) {
    $destination = Join-Path $stageDir $game
    $null = New-Item -ItemType Directory -Path (Join-Path $destination 'Assemblies') -Force
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
# “包里存在”≠“本组合生效”：条件 li 只在模拟启用 VNPE 的组合里进入有效输入。
[xml]$folders = Get-Content -Raw -LiteralPath (Join-Path $stageDir 'LoadFolders.xml')
[xml]$metadata = Get-Content -Raw -LiteralPath (Join-Path $stageDir 'About/About.xml')
$vnpePackageId = 'VanillaExpanded.VNutrientE'
$contentExclusions = @('.gitkeep', 'codemap.md')

function Get-EffectiveRoot([string]$Relative) {
    if ($Relative -eq '/') { return $stageDir }
    return (Join-Path $stageDir ($Relative.Replace('/', [IO.Path]::DirectorySeparatorChar)))
}

# Loader 在一个声明根里只读 Defs／Languages／Assemblies，其余目录不进入有效内容。
function Get-EffectiveFiles([string]$Relative) {
    $rootPath = Get-EffectiveRoot $Relative
    $files = @()
    foreach ($kind in @('Defs', 'Languages', 'Assemblies')) {
        $directory = Join-Path $rootPath $kind
        if (Test-Path -LiteralPath $directory -PathType Container) {
            $files += @(Get-ChildItem -LiteralPath $directory -Recurse -File | ForEach-Object {
                [IO.Path]::GetRelativePath($rootPath, $_.FullName).Replace('\', '/')
            })
        }
    }
    @($files | Sort-Object)
}

# 条件只认本项目采用的 IfModActive 写法；其他条件属性一律拒绝，避免被当无条件加载。
function Split-LoadFolders($Nodes) {
    $unconditional = @()
    $conditional = @()
    foreach ($node in $Nodes) {
        $attributes = @($node.Attributes | Where-Object { $_.Name -ne '#text' })
        foreach ($attribute in $attributes) {
            if ($attribute.Name -ne 'IfModActive') {
                throw "Unsupported load-folder condition '$($attribute.Name)' on $($node.InnerText)."
            }
            if ($attribute.Value -ne $vnpePackageId) {
                throw "Unexpected IfModActive value '$($attribute.Value)' on $($node.InnerText)."
            }
        }
        if (@($attributes).Count -gt 0) { $conditional += $node.InnerText.Trim() } else { $unconditional += $node.InnerText.Trim() }
    }
    [pscustomobject]@{ Unconditional = $unconditional; Conditional = $conditional }
}

foreach ($relative in $contentRoots) {
    if (@(Get-ChildItem -LiteralPath (Join-Path $stageDir $relative) -Recurse -Directory -Filter Assemblies).Count -gt 0) {
        throw "Shared content root $relative must never carry assemblies."
    }
    # 每个发行根都要自带 Defs 与两语言 Keyed：漏一种语言就是玩家可见文字缺半边（AGENTS 硬边界）。
    $stagedRoot = Join-Path $stageDir $relative
    if (@(Get-ChildItem -LiteralPath (Join-Path $stagedRoot 'Defs') -Recurse -File -Filter *.xml -ErrorAction SilentlyContinue).Count -eq 0) {
        throw "Content root $relative ships no Defs."
    }
    foreach ($language in @('English', 'ChineseSimplified')) {
        $keyed = Join-Path $stagedRoot "Languages/$language/Keyed"
        if (@(Get-ChildItem -LiteralPath $keyed -File -Filter *.xml -ErrorAction SilentlyContinue).Count -eq 0) {
            throw "Content root $relative ships no $language Keyed copy."
        }
    }
}
foreach ($game in $gameVersions) {
    if (@($metadata.ModMetaData.supportedVersions.li) -notcontains $game) { throw "Missing supported minor $game." }
    $selection = $folders.loadFolders.SelectSingleNode("v$game")
    if ($null -eq $selection) { throw "Missing load-folder selection for $game." }
    $folderChoices = Split-LoadFolders $selection.SelectNodes('li')
    $base = @($folderChoices.Unconditional)
    $optional = @($folderChoices.Conditional)

    # 声明根存在：基础组合与 VNPE 组合用到的每个根都必须在包里，条件只决定是否生效。
    foreach ($declared in @($base + $optional)) {
        if (-not (Test-Path -LiteralPath (Get-EffectiveRoot $declared) -PathType Container)) {
            throw "Declared load root $declared is missing from the package for $game."
        }
    }
    # 显式布局：根→Common→当前 minor，兼容根只能是条件项。
    if ($base.Count -ne 3 -or $base[0] -ne '/' -or $base[1] -ne 'Common' -or $base[2] -ne $game) {
        throw "Load folders for $game must be root, Common, then $game; actual: $($base -join ', ')."
    }
    foreach ($declared in $optional) {
        if ($declared -notin $contentRoots) { throw "Conditional load root $declared is not a staged content root." }
    }

    # 每个组合都只能选到当前 minor 的一份 Mwah.dll；VNPE 组合不得再多出任何程序集。
    $expected = Join-Path $stageDir "$game/Assemblies/$modName.dll"
    $combinations = [ordered]@{
        base = $base
        vnpe = @($base + $optional)
    }
    foreach ($name in $combinations.Keys) {
        $activeDlls = @()
        foreach ($declared in $combinations[$name]) {
            $dllDirectory = Join-Path (Get-EffectiveRoot $declared) 'Assemblies'
            if (Test-Path -LiteralPath $dllDirectory -PathType Container) {
                $activeDlls += @(Get-ChildItem -LiteralPath $dllDirectory -Filter *.dll -File -Recurse)
            }
        }
        if ($activeDlls.Count -ne 1 -or $activeDlls[0].FullName -ne $expected) {
            throw "Invalid assembly selection for $game in the $name combination ($($activeDlls.Count) dll(s))."
        }
    }
    $framework = if ($game -eq '1.6') { '.NETFramework,Version=v4.7.2' } else { '.NETCoreApp,Version=v10.0' }
    $assemblyText = [Text.Encoding]::ASCII.GetString([IO.File]::ReadAllBytes($expected))
    if (-not $assemblyText.Contains($framework)) { throw "Wrong target framework in the RimWorld $game assembly." }

    # minor 只允许原生程序集：旧 Def／语言副本必须不存在（源树与包同为条件，漏一份就是隐式覆盖层）。
    foreach ($minorCopy in @(Get-ChildItem -LiteralPath (Join-Path $stageDir $game) -Directory | Where-Object Name -ne 'Assemblies')) {
        throw "Minor $game must ship only assemblies; found staged $($minorCopy.Name)."
    }
    foreach ($stale in @("$game/Defs", "$game/Languages")) {
        if (Test-Path -LiteralPath (Join-Path $root $stale) -PathType Container) {
            throw "Obsolete content source $stale still exists; Common is the only shared content root."
        }
    }
}

# 源文件清单：共同与兼容内容各一份，逐文件长度一致；两组合之间不得出现同相对路径遮盖。
foreach ($relative in $contentRoots) {
    $source = Join-Path $root $relative
    $sourceFiles = @(Get-ChildItem -LiteralPath $source -Recurse -File | Where-Object {
        $_.Extension -ne '.pdb' -and $_.Name -notin $contentExclusions
    })
    if ($sourceFiles.Count -eq 0) { throw "Content root $relative ships no content." }
    foreach ($file in $sourceFiles) {
        $copyRelative = [IO.Path]::GetRelativePath($source, $file.FullName)
        $copy = Join-Path (Join-Path $stageDir $relative) $copyRelative
        if (-not (Test-Path -LiteralPath $copy -PathType Leaf) -or (Get-Item -LiteralPath $copy).Length -ne $file.Length) {
            throw "Incomplete content for $relative/$($copyRelative.Replace('\', '/'))."
        }
    }
}
foreach ($game in $gameVersions) {
    $selection = $folders.loadFolders.SelectSingleNode("v$game")
    $parsed = Split-LoadFolders $selection.SelectNodes('li')
    $declaredRoots = @(@($parsed.Unconditional) + @($parsed.Conditional))
    $seen = @{}
    foreach ($declared in $declaredRoots) {
        foreach ($file in Get-EffectiveFiles $declared) {
            if ($file -like 'Assemblies/*') { continue }
            if ($seen.ContainsKey($file)) {
                throw "Effective content path $file is declared by both $($seen[$file]) and $declared for $game."
            }
            $seen[$file] = $declared
        }
    }
    foreach ($required in @(Get-EffectiveFiles 'Common')) {
        if ($required -like 'Assemblies/*') { continue }
        if (-not $seen.ContainsKey($required)) { throw "Common content $required is not in the $game load selection." }
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
