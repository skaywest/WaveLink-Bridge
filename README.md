# Beatrice Wave Link Bridge

A Windows x64 control helper and audio-routing integration project for using the **112-voice Beatrice for Windows app** with Elgato Wave Link 3 and FIFINE D6.

## Correct Beatrice component

The 112-voice product is the standalone Windows app:
https://github.com/Project-Beatrice-V2/Beatrice-voicechanger-windows

It includes three voice sets (JVS 100, Official 4, Classic Old TTS 8), runs the Beatrice engine locally, and exposes a documented local control API at 127.0.0.1:5005.

The separate native VST3 source at https://github.com/prj-beatrice/beatrice-vst is not, by itself, proof that the same 112 voices are packaged and selectable inside Wave Link. This project does not assume that.

## Integration design

No third-party VB-CABLE dependency is assumed. The user's existing Wave Link 3 virtual audio devices and routing should be used wherever they can carry the Beatrice signal.

Target signal order:

FIFINE K688 -> Beatrice AI conversion -> Wave Link 3 input/routing -> existing Wave Link plugin chain (FabFilter, LoudMax, etc.) -> existing Wave Link MicrophoneFX/output

The exact handoff from Beatrice back into Wave Link must be verified against the devices exposed by the installed Wave Link 3 version. Do not create a loop by feeding a Wave Link output back into an input that is mixed into the same output. Do not install VB-CABLE unless testing proves Wave Link cannot accept Beatrice's output through its existing devices.

Beatrice performs conversion; Wave Link remains the user's main mixer and plugin host. This is audio-device/application routing, not a claim that the standalone app is a native Wave Link VST insert.


## Build

Requires Windows 10/11 x64 and .NET 8 SDK.

    dotnet publish .\src\BeatriceBridge\BeatriceBridge.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

Copy src/BeatriceBridge/bridge.example.json beside the published executable as bridge.json and customize the shortcuts.

See docs/BUILDING.md for routing and configuration details, docs/ARCHITECTURE.md for the signal/control paths, and docs/ROADMAP.md for remaining validation.

## Scope and licensing

This repository does not bundle Beatrice, proprietary inference components, voice models, or VB-CABLE. Obtain each component from its authorized upstream source and follow its own license. The JVS corpus is identified upstream as non-commercial only.

No second inference worker, DLL injection, process patching, or audio capture is implemented here.
