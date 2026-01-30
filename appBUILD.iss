#define MyAppName "PerformSwitch"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "WebDec"
#define MyAppExeName "PerformSwitch.exe"
#define MySourceDir "C:\Users\webdec\Desktop\App"

[Setup]
AppId={{B8B7D4A9-5E5D-4D31-9A6B-0B9C9D7C2A11}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}

; ✅ Per-user install (geen admin nodig)
DefaultDirName={localappdata}\{#MyAppName}
PrivilegesRequired=lowest

DefaultGroupName={#MyAppName}
OutputDir={#MySourceDir}
OutputBaseFilename=PerformSwitchSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern

; ✅ Installer icoon
SetupIconFile={#MySourceDir}\PFS.ico

; ✅ Control Panel / Apps & Features icoon (uit de EXE)
UninstallDisplayIcon={app}\{#MyAppExeName},0
UninstallDisplayName={#MyAppName}

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked
Name: "startup"; Description: "Start PerformSwitch with &Windows"; GroupDescription: "Startup:"; Flags: checkedonce

[Files]
; ✅ Installeer alles uit je publish map (exe + dll + png + ico)
Source: "{#MySourceDir}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
; ✅ Startmenu shortcut (met juiste icoon)
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\PFS.ico"

; ✅ Desktop shortcut (optioneel)
Name: "{userdesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\PFS.ico"; Tasks: desktopicon

; ✅ Auto-start met Windows (per gebruiker)
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\PFS.ico"; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
