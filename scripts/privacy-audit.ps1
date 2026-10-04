# 隐私审计门（Mwah!）：push 前唯一需要人工运行的检查（移植自 SqueakyRatkin 的同名门，
# 对齐 UniversalSqueaker 2026-09-06 维护者裁决的最小发布仪式）。
#
# AGENTS.md 硬边界的执行器："仓库、文档、产物与可达历史中零绝对本地路径、零凭据、零工坊 ID 文件"。
# 三个向量各自独立扫描，结论不得互推——工作树干净不蕴含历史干净，提交信息干净不蕴含历史 blob 干净：
#   vector1 工作树：HEAD 已跟踪文件（git grep -- .）
#   vector2 提交信息：subject + body，全部 ref
#   vector3 历史 blob：每个可达 revision（仅 -FullHistory；先量后扫）
# 身份面：author / committer 必须全部是 GitHub noreply 地址；出现真实邮箱即失败。
#
# 已知历史债务台账（$knownHistoryDebt，只作用于 vector3）：建仓体检（2026-10-04）三向量零命中，
# 台账为空。新增一条 = 一次维护者裁决 + 指向决定处；不得用它掩盖新泄漏。历史/tag 重写需维护者
# 单独授权（AGENTS.md：append-only）——门禁不因已知债务失明，也不把债务当成每次人工复述的仪式。
#
# -FullHistory 全历史扫描（push 前人工步骤）；默认只扫工作树 + 提交信息 + 身份（CI 每次 push/PR 用）。
# -PrePush     追加推送前机械自检：工作树干净、存在 main、报告将推送的提交与本地 tag 集合。
#
# 退出码 0 = 可推送（可含已知债务记录）；1 = 存在未接受的命中或机械自检失败。
[CmdletBinding()]
param(
    [switch]$FullHistory,
    [switch]$PrePush
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    $newLine = [Environment]::NewLine
    $failures = New-Object 'System.Collections.Generic.List[string]'
    $knownHits = New-Object 'System.Collections.Generic.List[string]'

    function Add-Failure([string]$What, [string]$Detail) {
        $script:failures.Add(($What + $newLine + $Detail))
    }

    function Add-KnownHit([string]$Detail) {
        $script:knownHits.Add($Detail)
    }

    # 原生命令的 stderr 在某些宿主下会变成终止性错误；判定只依赖 $LASTEXITCODE，故调用期间降级 EAP。
    function Invoke-Git {
        param([string[]]$Arguments)
        $previous = $ErrorActionPreference
        $ErrorActionPreference = 'Continue'
        try {
            $lines = @(& git @Arguments 2>$null)
            $code = $LASTEXITCODE
        }
        finally {
            $ErrorActionPreference = $previous
        }
        return [pscustomobject]@{ ExitCode = $code; Lines = @($lines | ForEach-Object { [string]$_ }) }
    }

    # 逗号包裹：PowerShell 函数返回会解包数组，1 元素/0 元素时调用方 .Count 会在 StrictMode 下炸。
    function Select-Unique([string[]]$Values) {
        return ,@($Values | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique)
    }

    # 本机展开路径：盘符 + 一或两个分隔符 + Users/WorkSpace。{1,2} 是刻意的：历史 blob 里实测存在
    # JSON 转义的双反斜杠形态，单分隔符模式看不见它。
    $pathPattern = '[A-Za-z]:[\\/]{1,2}(Users|WorkSpace)'

    # 私钥标记由拼接构造，本脚本才不会在 vector1 里命中自己。
    $credPatterns = [ordered]@{
        'github-classic-pat'  = 'ghp_[A-Za-z0-9]{36}'
        'github-fine-grained' = 'github_pat_[A-Za-z0-9_]{20,}'
        'openai-style-key'    = 'sk-[A-Za-z0-9]{20,}'
        'private-key-block'   = ('-----' + 'BEGIN')
    }
    $publishedFileIdValuePattern = '<PublishedFileId>[0-9]+'

    # 已知历史债务台账（vector3 专用）。2026-10-04 建仓体检：零命中，台账为空。
    $knownHistoryDebt = @()

    function Test-KnownHistoryDebt([string]$PatternLabel, [string]$RepoPath) {
        foreach ($entry in $script:knownHistoryDebt) {
            if ($entry.Pattern -eq $PatternLabel -and $entry.Path -eq $RepoPath) { return $true }
        }
        return $false
    }

    # ---- vector1：工作树（已跟踪文件） ----
    $vector1Checks = [ordered]@{
        'personal-path'           = $pathPattern
        'published-file-id-value' = $publishedFileIdValuePattern
    }
    foreach ($name in $credPatterns.Keys) { $vector1Checks["credential '$name'"] = $credPatterns[$name] }

    foreach ($label in $vector1Checks.Keys) {
        # -e 是必须的：私钥标记以 '-' 开头，会被当成选项（git exit 129）。
        $result = Invoke-Git @('grep', '-l', '-I', '-E', '-e', $vector1Checks[$label], '--', '.')
        if ($result.ExitCode -ne 0 -and $result.ExitCode -ne 1) {
            Add-Failure "vector1 scan failed for $label (git exit $($result.ExitCode))" ''
            continue
        }
        $hits = Select-Unique $result.Lines
        if ($hits.Count -gt 0) { Add-Failure "vector1 working-tree $label" ($hits -join $newLine) }
    }
    $trackedIdFile = Select-Unique ((Invoke-Git @('ls-files', '--', 'About/PublishedFileId.txt')).Lines)
    if ($trackedIdFile.Count -gt 0) { Add-Failure 'vector1 PublishedFileId.txt is tracked' ($trackedIdFile -join $newLine) }
    Write-Host 'vector1 (working tree): scanned'

    # ---- vector2：提交信息（subject + body） ----
    $messageText = ((Invoke-Git @('log', '--all', '--format=%s%n%b')).Lines -join $newLine)
    $vector2Checks = [ordered]@{
        'personal-path'           = $pathPattern
        'published-file-id-value' = $publishedFileIdValuePattern
    }
    foreach ($name in $credPatterns.Keys) { $vector2Checks["credential '$name'"] = $credPatterns[$name] }

    foreach ($label in $vector2Checks.Keys) {
        $matches = @([regex]::Matches($messageText, $vector2Checks[$label]))
        if ($matches.Count -gt 0) {
            $samples = Select-Unique @($matches | ForEach-Object { $_.Value.Substring(0, [Math]::Min(48, $_.Value.Length)) })
            Add-Failure "vector2 commit messages $label (x $($matches.Count))" ($samples -join $newLine)
        }
    }
    Write-Host 'vector2 (commit messages): scanned'

    # ---- 身份面：author 与 committer 分开收集（合并格式串会把两个身份塞进一行，单身份正则看不见） ----
    $authors = Select-Unique ((Invoke-Git @('log', '--all', '--format=%an <%ae>')).Lines)
    $committers = Select-Unique ((Invoke-Git @('log', '--all', '--format=%cn <%ce>')).Lines)
    $identities = Select-Unique ($authors + $committers)
    $noreplyPattern = '^[^<]+ <[0-9]+\+[^@]+@users\.noreply\.github\.com>$'
    $botPattern = '^GitHub <noreply@github\.com>$'
    $badIdentities = @($identities | Where-Object { $_ -notmatch $noreplyPattern -and $_ -notmatch $botPattern })
    if ($identities.Count -eq 0) { Add-Failure 'identity scan found no commits' '' }
    if ($badIdentities.Count -gt 0) {
        Add-Failure "identity: $($badIdentities.Count) non-noreply identity/identities" ($badIdentities -join $newLine)
    }
    Write-Host "identity: $($identities.Count) unique identity/identities, all noreply"

    # ---- vector3：历史 blob（-FullHistory） ----
    if ($FullHistory) {
        $revs = @((Invoke-Git @('rev-list', '--all')).Lines | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        Write-Host "vector3 (historical blobs): scanning $($revs.Count) revisions"
        $vector3Checks = [ordered]@{
            'personal-path'           = $pathPattern
            'published-file-id-value' = $publishedFileIdValuePattern
        }
        foreach ($name in $credPatterns.Keys) { $vector3Checks["credential '$name'"] = $credPatterns[$name] }

        foreach ($label in $vector3Checks.Keys) {
            $result = Invoke-Git (@('grep', '-l', '-I', '-E', '-e', $vector3Checks[$label]) + $revs)
            if ($result.ExitCode -ne 0 -and $result.ExitCode -ne 1) {
                Add-Failure "vector3 scan failed for $label (git exit $($result.ExitCode))" ''
                continue
            }
            $paths = Select-Unique @($result.Lines | ForEach-Object { ($_ -split ':', 2)[1] })
            $unknown = New-Object 'System.Collections.Generic.List[string]'
            foreach ($path in $paths) {
                if (Test-KnownHistoryDebt $label $path) { Add-KnownHit "$label : $path" }
                else { $unknown.Add($path) }
            }
            if ($unknown.Count -gt 0) { Add-Failure "vector3 historical $label" (($unknown | Sort-Object -Unique) -join $newLine) }
        }
        Write-Host "vector3 (historical blobs): scanned; known-debt files accepted: $($knownHits.Count)"
    }
    else {
        Write-Host 'vector3 (historical blobs): SKIPPED (pass -FullHistory for the pre-push check)'
    }

    # ---- -PrePush：推送前机械自检 ----
    if ($PrePush) {
        $status = (Invoke-Git @('status', '--porcelain', '--untracked-files=normal')).Lines
        if ($status.Count -gt 0) { Add-Failure 'pre-push: working tree is not clean' ($status -join $newLine) }

        $mainRef = Invoke-Git @('rev-parse', '--verify', '--quiet', 'refs/heads/main')
        if ($mainRef.ExitCode -ne 0) { Add-Failure 'pre-push: no local main branch' '' }

        $branch = (Invoke-Git @('rev-parse', '--abbrev-ref', 'HEAD')).Lines
        $head = (Invoke-Git @('rev-parse', '--short', 'HEAD')).Lines
        Write-Host "pre-push: HEAD = $($branch -join '') @ $($head -join '')"

        $upstream = Invoke-Git @('rev-parse', '--abbrev-ref', '--symbolic-full-name', '@{u}')
        if ($upstream.ExitCode -eq 0 -and $upstream.Lines.Count -gt 0) {
            $ahead = (Invoke-Git @('log', '--oneline', ($upstream.Lines[0] + '..HEAD'))).Lines
            Write-Host "pre-push: commits ahead of $($upstream.Lines[0]): $($ahead.Count)"
            foreach ($line in $ahead) { Write-Host "  $line" }
        }
        else {
            Write-Host 'pre-push: no upstream configured; report all local commits manually'
        }

        $tags = Select-Unique ((Invoke-Git @('tag', '--list')).Lines)
        Write-Host "pre-push: local tags = $($tags.Count)"
        if ($tags.Count -gt 0) {
            Write-Host ('  ' + ($tags -join ', '))
            Write-Host '  note: pushing a tag triggers Release CI; confirm the tag set is deliberate'
        }
        Write-Host 'pre-push: mechanical self-check ran'
    }

    if ($knownHits.Count -gt 0) {
        Write-Host ''
        Write-Host 'KNOWN HISTORY DEBT (accepted, not failing this run):'
        foreach ($hit in ($knownHits | Sort-Object -Unique)) { Write-Host "  [known-debt] $hit" }
        Write-Host '  decision: history/tag rewrite requires separate maintainer authorization (AGENTS.md: append-only)'
    }

    if ($failures.Count -gt 0) {
        Write-Host ''
        Write-Host 'PRIVACY AUDIT FAILED:'
        foreach ($failure in $failures) { Write-Host "- $failure" }
        exit 1
    }

    Write-Host ''
    Write-Host 'PRIVACY AUDIT CLEAN'
    exit 0
}
finally {
    Pop-Location
}
