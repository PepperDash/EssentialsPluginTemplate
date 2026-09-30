<#
.SYNOPSIS
  Syncs the README .NET and PepperDash Essentials badges with the project files.
.PARAMETER Check
  Exit 1 if the README badges are stale instead of rewriting them.
.PARAMETER Stage
  git add README.md when it was changed.
#>
[CmdletBinding()]
param(
  [switch]$Check,
  [switch]$Stage
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$srcDir = Join-Path $root 'src'
$readmePath = Join-Path $root 'README.md'
$utf8 = New-Object System.Text.UTF8Encoding($false)

function Fail($message) {
  [Console]::Error.WriteLine("badges: $message")
  exit 1
}

# Staging README.md adds its whole working-tree copy, and badges are computed from working-tree
# sources, so refuse to run when any of those inputs differ from the index.
if ($Stage) {
  $unstaged = @(git -C $root diff --name-only -- README.md 'src/*.4Series.csproj' 'src/*Factory.cs')
  if ($LASTEXITCODE) { Fail 'git diff failed' }
  if ($unstaged.Count -gt 0) {
    Fail ("unstaged changes in badge inputs ($($unstaged -join ', ')); stage or stash them, then commit again")
  }
}

# Target framework from the 4Series csproj
$csprojFile = Get-ChildItem -Path $srcDir -Filter '*.4Series.csproj' | Select-Object -First 1
if (-not $csprojFile) { Fail 'no *.4Series.csproj found in src/' }
$csproj = [System.IO.File]::ReadAllText($csprojFile.FullName, $utf8)
$tfmMatch = [regex]::Match($csproj, '<TargetFramework>\s*([^<\s]+)\s*</TargetFramework>')
if (-not $tfmMatch.Success) { Fail "TargetFramework not found in $($csprojFile.Name)" }
$tfm = $tfmMatch.Groups[1].Value

# Minimum Essentials version: highest MinimumEssentialsFrameworkVersion across factories
$minVersions = @()
foreach ($file in Get-ChildItem -Path $srcDir -Filter '*Factory.cs') {
  $text = [System.IO.File]::ReadAllText($file.FullName, $utf8)
  $found = [regex]::Matches($text, '(?m)^\s*MinimumEssentialsFrameworkVersion\s*=\s*"(\d+(?:\.\d+)+)"\s*;')
  foreach ($m in $found) { $minVersions += $m.Groups[1].Value }
}
if ($minVersions.Count -eq 0) { Fail 'MinimumEssentialsFrameworkVersion not found in src/*Factory.cs' }
$minVersion = ($minVersions | Sort-Object { [version]$_ } | Select-Object -Last 1)

# Warn when the NuGet reference and the factory minimum disagree
$pkgMatch = [regex]::Match($csproj, '<PackageReference\s+Include="PepperDashEssentials"\s+Version="([^"]+)"')
if ($pkgMatch.Success -and $pkgMatch.Groups[1].Value -ne $minVersion) {
  Write-Warning "badges: PepperDashEssentials package $($pkgMatch.Groups[1].Value) != MinimumEssentialsFrameworkVersion $minVersion"
}

function Get-FrameworkBadge($moniker) {
  $legacy = [regex]::Match($moniker, '^net(\d)(\d)(\d)?$')
  if ($legacy.Success) {
    $parts = @($legacy.Groups[1].Value, $legacy.Groups[2].Value)
    if ($legacy.Groups[3].Success) { $parts += $legacy.Groups[3].Value }
    return @{ Label = '.NET Framework'; Message = ($parts -join '.') }
  }
  $modern = [regex]::Match($moniker, '^net(\d+\.\d+)')
  if ($modern.Success) { return @{ Label = '.NET'; Message = $modern.Groups[1].Value } }
  return @{ Label = '.NET'; Message = $moniker }
}

# shields.io static badge path: '-' and '_' are escaped by doubling, spaces as %20
function Format-Segment($s) { $s.Replace('-', '--').Replace('_', '__').Replace(' ', '%20') }
function Get-BadgeUrl($label, $message, $color) {
  'https://img.shields.io/badge/{0}-{1}-{2}' -f (Format-Segment $label), (Format-Segment $message), $color
}

$fw = Get-FrameworkBadge $tfm
$essentialsMessage = '{0} v{1}' -f [char]0x2265, $minVersion
$replacements = @(
  @{
    Pattern = '!\[\.NET[^\]]*\]\(https://img\.shields\.io/badge/[^)]*\)'
    Value   = '![.NET](' + (Get-BadgeUrl $fw.Label $fw.Message '512BD4') + ')'
  },
  @{
    Pattern = '!\[PepperDash Essentials\]\(https://img\.shields\.io/badge/[^)]*\)'
    Value   = '![PepperDash Essentials](' + (Get-BadgeUrl 'PepperDash Essentials' $essentialsMessage 'blue') + ')'
  }
)

$original = [System.IO.File]::ReadAllText($readmePath, $utf8)
$updated = $original
foreach ($r in $replacements) {
  if (-not [regex]::IsMatch($updated, $r.Pattern)) { Fail "badge not found in README.md: $($r.Pattern)" }
  $value = $r.Value
  $updated = [regex]::Replace($updated, $r.Pattern, { param($m) $value }, 'None')
}

if ($updated -ceq $original) {
  Write-Host 'badges: README.md up to date'
  exit 0
}

if ($Check) { Fail 'README.md badges are stale; run .github/scripts/Update-ReadmeBadges.ps1' }

[System.IO.File]::WriteAllText($readmePath, $updated, $utf8)
Write-Host "badges: README.md updated ($($fw.Label) $($fw.Message), Essentials >= v$minVersion)"

if ($Stage) { git -C $root add README.md }
