param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
    # dev 预演是整目录拷进 Mods/ 的文件夹，不是压缩包：默认不产 zip。
    # -Zip 留给真需要归档产物的调用方（CI 的 artifact 上传）；stage-package 里的
    # 写手保证 zip 根在唯一顶层 Mwah/ 下且逐条目盖 commit 日期（解压即合法模组目录，
    # 同一 commit 两次打包哈希一致）。
    [switch]$Zip
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# dev 包标签 = csproj <Version> 原样 + 短 commit（工作树脏则加 -dirty）。
# 包身份只存一份：包内 version.txt（stage-package 写），不再往 dist 里撒同名 .txt。
$root = [System.IO.Path]::GetFullPath($ProjectRoot)
$modName = 'Mwah'
$stageDir = Join-Path $root "dist\dev\$modName"
$projectFile = Join-Path $root "Source\$modName\$modName.csproj"

# 打包必构建（2026-10-04 事故纪律）：本脚本曾只搬 1.6/Assemblies 里的现成 DLL，
# 把旧产物装进新标签的包（包名 864df31、DLL 里是 ae8d855，两个修复根本没进包）。
# dotnet 的完整输出只在失败时回显；成功路径依旧一行。
$buildLog = & dotnet build $projectFile -c Release -nologo -v quiet 2>&1
if ($LASTEXITCODE -ne 0) {
    $buildLog | Out-Host
    throw 'Release build failed; refusing to package a stale DLL.'
}

[xml]$projectXml = Get-Content -LiteralPath $projectFile -Raw
$versionNode = $projectXml.SelectSingleNode('/Project/PropertyGroup/Version')
if ($null -eq $versionNode -or [string]::IsNullOrWhiteSpace($versionNode.InnerText)) {
    throw "Missing <Version> in project file: $projectFile"
}
$version = $versionNode.InnerText.Trim()

$shortCommitOutput = @(& git -C $root rev-parse --short HEAD 2>$null)
if ($LASTEXITCODE -ne 0 -or $shortCommitOutput.Count -ne 1) {
    throw 'Failed to determine the current Git commit for dev package labeling.'
}
$shortCommit = ([string]$shortCommitOutput[0]).Trim()

$statusOutput = @(& git -C $root status --porcelain --untracked-files=normal 2>$null)
if ($LASTEXITCODE -ne 0) {
    throw 'Failed to determine whether the working tree is dirty for dev package labeling.'
}
$commitLabel = $shortCommit + $(if ($statusOutput.Count -gt 0) { '-dirty' } else { '' })

# 先清历史 zip 再暂存：不点名 -Zip 时不产 zip，也绝不让上一轮的 zip 冒充本轮产物。
Get-ChildItem -LiteralPath (Split-Path -Parent $stageDir) -File -Filter "$modName-dev-v*.zip" -ErrorAction SilentlyContinue |
    Remove-Item -Force
$stageArgs = @{
    ProjectRoot  = $root
    StageDir     = $stageDir
    VersionLabel = $version
    BuildFlavor  = 'dev'
    CommitLabel  = $commitLabel
}
if ($Zip) { $stageArgs['CreateZip'] = $true }
& (Join-Path $PSScriptRoot 'stage-package.ps1') @stageArgs

# 旧版脚本遗留在 dist/dev 的 .txt 标签文件，见到就清（一次性迁移，不报错）。
Get-ChildItem -LiteralPath (Split-Path -Parent $stageDir) -File -Filter "$modName-dev-v*.txt" -ErrorAction SilentlyContinue |
    Remove-Item -Force
