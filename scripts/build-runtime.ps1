param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
    [ValidateSet('Dev', 'Steam', 'GitHub')][string]$BuildFlavor = 'Dev',
    [ValidateSet('1.6', '1.7')][string[]]$GameVersions = @('1.6', '1.7')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$project = Join-Path ([IO.Path]::GetFullPath($ProjectRoot)) 'Source/Mwah/Mwah.csproj'
foreach ($game in $GameVersions) {
    $buildLog = & dotnet build $project -c Release -nologo -v quiet `
        "-p:MWAHGameVersion=$game" "-p:MWAHBuildFlavor=$BuildFlavor" 2>&1
    if ($LASTEXITCODE -ne 0) {
        $buildLog | Out-Host
        throw "RimWorld $game $BuildFlavor build failed; refusing stale output."
    }
    Write-Host "[build-runtime] RimWorld $game ${BuildFlavor}: zero warnings/errors"
}
