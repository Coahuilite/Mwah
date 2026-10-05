param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# 一条命令出本地 dev 包：构建已收进 pack-dev.ps1（打包必构建，2026-10-04 事故纪律），
# 本脚本退化为它的一层别名，保留旧入口不裸奔。
# 输出纪律（学 FerriteLib 的打包脚本）：成功路径只打一行、只用仓库相对路径。
$root = [System.IO.Path]::GetFullPath($ProjectRoot)

& (Join-Path $PSScriptRoot 'pack-dev.ps1') -ProjectRoot $root
if ($LASTEXITCODE -ne 0) {
    throw 'pack-dev failed.'
}
