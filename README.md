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
- The bundled `WebView2Runtime\` folder next to the EXE (see [Distribution](#distribution))
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

### 2. Add the WebView2 fixed-version runtime

The app uses a **fixed-version** WebView2 runtime bundled alongside the EXE so the target machine does not need Edge or the Evergreen WebView2 Runtime installed.

1. Download the **Fixed Version** x64 runtime (version `1.0.4022.49`) from the [Microsoft WebView2 page](https://developer.microsoft.com/microsoft-edge/webview2/)
2. Extract it into a folder named `WebView2Runtime\` placed next to `OpcUaViewer.exe`

### 3. Ship the output folder

```
publish-standalone\
  OpcUaViewer.exe          (~121 MB, self-contained)
  WebView2Loader.dll
  WebView2Runtime\         (~150 MB, fixed WebView2 runtime)
  runtimes\
  OpcUaViewer.dll.config
```

No installer required — copy the folder to the target machine and run `OpcUaViewer.exe`.
