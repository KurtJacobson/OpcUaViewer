# Fold Control

Digital production management for Schroeder folding machines. Fold Control connects to the machine control over OPC UA, builds and dispatches production orders, displays the PDF drawing for the part in process, and records production statistics and fault events.

Copyright (c) 2026 MetalForming LLC. All rights reserved. This is proprietary software; see [LICENSE.md](LICENSE.md). A valid license key issued by MetalForming LLC is required to run it.

## Features

- Connects to the machine's OPC UA server and streams live tag values
- Builds Schroeder P3CAM production orders from a part-program library, with CSV import, and drops them into the control's input folder
- Displays the PDF drawing matching the active product ID in an embedded WebView2 viewer
- Tracks machine mode, cycle times, setup times, part counts, and operating hours
- Logs fault start/clear events and part-formed events to a local SQLite database for later MES/ERP write-back
- Plugin architecture: tabs and data sources are loaded from DLLs at startup and can be enabled or disabled in Settings
- RSA-signed license keys with per-feature and time-limited options

## Requirements

- Windows 10 or later (x64)
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (the installer does not bundle it)
- [WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/#download), included with Microsoft Edge

## Usage

1. Launch `FoldControl.exe`
2. Open **Settings** at the bottom of the left nav panel
3. Enter the machine's OPC UA endpoint URL and click **Connect**
4. Configure the **Groups** and **Document Viewer** plugins under Settings, then Plugins, then Configure, to set the CAM and PDF folder paths

## Building

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) or later.

```
dotnet build OpcUaViewer.slnx -c Release
```

The build copies the shipped plugin DLLs into a `plugins\` subfolder next to the output EXE.

## Building the Installer

Requires the .NET SDK and [Inno Setup 6](https://jrsoftware.org/isinfo.php).

```
.\build-installer.ps1
```

The script publishes the app, copies the plugin DLLs into `publish\plugins\`, reads the version from the published EXE, and compiles `installer\OpcUaViewer.iss`. The setup EXE is written to `installer\Output\`.

Pass `-SkipPublish` to recompile the installer without re-publishing the app.

### What the installer does

- Shows the EULA and requires acceptance before installing
- Installs per-user by default with no admin rights; the user can elevate to install system-wide
- Adds a Start Menu shortcut, an optional desktop shortcut, and an optional run-at-startup entry
- Presents a checkbox for each plugin during setup
- Installs `EULA.txt` and `THIRD-PARTY-NOTICES.txt` alongside the application

### Versioning

Versions come from git tags via MinVer. Tag a commit (for example `v1.2.0`) before building a release; the installer file name and the About page pick it up automatically.

## Project Structure

| Project | Description |
|---|---|
| `OpcUaViewer.Core` | Contracts, settings, services (OPC UA client, stats store, fault log), licensing |
| `OpcUaViewer.Theme` | Shared WPF dark theme, referenced by plugins for designer support |
| `OpcUaViewer.Wpf` | Main application (`FoldControl.exe`): shell, plugin loader, Monitor and Settings tabs |
| `OpcUaViewer.Plugin.OpcUa` | OPC UA data source |
| `OpcUaViewer.Plugin.Groups` | Production group and product order management |
| `OpcUaViewer.Plugin.Document` | PDF display driven by the active product ID |
| `OpcUaViewer.Plugin.Stats` | Machine modes, cycle times, and operating hours |
| `OpcUaViewer.Plugin.Webcam` | Live camera feed viewer |
| `OpcUaViewer.Plugin.CsvSource` | Reads Schroeder CSV log files for machines without full OPC UA support |
| `OpcUaViewer.Plugin.Example` | Developer reference plugin |

## Writing a Plugin

1. Create a .NET class library targeting `net8.0-windows` with `UseWPF=true`
2. Reference `OpcUaViewer.Core` and `OpcUaViewer.Theme`
3. Implement `IAppTab` (and optionally `IConfigurableTab`) for a tab, or `IMachineSource` for a data source
4. Drop the compiled DLL into the `plugins\` folder next to `FoldControl.exe`, or into `%LocalAppData%\FoldControl\plugins\`. It appears in the Plugins tab on next launch

## Third-Party Software

Fold Control uses open source components under the MIT, Apache 2.0, and BSD 3-Clause licenses, plus the public-domain SQLite library. The full list and license texts are in [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt).
