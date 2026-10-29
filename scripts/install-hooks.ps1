# 一次性安装（每个 clone 一次）：把 git hooks 指到版本库里的 scripts/githooks。
# core.hooksPath 是本机 git 设置，不入库——新 clone 后先跑这个，pre-push 隐私门才会生效。
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)
git config core.hooksPath scripts/githooks
if ($LASTEXITCODE -ne 0) { throw 'git config core.hooksPath failed' }
$hook = Get-Content -Raw scripts/githooks/pre-push
if ($hook -notmatch "`r`n") { 'hook line endings: LF (ok)' } else { throw 'pre-push must stay LF (git runs it with sh)' }
"[install-hooks] core.hooksPath = scripts/githooks — pre-push privacy audit armed"
