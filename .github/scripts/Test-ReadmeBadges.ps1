[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$fixture = Join-Path ([System.IO.Path]::GetTempPath()) ("readme-badges-" + [guid]::NewGuid())
$utf8 = [System.Text.UTF8Encoding]::new($false)
$pwsh = (Get-Process -Id $PID).Path

function Assert($condition, $message) {
  if (-not $condition) { throw $message }
}

function Write-Fixture($moniker, $packageVersion = '3.0.0-rc.19') {
  [System.IO.File]::WriteAllText((Join-Path $fixture 'src/Example.4Series.csproj'), @"
<Project><PropertyGroup><TargetFramework>$moniker</TargetFramework></PropertyGroup>
<ItemGroup><PackageReference Include="PepperDashEssentials" Version="$packageVersion" /></ItemGroup></Project>
"@, $utf8)
  [System.IO.File]::WriteAllText((Join-Path $fixture 'src/ExampleFactory.cs'), @'
MinimumEssentialsFrameworkVersion = "2.12.1";
MinimumEssentialsFrameworkVersion = "3.0.0";
MinimumEssentialsFrameworkVersion = "2.9.0";
'@, $utf8)
  [System.IO.File]::WriteAllText($readme, $original, $utf8)
}

function Invoke-Badges([string[]]$scriptArguments = @(), [int]$expectedExit = 0) {
  # Run in a child process: the production script calls exit, including on success.
  $output = & $pwsh -NoProfile -File $script @scriptArguments 2>&1 | Out-String
  Assert ($LASTEXITCODE -eq $expectedExit) "Expected exit $expectedExit, got ${LASTEXITCODE}: $output"
  return $output
}

try {
  New-Item -ItemType Directory -Path (Join-Path $fixture '.github/scripts'), (Join-Path $fixture 'src') -Force | Out-Null
  $script = Join-Path $fixture '.github/scripts/Update-ReadmeBadges.ps1'
  Copy-Item (Join-Path $PSScriptRoot 'Update-ReadmeBadges.ps1') $script
  $readme = Join-Path $fixture 'README.md'
  $original = @'
Unrelated text: ≥ keep this intact.
![PepperDash Essentials](https://img.shields.io/badge/stale-blue)
![.NET](https://img.shields.io/badge/stale-purple)
'@

  $cases = @(
    @('net472', '.NET%20Framework', '4.7.2'),
    @('net48', '.NET%20Framework', '4.8'),
    @('net10', '.NET%20Framework', '1.0'), # Existing legacy precedence is intentional.
    @('net8', '.NET', '8'),
    @('net8.0', '.NET', '8.0'),
    @('net10.0', '.NET', '10.0'),
    @('netstandard2.0', '.NET', 'netstandard2.0'),
    @('net8.0junk', '.NET', 'net8.0junk'),
    @('net8.0.1', '.NET', 'net8.0.1'),
    @('net8.', '.NET', 'net8.'),
    @('net8-windows', '.NET', 'net8--windows'),
    @('net8.0_windows', '.NET', 'net8.0__windows'),
    @('net', '.NET', 'net')
  )
  foreach ($case in $cases) {
    Write-Fixture $case[0]
    $output = Invoke-Badges
    Assert ($output -match 'package 3\.0\.0-rc\.19 != MinimumEssentialsFrameworkVersion 3\.0\.0') 'Literal package/minimum warning must be retained'
    $updated = [System.IO.File]::ReadAllText($readme, $utf8)
    $expected = $original.Replace('https://img.shields.io/badge/stale-purple',
      "https://img.shields.io/badge/$($case[1])-$($case[2])-512BD4").Replace(
      'https://img.shields.io/badge/stale-blue', 'https://img.shields.io/badge/PepperDash%20Essentials-≥%20v3.0.0-blue')
    Assert ($updated -ceq $expected) "Incorrect badges or unrelated README edits for $($case[0])"
    Invoke-Badges @('-Check') | Out-Null
    Invoke-Badges | Out-Null
    Assert ([System.IO.File]::ReadAllText($readme, $utf8) -ceq $updated) "Update/-Check not idempotent for $($case[0])"
  }

  Write-Fixture 'net8' '3.0.0'
  $output = Invoke-Badges @('-Check') 1
  Assert ($output -match 'badges are stale') 'Expected stale badge failure'
  Assert ($output -notmatch 'WARNING:') 'Matching numeric versions should not warn'
  Assert ([System.IO.File]::ReadAllText($readme, $utf8) -ceq $original) '-Check must not write README'

  Write-Fixture 'net8' '3.1.0'
  $output = Invoke-Badges
  Assert ($output -match 'package 3\.1\.0 != MinimumEssentialsFrameworkVersion 3\.0\.0') 'Genuine mismatch warning was lost'

  & git -C $fixture init --quiet
  Assert ($LASTEXITCODE -eq 0) 'Fixture git init failed'
  & git -C $fixture add .
  Assert ($LASTEXITCODE -eq 0) 'Fixture git add failed'
  foreach ($inputPath in @('README.md', 'src/Example.4Series.csproj', 'src/ExampleFactory.cs', 'src/UntrackedFactory.cs')) {
    $path = Join-Path $fixture $inputPath
    [System.IO.File]::AppendAllText($path, "`nUnstaged input", $utf8)
    $output = Invoke-Badges @('-Stage') 1
    Assert ($output -match 'unstaged or untracked badge inputs') "Stage guard failed for $inputPath"
    if ($inputPath -eq 'src/UntrackedFactory.cs') {
      Remove-Item $path
    } else {
      & git -C $fixture restore --worktree -- $inputPath
      Assert ($LASTEXITCODE -eq 0) 'Fixture git restore failed'
    }
  }
  Write-Fixture 'net8'
  & git -C $fixture add .
  Assert ($LASTEXITCODE -eq 0) 'Fixture git add failed'
  Invoke-Badges @('-Stage') | Out-Null
  $staged = & git -C $fixture show ':README.md' | Out-String
  Assert ($LASTEXITCODE -eq 0 -and $staged.Contains('/badge/.NET-8-512BD4')) '-Stage did not stage the updated badge'
  Invoke-Badges @('-Check') | Out-Null
  Write-Host "Passed $($cases.Count) framework cases, update/-Check idempotence, warnings, and staging guards."
} finally {
  if (Test-Path $fixture) { Remove-Item $fixture -Recurse -Force }
}
