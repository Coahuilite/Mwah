param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# GitHub 发布包：SemVer tag 驱动，构建 GitHub flavor → stage → zip。
# 与 pack-dev 同一条纪律：**打包必构建**（2026-10-04 事故：只 stage 旧 DLL 会把上一渠道的
# 产物装进新标签的包），flavor 由 -p:MWAHBuildFlavor 指定，构建失败拒包。
# 用法：pwsh scripts/pack-github.ps1 -Version v0.1.0（tag 原样传入，v 前缀保留在包名里）

function Resolve-NormalizedPath([string]$Path) { [System.IO.Path]::GetFullPath($Path) }

$root = Resolve-NormalizedPath $ProjectRoot
$modName = 'Mwah'
$projectFile = Join-Path $root "Source\$modName\$modName.csproj"
$stageDir = Join-Path $root "dist\github\$modName"
$zipDir = Join-Path $root 'dist\github'

$semVerTagPattern = '^v(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-(?:(?:0|[1-9]\d*)|[0-9A-Za-z-]*[A-Za-z-][0-9A-Za-z-]*)(?:\.(?:(?:0|[1-9]\d*)|[0-9A-Za-z-]*[A-Za-z-][0-9A-Za-z-]*))*)?$'
if ($Version -notmatch $semVerTagPattern) {
    throw "Version must be a strict SemVer 2.0 tag: vMAJOR.MINOR.PATCH with an optional prerelease suffix: $Version"
}

$shortCommit = @(& git -C $root rev-parse --short HEAD 2>$null)
if ($LASTEXITCODE -ne 0 -or $shortCommit.Count -ne 1) {
    throw 'Failed to determine the current Git commit for package labeling.'
}

$buildLog = & dotnet build $projectFile -c Release -p:MWAHBuildFlavor=GitHub -nologo -v quiet 2>&1
if ($LASTEXITCODE -ne 0) {
    $buildLog | Out-Host
    throw 'GitHub flavor build failed; refusing to package.'
}

& (Join-Path $PSScriptRoot 'stage-package.ps1') -ProjectRoot $root -StageDir $stageDir `
    -VersionLabel $Version.TrimStart('v') -BuildFlavor github -CommitLabel ([string]$shortCommit[0]).Trim()

$zipPath = Join-Path $zipDir "$modName-$Version.zip"
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}
Compress-Archive -Path $stageDir -DestinationPath $zipPath -Force
Write-Host "[pack-github] $([System.IO.Path]::GetRelativePath($root, $zipPath))  (flavor=github version=$Version)"
