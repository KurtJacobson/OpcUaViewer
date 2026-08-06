#Requires -Version 5.1
<#
.SYNOPSIS
    Publishes OPC UA Viewer and compiles the Inno Setup installer.

.PARAMETER Configuration
    Build configuration. Default: Release

.PARAMETER SkipPublish
    Skip the dotnet publish step and reuse an existing publish\ folder.
#>
param(
    [string] $Configuration = "Release",
    [switch] $SkipPublish
)

$ErrorActionPreference = "Stop"
$root    = $PSScriptRoot
$publish = Join-Path $root "publish"
$plugins = Join-Path $publish "plugins"
$tf      = "net8.0-windows"

# ── 1. Publish ────────────────────────────────────────────────────────────────
if (-not $SkipPublish) {
    if (Test-Path $publish) {
        Write-Host "Cleaning previous publish output..." -ForegroundColor Cyan
        Remove-Item $publish -Recurse -Force
    }
    Write-Host "Publishing OpcUaViewer.Wpf..." -ForegroundColor Cyan
    dotnet publish "$root\OpcUaViewer.Wpf\OpcUaViewer.Wpf.csproj" `
        -c $Configuration -o $publish
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }
}

# ── 2. Copy plugin DLLs into publish\plugins\ ─────────────────────────────────
# dotnet publish does not run the CopyShippedPlugins MSBuild target, so we do it here.
$pluginProjects = @(
    "OpcUaViewer.Plugin.OpcUa",
    "OpcUaViewer.Plugin.Groups",
    "OpcUaViewer.Plugin.Document",
    "OpcUaViewer.Plugin.Webcam",
    "OpcUaViewer.Plugin.Stats",
    "OpcUaViewer.Plugin.CsvSource"
)

Write-Host "Building and copying plugins..." -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $plugins | Out-Null

foreach ($proj in $pluginProjects) {
    dotnet build "$root\$proj\$proj.csproj" -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed for $proj." }

    $binDir = "$root\$proj\bin\$Configuration\$tf"
    $dll = "$binDir\$proj.dll"
    if (Test-Path $dll) {
        Copy-Item $dll -Destination $plugins -Force
        Write-Host "  Copied $proj.dll" -ForegroundColor Gray
    } else {
        Write-Warning "  $proj.dll not found at: $dll"
    }
}

# Webcam: copy managed wrapper to plugins\ and native DLL to app root
$webcamBin = "$root\OpcUaViewer.Plugin.Webcam\bin\$Configuration\$tf"
$managedCv = "$webcamBin\OpenCvSharp.dll"
$nativeCv  = "$webcamBin\OpenCvSharpExtern.dll"
if (Test-Path $managedCv) { Copy-Item $managedCv -Destination $plugins -Force; Write-Host "  Copied OpenCvSharp.dll" -ForegroundColor Gray }
if (Test-Path $nativeCv)  { Copy-Item $nativeCv  -Destination $publish  -Force; Write-Host "  Copied OpenCvSharpExtern.dll" -ForegroundColor Gray }

# ── 3. Compile installer ──────────────────────────────────────────────────────
$iscc = @(
    "C:\Program Files (x86)\Inno Setup 6\iscc.exe",
    "C:\Program Files\Inno Setup 6\iscc.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
    throw "Inno Setup not found. Install from https://jrsoftware.org/isinfo.php"
}

Write-Host "Compiling installer..." -ForegroundColor Cyan
& $iscc "$root\Installer\OpcUaViewer.iss"
if ($LASTEXITCODE -ne 0) { throw "iscc failed." }

Write-Host ""
Write-Host "Done. Installer written to Installer\Output\" -ForegroundColor Green
