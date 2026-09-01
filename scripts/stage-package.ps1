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
    # 预发布标签（0.1.0-EXP）的基准版本取 - 之前的部分。
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

$fileCount = (Get-ChildItem -LiteralPath $stageDir -Recurse -File | Measure-Object).Count
Write-Host "[stage-package] Staged $fileCount files to $stageDir"

if ($CreateZip) {
    $zipPath = Join-Path (Split-Path -Parent $stageDir) "$modName-$BuildFlavor-v$VersionLabel-$CommitLabel.zip"
    if (Test-Path -LiteralPath $zipPath -PathType Leaf) { Remove-Item -LiteralPath $zipPath -Force }
    Compress-Archive -Path (Join-Path $stageDir '*') -DestinationPath $zipPath
    Write-Host "[stage-package] Created zip $zipPath"
}
