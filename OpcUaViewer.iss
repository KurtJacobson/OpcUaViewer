#ifndef MyAppVersion
  #define MyAppVersion "0.0.0-dev"
#endif

[Setup]
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

[Code]
function WebView2IsInstalled: Boolean;
var
  Version: String;
begin
  if RegQueryStringValue(HKEY_LOCAL_MACHINE,
      'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}',
      'pv', Version) then begin Result := True; Exit; end;
  if RegQueryStringValue(HKEY_LOCAL_MACHINE,
      'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}',
      'pv', Version) then begin Result := True; Exit; end;
  Result := RegQueryStringValue(HKEY_CURRENT_USER,
      'Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}',
      'pv', Version);
end;

function InitializeSetup: Boolean;
begin
  if not WebView2IsInstalled then
  begin
    MsgBox(
      'Microsoft Edge WebView2 Runtime is required but was not found.' + #13#10 + #13#10 +
      'Please install the Evergreen Bootstrapper from:' + #13#10 +
      'https://developer.microsoft.com/microsoft-edge/webview2/' + #13#10 + #13#10 +
      'Then retry this installer.',
      mbError, MB_OK);
    Result := False;
  end
  else
    Result := True;
end;
