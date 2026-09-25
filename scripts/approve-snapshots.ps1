<#
.SYNOPSIS
  Promotes UI snapshot candidates (*.received.png) to approved images.
  Look at every candidate before running this; approval is a human decision.
#>
$root = Split-Path -Parent $PSScriptRoot
$candidates = Get-ChildItem -Path (Join-Path $root 'tests') -Recurse -Filter '*.received.png' |
    Where-Object { $_.Directory.Name -eq '__snapshots__' }
if (-not $candidates) { Write-Host 'No snapshot candidates to approve.'; return }
foreach ($candidate in $candidates) {
    $approved = Join-Path $candidate.DirectoryName ($candidate.Name -replace '\.received\.png$', '.approved.png')
    Move-Item -Force -LiteralPath $candidate.FullName -Destination $approved
    Write-Host "Approved $approved"
}
