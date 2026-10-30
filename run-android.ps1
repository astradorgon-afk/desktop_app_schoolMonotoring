# Build, deploy and launch on a connected Android device.
# Pass a serial to pick a device when more than one is attached:
#   .\run-android.ps1 R58M38607WT

param([string]$Serial)

$devices = @(adb devices | Select-Object -Skip 1 | Where-Object { $_ -match '\sdevice$' })

if ($devices.Count -eq 0) {
    Write-Error "No authorized Android device found. Connect the phone, enable USB debugging, and accept the RSA prompt."
    exit 1
}

if (-not $Serial -and $devices.Count -gt 1) {
    Write-Host "Multiple devices attached - pass a serial:"
    $devices | ForEach-Object { "  " + ($_ -split '\s+')[0] }
    exit 1
}

# EmbedAssembliesIntoApk packages the .NET assemblies inside the APK instead of
# using Fast Deployment, which stages them in the app's data directory. Fast
# Deployment is quicker but breaks with "No assemblies found ... Exiting" the
# moment that directory is wiped (adb shell pm clear, Settings > Clear data).
$common = @('-p:EmbedAssembliesIntoApk=true')

if ($Serial) {
    dotnet build -t:Run -f net10.0-android "-p:AdbTarget=-s $Serial" @common
} else {
    dotnet build -t:Run -f net10.0-android @common
}
