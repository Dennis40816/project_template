<#
.SYNOPSIS
  The single verification entry point, shared by local work, CI and release.

.PARAMETER Scope
  pr   : unit + architecture + golden tests (what every pull request runs).
  full : everything in pr, plus headless UI and snapshot tests (release candidates).

.PARAMETER Ci
  Restore in locked mode so an unreviewed dependency change fails the build.
#>
[CmdletBinding()]
param(
    [ValidateSet('pr', 'full')]
    [string]$Scope = 'pr',
    [switch]$Ci
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

function Invoke-Step([string]$Name, [scriptblock]$Action) {
    Write-Host "==> $Name" -ForegroundColor Cyan
    $global:LASTEXITCODE = 0
    & $Action
    if ($LASTEXITCODE -ne 0) { throw "$Name failed (exit $LASTEXITCODE)." }
}

$solution = (Get-ChildItem -Path $root -Filter '*.slnx' | Select-Object -First 1).FullName

Invoke-Step 'VERSION format' {
    $version = (Get-Content -LiteralPath (Join-Path $root 'VERSION') -Raw).Trim()
    if ($version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+$') { throw "VERSION must be X.Y.Z, found '$version'." }
}

Invoke-Step 'Agent skill projection is current' { & (Join-Path $PSScriptRoot 'sync-agent-skills.ps1') -Check }

if ($Ci) {
    Invoke-Step 'Restore (locked)' { dotnet restore $solution --locked-mode }
} else {
    Invoke-Step 'Restore' { dotnet restore $solution }
}

Invoke-Step 'Build' { dotnet build $solution --configuration Release --no-restore }

# Test projects are selected by scope; UI tests (slow, pixel snapshots) run only in full scope.
# Microsoft Testing Platform (global.json) fails a project that runs zero tests, so a test
# framework change that silently stops discovering tests breaks the build instead of passing.
$projects = Get-ChildItem -Path (Join-Path $root 'tests') -Filter '*.Tests.csproj' -Recurse |
    Where-Object { $Scope -eq 'full' -or $_.BaseName -notlike '*.Ui.Tests' } |
    Sort-Object BaseName
foreach ($project in $projects) {
    Invoke-Step "Test $($project.BaseName)" { dotnet test --project $project.FullName --configuration Release --no-build }
}
& (Join-Path $PSScriptRoot 'size-report.ps1')
Write-Host "verify ($Scope) passed." -ForegroundColor Green
