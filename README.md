# OPC UA Viewer

A Windows desktop application that connects to an OPC UA server, monitors live variable values, and automatically displays the PDF document matching the current product ID.

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

## Distribution

### 1. Build the standalone EXE

```
dotnet publish OpcUaViewer\OpcUaViewer.csproj -r win-x64 -c Release --self-contained -p:PublishSingleFile=true -o publish-standalone
```

### 2. Prepare the target machine

The app uses the **Evergreen** WebView2 Runtime (shipped with Microsoft Edge). If the target machine already has Edge installed, nothing else is needed. Otherwise, download and run the **Evergreen Bootstrapper** (~2 MB) from:

> https://developer.microsoft.com/en-us/microsoft-edge/webview2/?form=MA13LH#download

Run `MicrosoftEdgeWebview2Setup.exe` as administrator once; it installs and self-updates silently from then on.

### 3. Ship the output folder

```
publish-standalone\
  OpcUaViewer.exe          (self-contained, no .NET install needed)
  WebView2Loader.dll
  Microsoft.Web.WebView2.Core.dll
  Microsoft.Web.WebView2.WinForms.dll
  runtimes\
```

No installer required — copy the folder to the target machine and run `OpcUaViewer.exe`.
