<#
.SYNOPSIS
    Publishes AttendanceUI self-contained for Windows x64 and wraps it in a Setup .exe.

.DESCRIPTION
    Two steps:
      1. dotnet publish — self-contained, unpackaged, Windows App SDK included, so
         target machines need neither the .NET runtime nor the App SDK installed.
      2. ISCC (Inno Setup) — compiles installer\AttendanceUI.iss into
         installer\output\AttendanceUI-Setup-<version>.exe

    TargetFrameworks is pinned to the Windows TFM for the publish. Without that,
    restore also resolves runtime packs for the Android/iOS targets and fails
    looking for a Mono pack at win-x64.

.PARAMETER Version
    Version stamped into the installer and its filename. Default 1.0.0.

.PARAMETER SkipPublish
    Reuse the existing publish output instead of rebuilding it.

.PARAMETER RefreshIcons
    Delete the resizetizer cache before publishing. MAUI does not treat a changed
    MauiIcon source as invalidating its generated icons, so after editing the app
    icon a normal build silently keeps shipping the old one. Pass this whenever the
    icon or splash artwork changed.

.EXAMPLE
    .\installer\build-installer.ps1
    .\installer\build-installer.ps1 -Version 1.2.0
    .\installer\build-installer.ps1 -SkipPublish
    .\installer\build-installer.ps1 -RefreshIcons
#>
[CmdletBinding()]
param(
    [string] $Version = "1.0.0",
    [switch] $SkipPublish,
    [switch] $RefreshIcons
)

$ErrorActionPreference = "Stop"

$RepoRoot   = Split-Path -Parent $PSScriptRoot
$Tfm        = "net10.0-windows10.0.19041.0"
$Rid        = "win-x64"
$PublishDir = Join-Path $RepoRoot "bin\Release\$Tfm\$Rid\publish"
$IssFile    = Join-Path $PSScriptRoot "AttendanceUI.iss"
$OutputDir  = Join-Path $PSScriptRoot "output"

function Find-Iscc {
    $cmd = Get-Command iscc -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    # winget installs per-user by default, so %LOCALAPPDATA%\Programs is as likely
    # as Program Files. The registry key is authoritative when either misses.
    foreach ($candidate in @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    )) {
        if (Test-Path $candidate) { return $candidate }
    }

    foreach ($key in @(
        "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1",
        "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1",
        "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1"
    )) {
        try {
            $location = (Get-ItemProperty -Path $key -ErrorAction Stop).InstallLocation
            if ($location) {
                $candidate = Join-Path $location "ISCC.exe"
                if (Test-Path $candidate) { return $candidate }
            }
        }
        catch { }
    }

    return $null
}

# Fail before the slow publish if the compiler is missing.
$iscc = Find-Iscc
if (-not $iscc) {
    Write-Error @"
Inno Setup 6 was not found.

Install it with:
    winget install --id JRSoftware.InnoSetup --source winget

or download from https://jrsoftware.org/isdl.php, then re-run this script.
"@
}
Write-Host "Inno Setup: $iscc" -ForegroundColor DarkGray

# The app locks its own files while running.
$running = Get-Process AttendanceUI -ErrorAction SilentlyContinue
if ($running) {
    Write-Error "AttendanceUI is running (PID $($running.Id -join ', ')). Close it and re-run."
}

if ($SkipPublish) {
    if (-not (Test-Path (Join-Path $PublishDir "AttendanceUI.exe"))) {
        Write-Error "-SkipPublish was passed but no publish output exists at $PublishDir"
    }
    Write-Host "Reusing existing publish output." -ForegroundColor Yellow
}
else {
    if ($RefreshIcons) {
        $resizetizer = Join-Path $RepoRoot "obj\Release\$Tfm\$Rid\resizetizer"
        if (Test-Path $resizetizer) {
            Write-Host "Clearing resizetizer cache so the icon regenerates..." -ForegroundColor Yellow
            Remove-Item $resizetizer -Recurse -Force
        }
    }

    Write-Host "`nPublishing $Tfm / $Rid (self-contained)..." -ForegroundColor Cyan

    & dotnet publish $RepoRoot `
        -f $Tfm `
        -c Release `
        -r $Rid `
        --self-contained true `
        -p:TargetFrameworks=$Tfm `
        -p:WindowsPackageType=None `
        -p:WindowsAppSDKSelfContained=true `
        -p:ApplicationDisplayVersion=$Version `
        --nologo `
        -v minimal

    if ($LASTEXITCODE -ne 0) { Write-Error "dotnet publish failed with exit code $LASTEXITCODE." }
}

$payload = Get-ChildItem $PublishDir -Recurse -File
Write-Host ("Publish output: {0} files, {1:N1} MB" -f $payload.Count, (($payload | Measure-Object Length -Sum).Sum / 1MB))

Write-Host "`nCompiling installer..." -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

& $iscc "/DAppVersion=$Version" "/DPublishDir=$PublishDir" "/O$OutputDir" $IssFile
if ($LASTEXITCODE -ne 0) { Write-Error "ISCC failed with exit code $LASTEXITCODE." }

$setup = Join-Path $OutputDir "AttendanceUI-Setup-$Version.exe"
if (-not (Test-Path $setup)) { Write-Error "ISCC reported success but $setup is missing." }

Write-Host ("`nDone: {0} ({1:N1} MB)" -f $setup, ((Get-Item $setup).Length / 1MB)) -ForegroundColor Green
