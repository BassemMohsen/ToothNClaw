; ========================
; Tooth Installer
; ========================

[Setup]
AppName=Tooth
AppVersion=1.0.0
DefaultDirName={autopf}\Tooth
DefaultGroupName=Tooth
OutputDir=Output
OutputBaseFilename=ToothInstaller
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin

[Files]
; Backend Server
Source: "{#SourcePath}\Tooth\bin\x64\{#MyConfiguration}\*"; DestDir: "{app}\Backend"; Flags: recursesubdirs


; Game Bar Widget package (optional)
Source: "Build\Tooth.GameBarWidget\Tooth.GameBarWidget.msixbundle"; DestDir: "{app}\Widget"

[Icons]
Name: "{group}\Tooth Backend"; Filename: "{app}\Backend\Tooth.Backend.exe"; IconFilename: "{app}\Backend\tooth.ico"
Name: "{userdesktop}\Tooth Backend"; Filename: "{app}\Backend\Tooth.Backend.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Run]
; Optional: install the Game Bar Widget silently
Filename: "powershell.exe"; \
  Parameters: "-ExecutionPolicy Bypass -Command \"Add-AppxPackage -ForceApplicationShutdown -Path '{app}\Widget\Tooth.GameBarWidget.msixbundle'\""; \
  StatusMsg: "Installing Game Bar Widget..."; Flags: runhidden shellexec

; Create scheduled task for auto-start (run with highest privileges)
Filename: "schtasks.exe"; \
  Parameters: "/create /tn ""Tooth Backend"" /tr ""'{app}\Backend\Tooth.Backend.exe'"" /sc ONLOGON /RL HIGHEST /F"; \
  Flags: runhidden

; Optionally launch the backend immediately
Filename: "{app}\Backend\Tooth.Backend.exe"; Description: "Launch Tooth Backend now"; Flags: postinstall nowait

[UninstallRun]
; Clean up scheduled task on uninstall
Filename: "schtasks.exe"; Parameters: "/delete /tn ""Tooth Backend"" /f"; Flags: runhidden