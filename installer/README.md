# AttendanceUI installer

Builds a single `Setup.exe` for the Windows desktop app.

## Build it

```powershell
.\installer\build-installer.ps1
```

Output lands in `installer\output\AttendanceUI-Setup-1.0.0.exe`.

Other forms:

```powershell
.\installer\build-installer.ps1 -Version 1.2.0     # stamp a different version
.\installer\build-installer.ps1 -SkipPublish       # reuse the last publish (fast)
.\installer\build-installer.ps1 -RefreshIcons      # after changing the app icon
```

## Changing the app icon

The source is `Resources\AppIcon\appicon.png`, referenced by `MauiIcon` in
`AttendanceUI.csproj`. Two things to get right:

- **Make it square.** Resizetizer scales the source to each platform's icon sizes
  without preserving aspect ratio, so a non-square source comes out squashed. The
  current file is 1024x1024, padded from the original 531x550 artwork with the
  artwork's own corner colour (`#20262B`).
- **Rebuild with `-RefreshIcons`.** MAUI does *not* treat a changed `MauiIcon`
  source as invalidating its generated icons — a normal build happily keeps
  shipping the previous one. The switch deletes `obj\Release\...\resizetizer`
  first. Verify by checking that `publish\appicon.ico` changed size.

## Prerequisite

[Inno Setup 6.3+](https://jrsoftware.org/isdl.php):

```powershell
winget install --id JRSoftware.InnoSetup --source winget
```

The build script checks for it up front and stops before the slow publish if it's missing.

## What ships

The publish is **self-contained and unpackaged** — it carries the .NET runtime, the
Windows App SDK, and the SQLite native library. Target machines need nothing
pre-installed. That costs size: roughly 265 MB on disk, compressing to a
significantly smaller setup.

Publish flags worth knowing about, all set in `build-installer.ps1`:

| Flag | Why |
|---|---|
| `-p:TargetFrameworks=net10.0-windows...` | Restore otherwise resolves runtime packs for the Android/iOS targets too and fails looking for a Mono pack at `win-x64` |
| `-p:WindowsPackageType=None` | Unpackaged — a plain folder of files, not MSIX, so no code-signing certificate is needed to install |
| `-p:WindowsAppSDKSelfContained=true` | Bundles the App SDK; without it every target machine needs the runtime installed separately |

## Install behaviour

- **Per-machine by default** (Program Files, needs admin). The user can switch to a
  per-user install from the setup dialog — handy on locked-down school PCs where
  nobody has admin rights.
- **x64 only.** Setup refuses to run elsewhere rather than installing something
  that cannot start.
- **Upgrades in place.** The `AppId` GUID in `AttendanceUI.iss` is what Windows
  uses to recognise an upgrade — never change it, or the next version installs
  alongside the old one instead of replacing it.
- **The app is closed automatically** during install/uninstall via Restart Manager,
  rather than the install failing on locked files.

## User data is not removed on uninstall

The portable SQLite database lives outside the install directory:

```
%LOCALAPPDATA%\User Name\com.companyname.attendanceui\Data\school_sms.db
```

Uninstall deliberately leaves it alone, so reinstalling keeps existing records.
To wipe it, delete that folder by hand.

That `User Name` folder is derived from the app's publisher metadata and
`ApplicationId` (`com.companyname.attendanceui`) in `AttendanceUI.csproj` — both
still template defaults. They can be changed to something real, **but doing so
moves the data directory**, orphaning any existing database. If you want to
rename them, do it before rollout, or ship a migration that relocates the file.

## Signing

The setup is unsigned, so SmartScreen will warn on first download ("Windows
protected your PC" → *More info* → *Run anyway*). To remove that warning you need
a code-signing certificate, then sign both `AttendanceUI.exe` and the setup:

```powershell
signtool sign /fd SHA256 /f cert.pfx /p <password> /tr http://timestamp.digicert.com /td SHA256 <file>
```

Reputation with SmartScreen builds over time unless the certificate is EV.
