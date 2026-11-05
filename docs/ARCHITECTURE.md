# Architecture

## Correct integration target

The 112-voice product is the Beatrice for Windows standalone application, not the separate prj-beatrice/beatrice-vst source repository by itself.

Upstream application: https://github.com/Project-Beatrice-V2/Beatrice-voicechanger-windows

The Windows application bundles the Beatrice 2.0 engine and three voice sets:
- JVS Corpus: 100 speakers
- Official Model: 4 speakers
- Classic Old TTS: 8 speakers

It owns microphone capture and conversion. Its local backend listens on 127.0.0.1:5005 and exposes /status, /devices, /set_config, and /set_model. /status reports current_model, speaker_index, and n_speakers. /set_config?speaker_index=N selects a voice in the loaded set; /set_model?model=jvs|official_1|old_tts changes the set asynchronously.

## Audio path into Wave Link 3

The user's existing Wave Link 3 setup is the primary mixer and already hosts the downstream processing chain (including FabFilter and LoudMax). Do not assume a separate VB-CABLE is required.

Desired signal order:

FIFINE K688 -> Beatrice input/conversion -> Beatrice output -> a supported Wave Link 3 input route -> existing Wave Link processing chain -> existing MicrophoneFX/output.

The exact Beatrice-to-Wave-Link handoff is still an integration question. Inspect the actual Wave Link 3 device endpoints and supported application/device routing on the target PC before choosing the endpoint. Reuse Wave Link's own virtual devices if they can receive the Beatrice output without a feedback loop. Do not route a Wave Link output back into a channel that is included in that same output mix.

Keep the raw FIFINE K688 out of the final mix unless the user intentionally wants a dry mic alongside the converted voice. Keep Beatrice monitoring off initially and monitor from the existing Wave Link path to avoid doubled monitoring.

VB-CABLE is an optional fallback only if Wave Link's own endpoints cannot provide a stable return path. Do not make it a prerequisite or install it pre-emptively. This is audio-device routing, not a native Wave Link VST insert.

Do not load a second Beatrice inference engine in Wave Link while the standalone app is already converting the microphone.


## Control bridge

src/BeatriceBridge is a Windows helper outside the audio path. It registers configurable global keyboard shortcuts, talks only to http://127.0.0.1:5005, reads /status before changing a voice, waits for asynchronous model loading, validates voice indices against the active model, toggles LIVE/BYPASS, and serializes requests.

FIFINE's official help article documents a Shortcut Keys action that records a keyboard combination and assigns it to a D6 button. Therefore the intended mapping path is supported by the vendor; confirm the user's installed software exposes the same option. No direct device SDK is needed.

## Security

The upstream API is unauthenticated HTTP but binds to loopback. The helper must never expose it on the LAN or accept arbitrary URLs, shell commands, or file paths. Only documented model names and in-range voice indices are allowed. The helper does not record audio or inject into Beatrice/Wave Link processes.

## Separate VST3 project

prj-beatrice/beatrice-vst is a separate native VST3 codebase. It is not proof that the standalone app's 112 voices are available as a Wave Link insert. Native VST3 integration is out of scope unless host testing demonstrates a need and licensing is confirmed.
