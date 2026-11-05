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

Initial supported topology to validate on the user's PC:

FIFINE K688 -> Beatrice input -> Beatrice conversion -> VB-CABLE Input -> VB-CABLE Output -> Wave Link input channel -> Wave Link mix/output

- Beatrice output is routed to CABLE Input.
- Wave Link receives CABLE Output as an input device/channel.
- Do not capture the physical FIFINE K688 twice in Wave Link unless the user intentionally wants both raw and converted signals.
- Disable Beatrice Hear Yourself initially; monitor from Wave Link/headphones to avoid duplicate monitoring.
- This is device routing, not a native Wave Link VST insert. Test latency, sample rate, channels, and feedback.

Do not load a second Beatrice inference engine in Wave Link while the standalone app is already converting the microphone.

## Control bridge

src/BeatriceBridge is a Windows helper outside the audio path. It registers configurable global keyboard shortcuts, talks only to http://127.0.0.1:5005, reads /status before changing a voice, waits for asynchronous model loading, validates voice indices against the active model, toggles LIVE/BYPASS, and serializes requests.

FIFINE's official help article documents a Shortcut Keys action that records a keyboard combination and assigns it to a D6 button. Therefore the intended mapping path is supported by the vendor; confirm the user's installed software exposes the same option. No direct device SDK is needed.

## Security

The upstream API is unauthenticated HTTP but binds to loopback. The helper must never expose it on the LAN or accept arbitrary URLs, shell commands, or file paths. Only documented model names and in-range voice indices are allowed. The helper does not record audio or inject into Beatrice/Wave Link processes.

## Separate VST3 project

prj-beatrice/beatrice-vst is a separate native VST3 codebase. It is not proof that the standalone app's 112 voices are available as a Wave Link insert. Native VST3 integration is out of scope unless host testing demonstrates a need and licensing is confirmed.
