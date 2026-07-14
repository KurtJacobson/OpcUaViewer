param(
    [switch]$SkipPublish
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Version from git ─────────────────────────────────────────────────────────
$exactTag = try { git describe --tags --exact-match HEAD 2>$null } catch { $null }
$isDirty  = [bool](git status --porcelain)

if ($exactTag -and -not $isDirty) {
    $version = $exactTag -replace '^v', ''
} else {
    $shortHash  = git rev-parse --short HEAD 2>$null
    $nearestTag = try { git describe --tags --abbrev=0 HEAD 2>$null } catch { $null }
    $base       = if ($nearestTag) { $nearestTag -replace '^v', '' } else { '0.1.0' }
    $version    = "$base-$shortHash"
}

Write-Host "Building version: $version"

# ── Publish app ───────────────────────────────────────────────────────────────
if (-not $SkipPublish) {
    dotnet publish OpcUaViewer\OpcUaViewer.csproj `
        -c Release -r win-x64 --self-contained `
        -o OpcUaViewer\publish
}

# ── Compile installer ─────────────────────────────────────────────────────────
& "C:\Program Files (x86)\Inno Setup 6\iscc.exe" `
    "/DMyAppVersion=$version" `
    OpcUaViewer.iss
