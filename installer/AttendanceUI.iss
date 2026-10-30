; ─────────────────────────────────────────────────────────────────────────────
; Inno Setup script for AttendanceUI.
;
; Wraps the self-contained Windows publish (bin\Release\...\win-x64\publish) into
; a single Setup .exe. The publish already carries the .NET runtime, the Windows
; App SDK and the SQLite native library, so target machines need no prerequisites.
;
; Build with installer\build-installer.ps1 — it publishes first, then invokes this.
; Requires Inno Setup 6.3 or newer (ArchitecturesAllowed=x64compatible).
; ─────────────────────────────────────────────────────────────────────────────

#define AppName        "Attendance System"
#define AppShortName   "AttendanceUI"
#define AppPublisher   "Attendance System"
#define AppExeName     "AttendanceUI.exe"

; Overridable from the command line: ISCC /DAppVersion=1.1.0
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

; Overridable so the script can be run against any publish folder.
#ifndef PublishDir
  #define PublishDir "..\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish"
#endif

[Setup]
; Keep AppId stable forever — it is how Windows recognises an upgrade rather
; than a second parallel installation.
AppId={{C62763EF-5779-42AE-98C2-128C8B0A6A62}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppShortName}
DefaultGroupName={#AppName}
UninstallDisplayName={#AppName}
UninstallDisplayIcon={app}\{#AppExeName}
OutputDir=output
OutputBaseFilename={#AppShortName}-Setup-{#AppVersion}
SetupIconFile={#PublishDir}\appicon.ico

; The app is x64-only (win-x64 publish), so refuse to install anywhere it cannot run.
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; Default to a per-machine install, but let the user drop to a per-user install
; (no admin rights) from the setup dialog — useful on locked-down school PCs.
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; ~265 MB of payload; lzma2/max brings the setup down substantially.
Compression=lzma2/max
SolidCompression=yes
LZMAUseSeparateProcess=yes

; Use Restart Manager to shut the app down cleanly instead of failing on locked files.
CloseApplications=yes
RestartApplications=no

WizardStyle=modern
DisableProgramGroupPage=yes
ShowLanguageDialog=no
MinVersion=10.0.17763

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
; Recurse the whole publish folder. Everything in there is required.
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Logs and caches the app writes beside itself. The user's database lives in
; %LOCALAPPDATA% and is deliberately left untouched — see the note in README.md.
Type: filesandordirs; Name: "{app}\logs"
Type: dirifempty; Name: "{app}"
