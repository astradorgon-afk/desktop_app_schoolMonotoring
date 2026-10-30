# desktop_app_schoolMonotoring

.NET MAUI + Blazor Hybrid desktop app for school attendance monitoring.

## Running

The project is multi-targeted (`net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`
and `net10.0-windows10.0.19041.0`), so a bare `dotnet run` cannot pick a head — it
stops immediately with "Your project targets multiple frameworks. Specify which
framework to run using '--framework'". Name the framework:

```bat
:: Windows desktop - the day-to-day target on this machine
run.bat
:: which is just:
dotnet run --framework net10.0-windows10.0.19041.0
```

```powershell
# Android device or emulator (see the header of the script for options)
.\run-android.ps1
```

## Diagnosing a failed Windows start

A WinUI process that throws while starting up dies before it can print anything, so
Windows' only record is a generic APPCRASH (`0xc000027b`, "stowed exception") naming
a WinUI DLL rather than your code. The Windows head therefore logs every unhandled
startup exception to `%LOCALAPPDATA%\AttendanceUI\logs\startup.log` — check there
first when the app appears to do nothing at all.
