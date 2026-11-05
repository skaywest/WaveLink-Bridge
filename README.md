# Beatrice Wave Link Bridge

A Windows x64 control helper and audio-routing integration project for using the **112-voice Beatrice for Windows app** with Elgato Wave Link 3 and FIFINE D6.

## Correct Beatrice component

The 112-voice product is the standalone Windows app:
https://github.com/Project-Beatrice-V2/Beatrice-voicechanger-windows

It includes three voice sets (JVS 100, Official 4, Classic Old TTS 8), runs the Beatrice engine locally, and exposes a documented local control API at 127.0.0.1:5005.

The separate native VST3 source at https://github.com/prj-beatrice/beatrice-vst is not, by itself, proof that the same 112 voices are packaged and selectable inside Wave Link. This project does not assume that.

## Integration design

Initial route to validate:

FIFINE K688 -> Beatrice standalone app -> VB-CABLE -> Wave Link 3 input channel

Beatrice performs conversion. Wave Link receives the converted virtual-cable output as an input. This is audio-device routing, not a native Wave Link VST insert.

The companion helper in src/BeatriceBridge:
- registers configurable Windows global hotkeys;
- selects a voice from JVS, Official Model, or Classic Old TTS;
- waits for asynchronous model-set changes;
- validates speaker indices using Beatrice /status;
- toggles Beatrice LIVE/BYPASS;
- communicates only with the loopback API at 127.0.0.1:5005.

FIFINE D6 buttons can trigger these actions if the installed D6 software supports assigning keyboard shortcuts. That capability still needs confirmation on the user's PC.

## Build

Requires Windows 10/11 x64 and .NET 8 SDK.

    dotnet publish .\src\BeatriceBridge\BeatriceBridge.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

Copy src/BeatriceBridge/bridge.example.json beside the published executable as bridge.json and customize the shortcuts.

See docs/BUILDING.md for routing and configuration details, docs/ARCHITECTURE.md for the signal/control paths, and docs/ROADMAP.md for remaining validation.

## Scope and licensing

This repository does not bundle Beatrice, proprietary inference components, voice models, or VB-CABLE. Obtain each component from its authorized upstream source and follow its own license. The JVS corpus is identified upstream as non-commercial only.

No second inference worker, DLL injection, process patching, or audio capture is implemented here.
