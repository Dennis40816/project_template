<#
.SYNOPSIS
  Checks a packaged zip: its SHA-256, its version, and one golden case through the CLI.
  Used on the candidate and again on the published download.
#>
[CmdletBinding()]
param([Parameter(Mandatory)][string]$PackagePath)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$package = Get-Item -LiteralPath $PackagePath
$sums = Join-Path $package.DirectoryName 'SHA256SUMS'
$expected = ((Get-Content -LiteralPath $sums) -split '\s+')[0]
$actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $package.FullName).Hash.ToLowerInvariant()
if ($expected -ne $actual) { throw "SHA-256 mismatch: expected $expected, got $actual." }

$extract = Join-Path ([IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString('N'))
Expand-Archive -LiteralPath $package.FullName -DestinationPath $extract
try {
    $release = Get-Content -LiteralPath (Join-Path $extract 'RELEASE.json') -Raw | ConvertFrom-Json
    $cli = Get-ChildItem -Path (Join-Path $extract 'cli') -Filter '*.Cli.exe' | Select-Object -First 1
    if (-not $cli) { throw 'CLI executable is missing from the package.' }

    $reported = (& $cli.FullName --version).Trim()
    if ($reported -ne $release.version) { throw "CLI reports $reported, package says $($release.version)." }

    $sample = Join-Path $root 'testdata/golden/cases/sample-256.bin'
    $golden = (Get-Content -LiteralPath (Join-Path $root 'testdata/golden/cases/sample-256.expected.json') -Raw).Trim()
    $output = ((& $cli.FullName inspect $sample) -join "`n").Trim()
    if ($LASTEXITCODE -ne 0 -or $output.Replace("`r`n", "`n") -ne $golden.Replace("`r`n", "`n")) {
        throw "Packaged CLI output differs from the golden case:`n$output"
    }
    Write-Host "Smoke passed for $($package.Name) (v$($release.version))." -ForegroundColor Green
} finally {
    Remove-Item -Recurse -Force $extract
}
