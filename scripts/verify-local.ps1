param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
    [switch]$PackDev
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# 本模组的本地验证：无测试工程，因此检查顺序为
#   1   Release 构建（零警告零错误由 dotnet 自身把关）
#   1b  三个构建渠道都要能编译（Dev / Steam / GitHub）
#   2   全部 XML 良构
#   3   Keyed 中英键集合一致
#   4   C# 引用的 MWAH.* 键在两种语言里都存在
#   5   defName(XML) ↔ DefOf 字段(C#) ↔ driverClass 字符串
#   6   DLL 符号审计 + 零 Harmony 断言
#   7   版本纪律：csproj <Version> == About.xml <modVersion>
#   8   分发卫生与隐私红线：无 PublishedFileId、无绝对本地路径
#   9   设置项三处锁死：字段名 ↔ Scribe key ↔ Constants 默认值
#  10   门禁档位 ↔ 双语档位名键 ↔ Constants 档位范围
# 全部通过后 -PackDev 才出 dev 包。

$root = [System.IO.Path]::GetFullPath($ProjectRoot)

function Get-KeySet([string]$path) {
    [xml]$doc = Get-Content -Raw -LiteralPath $path
    # 只取元素节点：XML 注释在 DOM 里名为 #comment，会把键计数灌水。
    @($doc.DocumentElement.ChildNodes | Where-Object { $_.Name -notlike '#*' } | ForEach-Object { $_.Name }) | Sort-Object
}
$modName = 'Mwah'
$projectFile = Join-Path $root "Source\$modName\$modName.csproj"
$assemblyPath = Join-Path $root "1.6\Assemblies\$modName.dll"
$failures = @()

function Invoke-Check {
    param([string]$Name, [scriptblock]$Action)
    & $Action
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[FAIL] $Name"
        $script:failures += $Name
    } else {
        Write-Host "[ok] $Name"
    }
}

function Assert-True([string]$Name, [bool]$Condition, [string]$Detail = '') {
    if ($Condition) { Write-Host "[ok] $Name" }
    else {
        Write-Host "[FAIL] $Name $Detail"
        $script:failures += $Name
    }
}

Invoke-Check "Release build ($modName.csproj)" {
    & dotnet build $projectFile -nologo -c Release -p:DebugType=none -p:DebugSymbols=false | Out-Host
}

