param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# 一条命令出本地 dev 包：按 csproj 默认（Dev flavor）构建，再 stage + zip。
# 输出纪律（学 FerriteLib 的打包脚本）：成功路径只打一行、只用仓库相对路径；
# dotnet 的完整输出只在失败时回显 —— 打包是给 agent 与人反复跑的，日志越薄越可信。
$root = [System.IO.Path]::GetFullPath($ProjectRoot)
$projectFile = Join-Path $root 'Source\Mwah\Mwah.csproj'

$buildLog = & dotnet build $projectFile -c Release -nologo -v quiet 2>&1
if ($LASTEXITCODE -ne 0) {
    $buildLog | Out-Host
    throw 'Dev flavor build failed.'
}

& (Join-Path $PSScriptRoot 'pack-dev.ps1') -ProjectRoot $root
if ($LASTEXITCODE -ne 0) {
    throw 'pack-dev failed.'
}
