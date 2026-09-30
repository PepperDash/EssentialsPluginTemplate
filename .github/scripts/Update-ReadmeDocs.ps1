<#
.SYNOPSIS
  Regenerates the plugin documentation sections in README.md locally.
.DESCRIPTION
  Runs the same metadata.py used by the PepperDash/workflow-templates update-readme
  workflow, but against the local working tree so the result lands in your branch.
  Sections live between <!-- START name --> / <!-- END name --> markers; add
  <!-- SKIP --> inside a section to stop it from being regenerated.
.PARAMETER Ref
  workflow-templates branch or tag to download metadata.py from.
.PARAMETER ScriptPath
  Use a local metadata.py instead of downloading one.
#>
[CmdletBinding()]
param(
  [string]$Ref = 'main',
  [string]$ScriptPath
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')

$python = @('python3', 'python', 'py') |
  ForEach-Object { Get-Command $_ -ErrorAction SilentlyContinue } |
  Where-Object { $_.Source -notmatch 'WindowsApps' } |
  Select-Object -First 1
if (-not $python) {
  [Console]::Error.WriteLine('readme-docs: Python 3 not found on PATH (https://www.python.org/downloads/)')
  exit 1
}

if (-not $ScriptPath) {
  $ScriptPath = Join-Path ([System.IO.Path]::GetTempPath()) 'pd-readme-metadata.py'
  $url = "https://raw.githubusercontent.com/PepperDash/workflow-templates/$Ref/.github/scripts/metadata.py"
  Invoke-WebRequest -Uri $url -OutFile $ScriptPath -UseBasicParsing
}

# metadata.py logs at DEBUG level; keep only INFO and above
& $python.Source $ScriptPath $root 2>&1 |
  Where-Object { "$_" -notmatch '^DEBUG:' } |
  ForEach-Object { Write-Host $_ }
if ($LASTEXITCODE -ne 0) {
  [Console]::Error.WriteLine("readme-docs: metadata.py failed (exit $LASTEXITCODE)")
  exit $LASTEXITCODE
}

# Config Example: "type" = first TypeNames entry of the first *Factory.cs (by name); "uid" is unused and removed.
# Applied even to <!-- SKIP --> sections.
$utf8 = New-Object System.Text.UTF8Encoding($false)
$configType = $null
$factories = Get-ChildItem -Path (Join-Path $root 'src') -Filter '*Factory.cs' -Recurse |
  Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
  Sort-Object Name
foreach ($file in $factories) {
  $code = [regex]::Replace([System.IO.File]::ReadAllText($file.FullName, $utf8), '//.*', '')
  $m = [regex]::Match($code, 'TypeNames\s*=\s*new\s+List<string>\s*\(\s*\)\s*\{\s*"([^"]+)"')
  if ($m.Success) { $configType = $m.Groups[1].Value; break }
}
$readmePath = Join-Path $root 'README.md'

# Base Classes and Interfaces: types declared by the plugin's own device classes (not factories or join maps).
# Interfaces go in the generator's "Interfaces Implemented" section, retitled "Interfaces". Sections marked <!-- SKIP --> are left alone.
function Split-TopLevel([string]$list) {
  $parts = @(); $depth = 0; $cur = ''
  foreach ($ch in $list.ToCharArray()) {
    if ($ch -eq '<') { $depth++ } elseif ($ch -eq '>') { $depth-- }
    if ($ch -eq ',' -and $depth -eq 0) { $parts += $cur.Trim(); $cur = '' } else { $cur += $ch }
  }
  if ($cur.Trim()) { $parts += $cur.Trim() }
  $parts
}
$classPattern = [regex]'(?m)^[ \t]*(?:(?:public|internal|abstract|sealed|static|partial)[ \t]+)*class[ \t]+(\w+)[ \t\r\n]*:([^{]+)\{'
$baseTypes = New-Object System.Collections.Generic.List[string]
$interfaceTypes = New-Object System.Collections.Generic.List[string]
$sources = Get-ChildItem -Path (Join-Path $root 'src') -Filter '*.cs' -Recurse |
  Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
  Sort-Object Name
foreach ($file in $sources) {
  $code = [System.IO.File]::ReadAllText($file.FullName, $utf8)
  $code = [regex]::Replace([regex]::Replace($code, '(?s)/\*.*?\*/', ''), '//.*', '')
  foreach ($m in $classPattern.Matches($code)) {
    if ($m.Groups[1].Value -like '*Factory') { continue }
    $bases = Split-TopLevel (($m.Groups[2].Value -split '\swhere\s')[0])
    if ($bases -contains 'JoinMapBaseAdvanced') { continue }
    foreach ($b in $bases) {
      if ($b -match '^I[A-Z]') {
        if (-not $interfaceTypes.Contains($b)) { $interfaceTypes.Add($b) }
      }
      elseif (-not $baseTypes.Contains($b)) { $baseTypes.Add($b) }
    }
  }
}

function Set-SectionBody([string]$text, [string]$name, [string]$body) {
  $pattern = '(?s)(<!-- START ' + [regex]::Escape($name) + ' -->)(.*?)(<!-- END ' + [regex]::Escape($name) + ' -->)'
  $m = [regex]::Match($text, $pattern)
  if (-not $m.Success -or $m.Groups[2].Value -match '<!-- SKIP -->') { return $text }
  $text.Substring(0, $m.Index) + $m.Groups[1].Value + $body + $m.Groups[3].Value + $text.Substring($m.Index + $m.Length)
}

if (Test-Path $readmePath) {
  $original = [System.IO.File]::ReadAllText($readmePath, $utf8)
  $readme = $original
  $nl = if ($readme.Contains("`r`n")) { "`r`n" } else { "`n" }

  $section = [regex]::Match($readme, '(?s)<!-- START Config Example -->.*?<!-- END Config Example -->')
  if ($section.Success) {
    $fixed = [regex]::Replace($section.Value, '(?m)^[ \t]*"uid":\s*\d+,?[ \t]*\r?\n', '')
    if ($configType) {
      $fixed = ([regex]'"type":\s*"[^"]*"').Replace($fixed, "`"type`": `"$configType`"", 1)
    }
    $readme = $readme.Substring(0, $section.Index) + $fixed + $readme.Substring($section.Index + $section.Length)
  }

  $listBody = {
    param([string]$title, $items)
    if (@($items).Count -eq 0) { return $nl }
    $nl + "### $title" + $nl + $nl + ((@($items) | ForEach-Object { "- ``$_``" }) -join $nl) + $nl
  }
  $readme = Set-SectionBody $readme 'Base Classes' (& $listBody 'Base Classes' $baseTypes)
  $readme = Set-SectionBody $readme 'Interfaces Implemented' (& $listBody 'Interfaces' $interfaceTypes)

  if ($readme -cne $original) { [System.IO.File]::WriteAllText($readmePath, $readme, $utf8) }
}

git -C $root diff --stat -- README.md
Write-Host 'readme-docs: review the README.md diff, then commit it with your changes'
