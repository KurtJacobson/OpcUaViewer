# OPC UA Viewer

A Windows desktop application that connects to an OPC UA server, monitors live variable values, and automatically displays the PDF document matching the current product ID.

## Download

[**Download latest installer**](https://github.com/kurtjacobson/OpcUaViewer-releases/releases/latest/download/OpcUaViewer-Setup.exe)

## Features

- Connects to any OPC UA server endpoint (anonymous / no security)
- Browses a configurable node path and displays all discovered variables in a live-updating grid
- Watches for a variable whose name contains `ProductId` and automatically opens the matching PDF from a configured folder
- Embeds a WebView2-based PDF viewer — no external PDF reader required
- Persists the PDF folder path between sessions

## Requirements

- Windows 7 or later (x64)
- [WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/?form=MA13LH#download) installed on the target machine (included with Microsoft Edge; or install the **Evergreen Bootstrapper** from that page)
- .NET 9 runtime is **not** required — the EXE is self-contained

## Usage

1. Launch `OpcUaViewer.exe`
2. Enter your OPC UA server endpoint URL (e.g. `opc.tcp://192.168.1.10:4840`)
3. Click **Connect** — the app browses the configured path and begins monitoring all variables it finds
4. Set the **PDF Folder** to the directory containing your product PDFs
5. When the `ProductId` variable changes, the app looks for `<productId>.pdf` in that folder and displays it automatically

The PDF folder setting is saved and restored on next launch.

## Configuration

Two constants at the top of `Form1.cs` control the OPC UA browsing behavior:

| Constant | Default | Description |
|---|---|---|
| `MonitoringFolderPath` | `4:PLC/6:Modules/6:::/6:Global PV/6:Monitoring` | Path under `/Root/Objects` to browse, expressed as `namespaceIndex:BrowseName` segments separated by `/` |
| `ProductIdNameMatch` | `ProductId` | Any monitored variable whose name contains this string (case-insensitive) drives the PDF display |

Change these to match your server's address space and rebuild.

## Building

Requires the [.NET 9 SDK](https://dotnet.microsoft.com/download).

```
dotnet build OpcUaViewer\OpcUaViewer.csproj -c Release
```

## Building the Installer

Requires the [.NET 9 SDK](https://dotnet.microsoft.com/download) and [Inno Setup 6](https://jrsoftware.org/isinfo.php) (free).

```
.\build-installer.ps1
```

The script publishes the app and compiles the installer in one step. The installer is output to `installer\OpcUaViewer-Setup.exe`.

The version is derived from git automatically:
- If HEAD is exactly on a tag and the working tree is clean → uses the tag (e.g. `1.2.0`)
- Otherwise → appends the short commit hash (e.g. `1.2.0-a3f9c1d`)

Pass `-SkipPublish` to recompile the installer without republishing the app.

The installer will:
- Install to `Program Files\OPC UA Viewer\`
- Add a Start Menu shortcut
- Optionally add a Windows startup entry (user's choice during install)
- Check for the WebView2 Runtime and show an error with a download link if missing

## Distribution

The app uses the **Evergreen** WebView2 Runtime (shipped with Microsoft Edge). If the target machine already has Edge installed, nothing else is needed. Otherwise, download and run the **Evergreen Bootstrapper** (~2 MB) from:

> https://developer.microsoft.com/en-us/microsoft-edge/webview2/?form=MA13LH#download
