; OPC UA Viewer — Inno Setup Script
; Build with: iscc OpcUaViewer.iss
; Requires a Release publish first:
;   dotnet publish OpcUaViewer.Wpf\OpcUaViewer.Wpf.csproj -c Release -o publish\

#define AppName      "OPC UA Viewer"
#define AppVersion   "1.0.0"
#define AppPublisher "Metalforming USA"
#define AppExeName   "OpcUaViewer.exe"
#define PublishDir   "..\publish"

[Setup]
AppId={{B3C1A7E2-4F8D-4A3B-9C5E-0D6F2A1B8E34}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisherURL=https://metalforming-usa.net
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
OutputDir=Output
OutputBaseFilename=OpcUaViewerSetup-{#AppVersion}
SetupIconFile=..\OpcUaViewer\Resources\app.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Components]
Name: "main";    Description: "OPC UA Viewer (required)"; Types: full compact custom; Flags: fixed
Name: "groups";  Description: "Groups Plugin — manage production groups and product orders"; Types: full
Name: "docview"; Description: "Document Viewer Plugin — display product PDF documentation"; Types: full
Name: "webcam";  Description: "Webcam Plugin — live camera feed viewer"; Types: full
Name: "example"; Description: "Example Plugin — developer reference plugin"; Types: full

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut";    GroupDescription: "Additional icons:"
Name: "startup";     Description: "Start automatically with Windows"; GroupDescription: "Startup:"

[Files]
; ── Core application ─────────────────────────────────────────────────────────
Source: "{#PublishDir}\{#AppExeName}";                     DestDir: "{app}"; Components: main; Flags: ignoreversion
Source: "{#PublishDir}\*.dll";                             DestDir: "{app}"; Components: main; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "plugins\*"
Source: "{#PublishDir}\*.json";                            DestDir: "{app}"; Components: main; Flags: ignoreversion
Source: "{#PublishDir}\*.runtimeconfig.json";              DestDir: "{app}"; Components: main; Flags: ignoreversion
Source: "{#PublishDir}\Assets\*";                          DestDir: "{app}\Assets"; Components: main; Flags: ignoreversion recursesubdirs createallsubdirs

; ── Shipped plugins (each optional) ──────────────────────────────────────────
Source: "{#PublishDir}\plugins\OpcUaViewer.Plugin.Groups.dll";    DestDir: "{app}\plugins"; Components: groups;  Flags: ignoreversion skipifsourcedoesntexist
Source: "{#PublishDir}\plugins\OpcUaViewer.Plugin.Document.dll";  DestDir: "{app}\plugins"; Components: docview; Flags: ignoreversion skipifsourcedoesntexist
Source: "{#PublishDir}\plugins\OpcUaViewer.Plugin.Webcam.dll";    DestDir: "{app}\plugins"; Components: webcam;  Flags: ignoreversion skipifsourcedoesntexist
Source: "{#PublishDir}\plugins\OpenCvSharp.dll";                 DestDir: "{app}\plugins"; Components: webcam;  Flags: ignoreversion skipifsourcedoesntexist
Source: "{#PublishDir}\OpenCvSharpExtern.dll";                   DestDir: "{app}";         Components: webcam;  Flags: ignoreversion skipifsourcedoesntexist
Source: "{#PublishDir}\plugins\OpcUaViewer.Plugin.Example.dll";   DestDir: "{app}\plugins"; Components: example; Flags: ignoreversion skipifsourcedoesntexist

[Icons]
Name: "{group}\{#AppName}";       Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; \
    ValueType: string; ValueName: "{#AppName}"; ValueData: """{app}\{#AppExeName}"""; \
    Flags: uninsdeletevalue; Tasks: startup

[UninstallDelete]
; Remove user data only if they confirm — don't auto-delete settings
Type: dirifempty; Name: "{app}\plugins"
