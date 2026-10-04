param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Steam 上传暂存：构建 Steam flavor → stage 到 dist/steam/Mwah（不 zip——SteamCMD/工坊工具
# 上传的是目录）。与 pack-dev/pack-github 同一条"打包必构建"纪律。
# **要求干净工作树**：Steam 是最终发布渠道，从未提交的改动打出的包会伪报 provenance
# （commit=HEAD 但内容不是 HEAD）。工坊 item ID 只写本地上传副本的 About/PublishedFileId.txt，
# 它被 gitignore 钉死，永远不进仓库、不进本暂存目录之外的任何地方。

function Resolve-NormalizedPath([string]$Path) { [System.IO.Path]::GetFullPath($Path) }

$root = Resolve-NormalizedPath $ProjectRoot
$modName = 'Mwah'
$projectFile = Join-Path $root "Source\$modName\$modName.csproj"
$stageDir = Join-Path $root "dist\steam\$modName"

$statusOutput = @(& git -C $root status --porcelain --untracked-files=normal 2>$null)
if ($LASTEXITCODE -ne 0) {
    throw 'Failed to determine working-tree cleanliness.'
}
if ($statusOutput.Count -gt 0) {
    throw 'Steam packaging requires a clean working tree - commit first, Steam is the final release step.'
}

$shortCommit = @(& git -C $root rev-parse --short HEAD 2>$null)
if ($LASTEXITCODE -ne 0 -or $shortCommit.Count -ne 1) {
    throw 'Failed to determine the current Git commit for package labeling.'
}

[xml]$projectXml = Get-Content -LiteralPath $projectFile -Raw
$version = $projectXml.SelectSingleNode('/Project/PropertyGroup/Version').InnerText.Trim()

$buildLog = & dotnet build $projectFile -c Release -p:MWAHBuildFlavor=Steam -nologo -v quiet 2>&1
if ($LASTEXITCODE -ne 0) {
    $buildLog | Out-Host
    throw 'Steam flavor build failed; refusing to package.'
}

& (Join-Path $PSScriptRoot 'stage-package.ps1') -ProjectRoot $root -StageDir $stageDir `
    -VersionLabel $version -BuildFlavor steam -CommitLabel ([string]$shortCommit[0]).Trim()

$fileCount = (Get-ChildItem -LiteralPath $stageDir -Recurse -File | Measure-Object).Count
Write-Host "[pack-steam] $([System.IO.Path]::GetRelativePath($root, $stageDir))  ($fileCount files, flavor=steam version=$version commit=$([string]$shortCommit[0].Trim()))"
