param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# 一条命令出本地 dev 包：先按 csproj 默认（Dev flavor）构建，再 stage + zip。
$root = [System.IO.Path]::GetFullPath($ProjectRoot)
$projectFile = Join-Path $root 'Source\EveryPawnKissEachOther\EveryPawnKissEachOther.csproj'

& dotnet build $projectFile -c Release -nologo
if ($LASTEXITCODE -ne 0) {
    throw "Dev flavor build failed."
}

& (Join-Path $PSScriptRoot 'pack-dev.ps1') -ProjectRoot $root
if ($LASTEXITCODE -ne 0) {
    throw "pack-dev failed."
}
