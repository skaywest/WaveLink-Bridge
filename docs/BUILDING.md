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

In the D6 software, assign each physical button a keyboard shortcut matching bridge.json. Exact steps depend on the installed D6 software version and must be confirmed on the user's PC. If the D6 software cannot emit arbitrary shortcuts, revise the mapping only after verifying its actual capabilities.

## Audio routing checklist

1. In Beatrice, choose FIFINE K688 as input.
2. Choose CABLE Input (VB-Audio Virtual Cable) as Beatrice output.
3. In Wave Link 3, add CABLE Output as an input channel.
4. Do not add raw FIFINE K688 to the same outgoing mix unless intentionally desired.
5. Keep Beatrice Hear Yourself off initially; monitor from Wave Link.

## Limitations

- The helper does not embed or redistribute Beatrice, its engine, voice models, or VB-CABLE.
- The helper controls the standalone app; it does not turn it into a native Wave Link VST3 insert.
- Build and runtime validation require Windows; source code alone cannot prove the user's Wave Link routing or D6 key output.
