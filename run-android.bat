@echo off
REM Build, deploy and launch on a connected Android device.
REM Optional first argument is the device serial (adb devices).
REM EmbedAssembliesIntoApk avoids Fast Deployment, which stages assemblies in the
REM app's data directory and breaks if that directory is ever cleared.
if "%~1"=="" (
    dotnet build -t:Run -f net10.0-android -p:EmbedAssembliesIntoApk=true
) else (
    dotnet build -t:Run -f net10.0-android "-p:AdbTarget=-s %~1" -p:EmbedAssembliesIntoApk=true
)
