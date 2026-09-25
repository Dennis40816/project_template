<#
.SYNOPSIS
  .agents/skills is the single source of agent skills. This script writes the identical
  projection that Claude Code reads (.claude/skills). With -Check it only reports drift.
#>
[CmdletBinding()]
param([switch]$Check)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root '.agents/skills'
$target = Join-Path $root '.claude/skills'

$drift = @()
$sourceSkills = @(Get-ChildItem -LiteralPath $source -Directory)
foreach ($skill in $sourceSkills) {
    $from = Join-Path $skill.FullName 'SKILL.md'
    $to = Join-Path (Join-Path $target $skill.Name) 'SKILL.md'
    $expected = Get-Content -LiteralPath $from -Raw
    $current = if (Test-Path -LiteralPath $to) { Get-Content -LiteralPath $to -Raw } else { $null }
    if ($current -ne $expected) {
        $drift += $skill.Name
        if (-not $Check) {
            New-Item -ItemType Directory -Force -Path (Split-Path $to) | Out-Null
            [IO.File]::WriteAllText($to, $expected)
        }
    }
}

if (Test-Path -LiteralPath $target) {
    foreach ($orphan in Get-ChildItem -LiteralPath $target -Directory | Where-Object { $sourceSkills.Name -notcontains $_.Name }) {
        $drift += "$($orphan.Name) (orphan)"
        if (-not $Check) { Remove-Item -Recurse -Force -LiteralPath $orphan.FullName }
    }
}

if ($Check -and $drift) {
    Write-Error "Agent skill projection is stale: $($drift -join ', '). Run scripts/sync-agent-skills.ps1."
    exit 1
}
if (-not $Check -and $drift) { Write-Host "Updated .claude/skills: $($drift -join ', ')" }
$global:LASTEXITCODE = 0
