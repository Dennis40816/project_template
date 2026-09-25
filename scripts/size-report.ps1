<#
.SYNOPSIS
  Advisory size report. Lists types whose files (partial files counted together) exceed
  the threshold. It never fails the build: size is a prompt to look, not a gate.
#>
[CmdletBinding()]
param([int]$Threshold = 400)

$root = Split-Path -Parent $PSScriptRoot
$groups = Get-ChildItem -Path (Join-Path $root 'src') -Recurse -Include '*.cs', '*.axaml' -File |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
    ForEach-Object {
        # Foo.cs, Foo.Part.cs and Foo.axaml.cs all count toward type "Foo" in the same folder.
        $type = $_.Name.Split('.')[0]
        [pscustomobject]@{
            Key   = Join-Path $_.DirectoryName $type
            Lines = @(Get-Content -LiteralPath $_.FullName | Where-Object { $_.Trim() -ne '' }).Count
        }
    } |
    Group-Object Key |
    ForEach-Object {
        [pscustomobject]@{
            Type  = $_.Name.Substring($root.Length + 1)
            Files = $_.Count
            Lines = ($_.Group | Measure-Object Lines -Sum).Sum
        }
    } |
    Where-Object { $_.Lines -gt $Threshold } |
    Sort-Object Lines -Descending

if ($groups) {
    Write-Host "Size report (advisory): types over $Threshold non-blank lines" -ForegroundColor Yellow
    $groups | Format-Table -AutoSize | Out-String | Write-Host
} else {
    Write-Host "Size report (advisory): no type over $Threshold non-blank lines."
}
$global:LASTEXITCODE = 0
