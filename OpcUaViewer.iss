#ifndef MyAppVersion
  #define MyAppVersion "0.0.0-dev"
#endif

[Setup]
AppId={{A3F7C2B1-4D8E-4F9A-B6C3-2E1D5A7F8B9C}
AppName=OPC UA Viewer
AppVersion={#MyAppVersion}
AppPublisher=MetalForming LLC
DefaultDirName={localappdata}\OPC UA Viewer
DefaultGroupName=OPC UA Viewer
OutputBaseFilename=OpcUaViewer-Setup
OutputDir=installer
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
SetupIconFile=OpcUaViewer\Resources\app.ico
UninstallDisplayIcon={app}\OpcUaViewer.exe
PrivilegesRequired=lowest
CloseApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional options:"
Name: "startup"; Description: "Launch OPC UA Viewer when Windows &starts"; GroupDescription: "Additional options:"

[Files]
Source: "OpcUaViewer\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "Application Files\*"

[Icons]
Name: "{userprograms}\OPC UA Viewer"; Filename: "{app}\OpcUaViewer.exe"
Name: "{userdesktop}\OPC UA Viewer"; Filename: "{app}\OpcUaViewer.exe"; Tasks: desktopicon
Name: "{userstartup}\OPC UA Viewer"; Filename: "{app}\OpcUaViewer.exe"; Tasks: startup

[Run]
Filename: "{app}\OpcUaViewer.exe"; Description: "Launch OPC UA Viewer"; Flags: nowait postinstall skipifsilent

