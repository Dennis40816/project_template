<#
.SYNOPSIS
  Turns a fresh copy of the template into a new product.

.EXAMPLE
  ./scripts/init-project.ps1 -Name Acme.FwTool
  Renames every "ProjectTemplate" (file contents, file and folder names) to "Acme.FwTool",
  resets VERSION to 0.1.0, removes template-only files, and regenerates lock files.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[A-Z][A-Za-z0-9]*(\.[A-Z][A-Za-z0-9]*)*$')]
    [string]$Name,
    [switch]$Commit
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root
$old = 'ProjectTemplate'
if (-not (Test-Path "$old.slnx")) { throw "$old.slnx not found: this repository was already initialized." }

$skip = '[\\/](\.git|bin|obj|artifacts|\.vs)[\\/]'
$textExtensions = @('.cs', '.csproj', '.slnx', '.props', '.targets', '.axaml', '.json', '.md', '.ps1', '.yml', '.yaml', '.config', '.editorconfig', '')

# 1. File contents (exact case and the lowercase form used by packages.lock.json).
Get-ChildItem -Recurse -File -Force |
    Where-Object { $_.FullName -notmatch $skip -and $textExtensions -contains $_.Extension -and $_.FullName -ne $PSCommandPath } |
    ForEach-Object {
    $text = [IO.File]::ReadAllText($_.FullName)
    $updated = $text.Replace($old, $Name).Replace($old.ToLowerInvariant(), $Name.ToLowerInvariant())
    if ($updated -ne $text) { [IO.File]::WriteAllText($_.FullName, $updated) }
}

# 2. File and folder names, deepest first.
Get-ChildItem -Recurse -Force | Where-Object { $_.FullName -notmatch $skip -and $_.Name -like "*$old*" } |
    Sort-Object { $_.FullName.Length } -Descending |
    ForEach-Object { Rename-Item -LiteralPath $_.FullName -NewName $_.Name.Replace($old, $Name) }

# 3. Fresh product state.
Set-Content -LiteralPath VERSION -Value '0.1.0' -Encoding ascii
Get-ChildItem -Recurse -Filter 'packages.lock.json' | Remove-Item
Get-ChildItem -Recurse -Filter '*.approved.png' | Remove-Item
if (Test-Path TEMPLATE.md) { Remove-Item TEMPLATE.md }

dotnet restore "$Name.slnx"
if ($LASTEXITCODE -ne 0) { throw 'Restore failed after renaming.' }

if (-not (Test-Path .git)) { git init -b main | Out-Null }
if ($Commit) {
    git add -A
    git commit -m "chore: initialize $Name from project_template"
}

Write-Host ""
Write-Host "Initialized $Name. Next:" -ForegroundColor Green
Write-Host "  1. Edit CONTEXT.md and docs/spec.md for the new product."
Write-Host "  2. ./scripts/verify.ps1 -Scope full, review the UI snapshot candidate, then ./scripts/approve-snapshots.ps1"
Write-Host "  3. Push to GitHub, then ./scripts/github/apply-repo-settings.ps1 -Repo <owner>/<repo>"
