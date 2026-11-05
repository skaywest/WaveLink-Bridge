# Beatrice Wave Link Bridge

A Windows x64 control helper and audio-routing integration project for using the **112-voice Beatrice for Windows app** with Elgato Wave Link 3 and FIFINE D6.

## Correct Beatrice component

The 112-voice product is the standalone Windows app:
https://github.com/Project-Beatrice-V2/Beatrice-voicechanger-windows

It includes three voice sets (JVS 100, Official 4, Classic Old TTS 8), runs the Beatrice engine locally, and exposes a documented local control API at 127.0.0.1:5005.

The separate native VST3 source at https://github.com/prj-beatrice/beatrice-vst is not, by itself, proof that the same 112 voices are packaged and selectable inside Wave Link. This project does not assume that.

## Integration design

No third-party VB-CABLE dependency is assumed. The user's existing Wave Link 3 virtual audio devices and routing should be used wherever they can carry the Beatrice signal.

Confirmed existing signal order:

FIFINE K688 (XLR) -> MaonoCaster G1 Neo -> Wave Link 3 microphone input/channel -> the user's existing downstream Mixes/output processing and routing.

The screenshot confirms the FabFilter/TDR/LoudMax effects are attached in the Wave Link 3 Mixes area over the added microphone source. Do not move these effects or assume they run before Wave Link. Beatrice is treated as its own virtual audio device/endpoint, as identified by the user. Its exact insertion/routing point relative to Wave Link must be established from the actual Windows endpoint names and Wave Link routing UI before prescribing a new signal path. Do not redraw the user's confirmed K688 -> G1 Neo -> Wave Link order.

The Beatrice virtual endpoint and Wave Link's virtual endpoints must be distinguished by their exact Windows device names. Do not call Beatrice's endpoint a Wave Link virtual cable, and do not assume VB-CABLE is required. Avoid feedback by never routing a Wave Link mix back into an input included in that same mix.

Beatrice performs conversion; Wave Link remains the user's main mixer and plugin host. This is audio-device/application routing, not a claim that the standalone app is a native Wave Link VST insert.


## Build

Requires Windows 10/11 x64 and .NET 8 SDK.

    dotnet publish .\src\BeatriceBridge\BeatriceBridge.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

The published package includes bridge.example.json. The helper can start with this example automatically; to customize shortcuts, copy it beside the executable as bridge.json and edit that file. bridge.json takes priority when present.

See docs/BUILDING.md for routing and configuration details, docs/ARCHITECTURE.md for the signal/control paths, and docs/ROADMAP.md for remaining validation.

## Scope and licensing

This repository does not bundle Beatrice, proprietary inference components, voice models, or VB-CABLE. Obtain each component from its authorized upstream source and follow its own license. The JVS corpus is identified upstream as non-commercial only.

No second inference worker, DLL injection, process patching, or audio capture is implemented here.