# 1b. 构建渠道纪律。MWAH_DEV 决定 dev 打点类是否存在，而它只随 MWAHBuildFlavor 变；
#     真实踩过的坑：JobDriver 里一行没包 #if 的 KissTrace.Clear() 让 Steam/GitHub 渠道
#     根本编译不过，而默认（Dev）构建一路绿。凡是 release 要发的包都必须各自过编译。
foreach ($flavor in @('Steam', 'GitHub')) {
    $probeOut = Join-Path ([System.IO.Path]::GetTempPath()) "mwah-flavor-$flavor"
    Invoke-Check "flavor build compiles ($flavor)" {
        & dotnet build $projectFile -nologo -c Release -p:DebugType=none -p:DebugSymbols=false `
            -p:MWAHBuildFlavor=$flavor -o $probeOut | Out-Host
    }
}

Assert-True 'built assembly present' (Test-Path -LiteralPath $assemblyPath -PathType Leaf)

# 2. XML well-formedness
$xmlFiles = Get-ChildItem -LiteralPath $root -Recurse -Filter *.xml |
    Where-Object { $_.FullName -notmatch '\\(obj|bin|dist|\.git)\\' }
$bad = @()
foreach ($f in $xmlFiles) {
    try { $null = [xml](Get-Content -Raw -LiteralPath $f.FullName) }
    catch { $bad += "$($f.Name): $($_.Exception.Message)" }
}
Assert-True ("all XML well-formed (" + @($xmlFiles).Count + " files)") ($bad.Count -eq 0) ($bad -join ' | ')

# 3/4. Localization
$enPath = Join-Path $root '1.6\Languages\English\Keyed\MWAH_Strings.xml'
$zhPath = Join-Path $root '1.6\Languages\ChineseSimplified\Keyed\MWAH_Strings.xml'
$en = Get-KeySet $enPath
$zh = Get-KeySet $zhPath
Assert-True "Keyed parity English/ChineseSimplified ($($en.Count)/$($zh.Count))" ((@(Compare-Object $en $zh)).Count -eq 0)

$code = @(Get-ChildItem (Join-Path $root "Source\$modName") -Recurse -Filter *.cs | Get-Content -Raw) -join "`n"

# 门禁档位名键是拼出来的（"MWAH.Settings.Scope." + scope），字面量扫描看不见，
# 所以先从枚举展开真实键名，再让正、反两个方向都用展开后的集合。
$scopeCs = Get-Content -Raw -LiteralPath (Join-Path $root "Source\$modName\Actions\Kiss\KissScope.cs")
$scopeRungs = @()
if ($scopeCs -match '(?s)public enum KissScope\s*\r?\n\{(.*?)\r?\n\}') {
    $scopeRungs = @([regex]::Matches($matches[1], '(?m)^\s{4}(\w+)\s*=\s*(\d+)') | ForEach-Object { ,@($_.Groups[1].Value, [int]$_.Groups[2].Value) })
}
$scopeKeys = @($scopeRungs | ForEach-Object { "MWAH.Settings.Scope." + $_[0] })
$literalKeys = @([regex]::Matches($code, '"(MWAH\.[A-Za-z0-9_.]+)"') | ForEach-Object { $_.Groups[1].Value })
# 以点结尾的匹配是拼接前缀，不是键名。
$used = @((($literalKeys + $scopeKeys) | Where-Object { $_ -notmatch '\.$' } | Sort-Object -Unique))
$missing = @($used | Where-Object { $en -notcontains $_ -or $zh -notcontains $_ })
Assert-True ("all $($used.Count) C#-referenced keys exist in both languages") ($missing.Count -eq 0) ($missing -join ', ')
# 反向信息项：定义了却没人用的键（不失败，只提示，防止语言文件攒尸体）
$unused = @($en | Where-Object { $used -notcontains $_ })
if ($unused.Count -gt 0) { Write-Host ("[info] defined but unreferenced keys: " + ($unused -join ', ')) }

# 5. Def identity cross-check
$defXml = @(Get-ChildItem (Join-Path $root '1.6\Defs') -Recurse -Filter *.xml | Get-Content -Raw) -join "`n"
$xmlDefs = @([regex]::Matches($defXml, '<defName>(MWAH_\w+)</defName>') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
$defOfFields = @([regex]::Matches((Get-ChildItem (Join-Path $root "Source\$modName\DefOf") -Recurse -Filter *.cs | Get-Content -Raw),
    'public static (?!class\b)\w+ (MWAH_\w+)') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
$orphanFields = @($defOfFields | Where-Object { $xmlDefs -notcontains $_ })
Assert-True ("DefOf fields all resolve to an XML defName ($($defOfFields.Count)/$($xmlDefs.Count))") ($orphanFields.Count -eq 0) ($orphanFields -join ', ')
$driverRefs = @([regex]::Matches($defXml, '<driverClass>([^<]+)</driverClass>') | ForEach-Object { $_.Groups[1].Value })
Assert-True 'driverClass uses the real namespace.type' (($driverRefs.Count -eq 1) -and $driverRefs[0] -eq "$modName.JobDriver_Kiss")

# 5b. DefInjected 结构检查：顶层元素必须是扁平键 `DefName.字段路径`（原版格式），
# 不是嵌套 def 格式。引擎 SetDefFieldAtPath 用 path.Split('.')[0] 当 defName，
# 写成 <MWAH_KissDirector><label>…</label></MWAH_KissDirector> 会让顶层名变成裸 defName、
# 找不到字段路径，静默丢进翻译报告的 "missing" 节——本项目踩过，Keyed 检查看不见。
$injBad = @()
foreach ($langDir in @('English', 'ChineseSimplified')) {
    $injRoot = Join-Path $root "1.6\Languages\$langDir\DefInjected"
    if (-not (Test-Path $injRoot)) { continue }
    foreach ($f in Get-ChildItem $injRoot -Recurse -Filter *.xml) {
        [xml]$x = Get-Content -Raw -LiteralPath $f.FullName
        foreach ($node in $x.LanguageData.ChildNodes) {
            if ($node.NodeType -ne 'Element') { continue }
            if ($node.Name -notmatch '\.') {
                $injBad += "$langDir/$($f.Name): '$($node.Name)' has no '.field' suffix (nested-def form; must be flat DefName.path)"
                continue
            }
            $defPart = $node.Name.Split('.')[0]
            if ($xmlDefs -notcontains $defPart) {
                $injBad += "$langDir/$($f.Name): '$($node.Name)' -> def '$defPart' is not a known defName"
            }
        }
    }
}
Assert-True 'DefInjected uses flat DefName.path keys' ($injBad.Count -eq 0) ($injBad -join ' | ')

# 6. DLL symbol audit + zero Harmony
$text = [System.Text.Encoding]::ASCII.GetString([System.IO.File]::ReadAllBytes($assemblyPath))
$symbols = @('FloatMenuOptionProvider_Kiss','JobDriver_Kiss','KissUtility','KissBoundary','KissScope','KissScopeUtility','KissAmbient',
    'KissDirector','Dialog_KissDirector','KissTicker','MainButtonWorker_KissDirector','KissMoodReward','KissCooldown','KissReturnQueue','MwahSettings','MwahMod','MWAH_JobDefOf','MWAH_ThoughtDefOf',"$modName.JobDriver_Kiss",'MWAH_Kiss','MWAH_KissedBond')
$missingSyms = @($symbols | Where-Object { -not $text.Contains($_) })
Assert-True ("DLL contains all $($symbols.Count) key symbols") ($missingSyms.Count -eq 0) ($missingSyms -join ', ')
Assert-True 'zero-Harmony: no Harmony/HarmonyLib reference in DLL' (-not ($text.Contains('HarmonyLib') -or $text.Contains('Harmony')))

# 7. Version discipline
[xml]$csproj = Get-Content -LiteralPath $projectFile -Raw
$csprojVersion = $csproj.SelectSingleNode('/Project/PropertyGroup/Version').InnerText.Trim()
[xml]$about = Get-Content -LiteralPath (Join-Path $root 'About\About.xml') -Raw
$modVersion = $about.SelectSingleNode('/ModMetaData/modVersion').InnerText.Trim()
$packageId = $about.SelectSingleNode('/ModMetaData/packageId').InnerText.Trim()
$constMatch = [regex]::Match($code, 'ModId = "([^"]+)"').Groups[1].Value
Assert-True "csproj <Version> == About <modVersion> ($csprojVersion)" ($csprojVersion -eq $modVersion)
Assert-True "Constants.ModId == About packageId ($packageId)" ($constMatch -eq $packageId)
Assert-True 'packageId is lowercase' ($packageId -eq $packageId.ToLowerInvariant())

# 8. Distribution hygiene + privacy red line
Assert-True 'no About/PublishedFileId.txt in repo' (-not (Test-Path -LiteralPath (Join-Path $root 'About\PublishedFileId.txt')))
$textFiles = Get-ChildItem -LiteralPath $root -Recurse -File -Include *.cs, *.xml, *.ps1, *.md, .gitignore, .gitattributes |
    Where-Object { $_.FullName -notmatch '\\(obj|bin|dist|\.git)\\' }
$privacyHits = @($textFiles | Select-String -Pattern '[A-Za-z]:\\' | Where-Object { $_.Line -notmatch '^\s*#' })
Assert-True 'no absolute local paths in tracked text files' ($privacyHits.Count -eq 0) (($privacyHits | Select-Object -First 3 | ForEach-Object { "$($_.Path):$($_.LineNumber)" }) -join ' | ')

# 9. Settings fields, Scribe keys and Constants defaults must name the same thing.
#    改设置项字段名时最容易只改一半：字段改了、Scribe key 没改，老存档的值就静默回到默认。
$settingsCs = Get-Content -Raw -LiteralPath (Join-Path $root "Source\$modName\MwahSettings.cs")
$constantsCs = Get-Content -Raw -LiteralPath (Join-Path $root "Source\$modName\Constants.cs")
$decls = @([regex]::Matches($settingsCs, 'public\s+(?:bool|int|float)\s+(\w+)\s*=\s*Constants\.(\w+)\s*;'))
$badSettings = @()
foreach ($d in $decls) {
    $field = $d.Groups[1].Value
    $const = $d.Groups[2].Value
    $look = 'Scribe_Values\.Look\(ref ' + $field + ', "' + $field + '", Constants\.' + $const + '\)'
    if ($settingsCs -notmatch $look) { $badSettings += "$field : Scribe key mismatch" }
    if ($constantsCs -notmatch ('\b' + $const + '\b')) { $badSettings += "$field : Constants.$const missing" }
}
Assert-True ("settings fields locked to Scribe keys and Constants ($($decls.Count) fields)") ($badSettings.Count -eq 0) ($badSettings -join ' | ')

# 9b. RestoreDefaults 必须覆盖全部设置字段：字段加进声明区却漏掉恢复按钮，是"恢复默认值"
#     对新项静默失效的头号路径。这里按方法体逐字段核对，缺一个就红。
$restoreBody = [regex]::Match($settingsCs, '(?s)public void RestoreDefaults\(\)\s*\{(.*?)\n    \}').Groups[1].Value
$missRestore = @($decls | Where-Object { $restoreBody -notmatch ('\b' + $_.Groups[1].Value + '\s*=\s*Constants\.') })
Assert-True ("RestoreDefaults covers all $($decls.Count) settings fields") ($missRestore.Count -eq 0) (($missRestore | ForEach-Object { $_.Groups[1].Value }) -join ', ')

# 10. 门禁每加一档就得同时有双语档位名，且范围与默认常量跟着改；漏一处就是滑条上出现裸键名。
#     枚举在门 3/4 之前已解析成 $scopeRungs，这里只做断言，不重复解析。
Assert-True ("kiss scope enum parsed ($($scopeRungs.Count) rungs)") ($scopeRungs.Count -ge 2)
$missingRungKeys = @($scopeRungs | Where-Object { $en -notcontains ("MWAH.Settings.Scope." + $_[0]) -or $zh -notcontains ("MWAH.Settings.Scope." + $_[0]) } | ForEach-Object { $_[0] })
Assert-True ("every kiss scope rung has a label in both languages ($($scopeRungs.Count) rungs)") ($missingRungKeys.Count -eq 0) ($missingRungKeys -join ', ')
$rungsSequential = $true
for ($k = 0; $k -lt $scopeRungs.Count; $k++) { if ($scopeRungs[$k][1] -ne $k) { $rungsSequential = $false } }
Assert-True 'scope rungs are numbered from 0 without gaps' $rungsSequential
$rangeLine = [regex]::Match($constantsCs, 'PairScopeRange = new\(\(int\)KissScope\.(\w+),\s*\(int\)KissScope\.(\w+)\)')
Assert-True 'scope range constants match the enum ends' ($rangeLine.Success -and $rangeLine.Groups[1].Value -eq $scopeRungs[0][0] -and $rangeLine.Groups[2].Value -eq $scopeRungs[$scopeRungs.Count - 1][0])
$defaultLine = [regex]::Match($constantsCs, 'PairScopeDefault = \(int\)KissScope\.(\w+)')
Assert-True ("factory default scope is the widest rung ($($defaultLine.Groups[1].Value))") ($defaultLine.Groups[1].Value -eq $scopeRungs[$scopeRungs.Count - 1][0])

if ($failures.Count -gt 0) {
    Write-Host ''
    Write-Host "[verify-local] FAIL: $($failures.Count) check(s) failed:"
    $failures | ForEach-Object { Write-Host "  - $_" }
    exit 1
}

Write-Host ''
Write-Host '[verify-local] all checks passed.'

if ($PackDev) {
    & (Join-Path $PSScriptRoot 'pack-dev.ps1') -ProjectRoot $root
    if ($LASTEXITCODE -ne 0) { throw 'pack-dev failed.' }
}
