# Building

## Prerequisites

- Windows 10/11 x64
- .NET 8 SDK
- Official Beatrice for Windows application installed and running

## Build the helper

From the repository root in PowerShell:

    dotnet publish .\src\BeatriceBridge\BeatriceBridge.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

The executable is written under:

src/BeatriceBridge/bin/Release/net8.0-windows/win-x64/publish/BeatriceBridge.exe

Copy bridge.example.json beside the executable as bridge.json and edit the mappings. Start Beatrice first; the helper connects to its local API at 127.0.0.1:5005.

## Hotkey format

Supported combinations are Ctrl/Alt/Shift plus F1-F12 or A-Z. Example:

    {
      "hotkeys": {
        "Ctrl+Alt+F1": { "model": "jvs", "speakerIndex": 0 },
        "Ctrl+Alt+F2": { "model": "jvs", "speakerIndex": 1 },
        "Ctrl+Alt+F3": { "model": "official_1", "speakerIndex": 0 },
        "Ctrl+Alt+F4": { "model": "old_tts", "speakerIndex": 0 },
        "Ctrl+Alt+V": { "toggleBypass": true }
      }
    }

speakerIndex is zero-based and must be less than the active model's n_speakers. The helper checks /status before sending a selection. Model changes are asynchronous upstream; the helper waits for the requested model to appear in /status.

## FIFINE D6

In the D6 software, assign each physical button a keyboard shortcut matching bridge.json. FIFINE's official Help Center documents the Shortcut Keys tool: drag it to a blank D6 key, click the shortcut field, and press the desired key combination. In the installed D6 app, assign combinations matching bridge.json. UI labels may vary by software version.

## Audio routing checklist

1. Preserve the user's current Wave Link 3 device and plugin configuration.
2. Choose FIFINE K688 as Beatrice's input.
3. Inspect Beatrice's available output devices and Wave Link 3's actual virtual endpoints.
4. Route Beatrice output into a Wave Link input path that is not fed back into Beatrice or into itself.
5. Keep the existing FabFilter/LoudMax chain downstream of the converted voice.
6. Avoid mixing the raw K688 a second time unless the user explicitly wants a dry signal.
7. Keep Beatrice Hear Yourself off initially; monitor through the existing Wave Link output.

Do not install VB-CABLE unless the native Wave Link endpoints cannot receive Beatrice's output cleanly. The exact routing must be verified on the target PC before documenting a fixed device selection.


## Limitations

- The helper does not embed or redistribute Beatrice, its engine, voice models, or VB-CABLE.
- The helper controls the standalone app; it does not turn it into a native Wave Link VST3 insert.
- Build and runtime validation require Windows; source code alone cannot prove the user's Wave Link routing or D6 key output.
