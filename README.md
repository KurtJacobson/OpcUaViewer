# OPC UA Viewer

A Windows desktop application that connects to an OPC UA server, monitors live variable values, and automatically displays the PDF document matching the current product ID.

## Download

[**Download latest installer**](https://github.com/kurtjacobson/OpcUaViewer-releases/releases/latest/download/OpcUaViewer-Setup.exe)

## Features

- Connects to any OPC UA server endpoint (anonymous / no security)
- Browses a configurable node path and displays all discovered variables in a live-updating grid
- Watches for a variable whose name contains `ProductId` and automatically opens the matching PDF from a configured folder
- Embeds a WebView2-based PDF viewer — no external PDF reader required
- Plugin architecture — tabs are loaded from DLLs at startup; plugins can be enabled/disabled in Settings
- Settings persist between sessions

## Requirements

- Windows 10 or later (x64)
- [WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/?form=MA13LH#download) (included with Microsoft Edge; or install the Evergreen Bootstrapper from that page)
- .NET runtime is **not** required when installed via the setup EXE — the app is self-contained

## Usage

1. Launch `OpcUaViewer.exe`
2. Open **Settings** (bottom of the left nav panel)
3. Enter your OPC UA server endpoint URL and click **Connect**
4. Use the **Groups** and **Document Viewer** plugins (Settings → Plugins → Configure) to set folder paths

## Building

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```
dotnet build OpcUaViewer.slnx -c Release
```

The build automatically copies the shipped plugin DLLs (`Groups`, `Document Viewer`) into a `plugins\` subfolder next to the output EXE.

## Building the Installer

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Inno Setup 6](https://jrsoftware.org/isinfo.php) (free).

```
.\build-installer.ps1
```

The script publishes the app, builds and copies the plugin DLLs into `publish\plugins\`, then compiles the installer. The EXE is written to `Installer\Output\`.

Pass `-SkipPublish` to recompile the installer without re-publishing the app.

### What the installer does

- Installs to `%LocalAppData%\Programs\OPC UA Viewer` by default (no admin required); users can elevate to install system-wide
- Adds a Start Menu shortcut and an optional desktop shortcut
- Presents checkboxes for each plugin during setup — Groups and Document Viewer are checked by default

### Versioning

Update `#define AppVersion` near the top of `Installer\OpcUaViewer.iss` before building a release.

## Project Structure

| Project | Description |
|---|---|
| `OpcUaViewer.Core` | Contracts (`IAppTab`, `IConfigurableTab`), settings, services |
| `OpcUaViewer.Theme` | Shared WPF dark theme (`DarkTheme.xaml`) — referenced by plugins for designer support |
| `OpcUaViewer.Wpf` | Main WPF application (shell, plugin loader, Monitor tab, Settings tab) |
| `OpcUaViewer.Plugin.Groups` | Groups plugin — production group and product order management |
| `OpcUaViewer.Plugin.Document` | Document Viewer plugin — PDF display driven by OPC UA product ID |
| `OpcUaViewer.Plugin.Example` | Developer reference plugin |

## Writing a Plugin

1. Create a .NET class library targeting `net10.0-windows` with `UseWPF=true`
2. Reference `OpcUaViewer.Core` and `OpcUaViewer.Theme`
3. Implement `IAppTab` (and optionally `IConfigurableTab`) on one or more classes
4. Drop the compiled DLL into `%LocalAppData%\OpcUaViewer\plugins\` — it will appear in the Plugins tab on next launch

## Distribution

The app uses the **Evergreen** WebView2 Runtime (shipped with Microsoft Edge). If Edge is already installed on the target machine, nothing else is needed. Otherwise, download and run the Evergreen Bootstrapper (~2 MB) from:

> https://developer.microsoft.com/en-us/microsoft-edge/webview2/?form=MA13LH#download
