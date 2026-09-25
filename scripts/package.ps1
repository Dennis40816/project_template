<#
.SYNOPSIS
  Builds the self-contained Windows package: app/ (Desktop), cli/ (CLI), RELEASE.json,
  then a zip and its SHA256SUMS. The file list is closed: anything unexpected fails.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Version,
    [Parameter(Mandatory)][string]$Commit,
    [string]$Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$product = (Get-ChildItem -Path $root -Filter '*.slnx' | Select-Object -First 1).BaseName

$fileVersion = (Get-Content -LiteralPath (Join-Path $root 'VERSION') -Raw).Trim()
if ($fileVersion -ne $Version) { throw "VERSION is $fileVersion but $Version was requested." }

$name = "$product-v$Version-$Runtime"
$outRoot = Join-Path $root 'artifacts/release'
$stage = Join-Path $outRoot $name
if (Test-Path $outRoot) { Remove-Item -Recurse -Force $outRoot }
New-Item -ItemType Directory -Force -Path $stage | Out-Null

foreach ($target in @(@{ Project = "$product.Desktop"; Dir = 'app' }, @{ Project = "$product.Cli"; Dir = 'cli' })) {
    $csproj = Join-Path $root "src/$($target.Project)/$($target.Project).csproj"
    dotnet publish $csproj --configuration Release --runtime $Runtime --self-contained true `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
        --output (Join-Path $stage $target.Dir)
    if ($LASTEXITCODE -ne 0) { throw "Publishing $($target.Project) failed." }
}

$release = [ordered]@{
    product = $product
    version = $Version
    commit  = $Commit
    runtime = $Runtime
    builtAt = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
}
$release | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $stage 'RELEASE.json') -Encoding utf8

# Closed allowlist: only these top-level entries may ship.
$allowed = @('app', 'cli', 'RELEASE.json')
$unexpected = Get-ChildItem -LiteralPath $stage | Where-Object { $allowed -notcontains $_.Name }
if ($unexpected) { throw "Unexpected package entries: $($unexpected.Name -join ', ')" }

$zip = Join-Path $outRoot "$name.zip"
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip
$hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $zip).Hash.ToLowerInvariant()
"$hash  $name.zip" | Set-Content -LiteralPath (Join-Path $outRoot 'SHA256SUMS') -Encoding ascii
Remove-Item -Recurse -Force $stage
Write-Host "Packaged $zip"
