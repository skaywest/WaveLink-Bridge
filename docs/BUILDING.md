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

The publish folder contains bridge.example.json. The helper can start with this example automatically, so renaming is not required for a first launch. To customize mappings, copy it beside the executable as bridge.json and edit that file; bridge.json takes priority. Start Beatrice first; the helper connects to its local API at 127.0.0.1:5005.

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
2. Preserve the confirmed physical chain: FIFINE K688 (XLR) -> MaonoCaster G1 Neo -> Wave Link 3.
3. Inspect Beatrice's own virtual audio endpoint and record its exact Windows device name and direction. Do not label it as a Wave Link virtual device.
4. Inspect Wave Link 3's actual channel and Mixes assignments before choosing any handoff. The user has confirmed the FabFilter/TDR/LoudMax effects are attached in the Mixes area over the added microphone source; preserve their current placement and settings.
5. Determine a supported route that uses Beatrice's virtual endpoint without feeding a Wave Link mix back into itself or duplicating the raw mic.
6. Keep Beatrice monitoring off initially; monitor from the established Wave Link output.
7. Do not change the user's existing routing until the endpoint path has been verified on the target PC.

Do not install VB-CABLE unless the native Wave Link endpoints cannot receive Beatrice's output cleanly. The exact routing must be verified on the target PC before documenting a fixed device selection.


## Limitations

- The helper does not embed or redistribute Beatrice, its engine, voice models, or VB-CABLE.
- The helper controls the standalone app; it does not turn it into a native Wave Link VST3 insert.
- Build and runtime validation require Windows; source code alone cannot prove the user's Wave Link routing or D6 key output.


## CI smoke test

The Windows workflow publishes the self-contained x64 executable, verifies the example configuration, and starts the published executable once without Beatrice running. Exit code 3 is expected in that isolated CI test because the local Beatrice API is intentionally unavailable; this confirms the app parsed its configuration and reached the API connection check. It does not replace an audio or live voice-switching test.
