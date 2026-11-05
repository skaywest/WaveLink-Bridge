# Architecture

## Goal

Use the existing official Beatrice 2 VST3 in Wave Link 3 and add a separate, lightweight control layer for preset switching, especially through FIFINE D6. Do not rebuild the Beatrice audio engine.

## 1. Existing Beatrice VST3

Upstream: https://github.com/prj-beatrice/beatrice-vst

The upstream repository contains a native C++20 VST3 plugin with an audio processor, controller, parameter schema, editor, and its own inference integration. The official plugin is the audio-processing component. This project should first test that exact plugin in Wave Link 3 before designing any replacement.

The upstream processor accepts mono/stereo buses and processes float32 audio. Its source reports latency through the VST3 processor interface when its latency-reporting parameter is enabled. Actual behavior, latency, and stability must be measured in Wave Link 3.

## 2. Wave Link compatibility validation

Verify on the target Windows 11 system:
- the official Beatrice VST3 package is installed in a location Wave Link 3 scans;
- Wave Link detects it and loads its editor;
- microphone input reaches the plugin and converted audio reaches the intended Wave Link output;
- sample rate, channel layout, block size, latency reporting, and CPU use are acceptable;
- presets and parameter state survive reload/restart as expected.

Do not assume that any VST3 plugin will necessarily work correctly in every host.

## 3. Control bridge

The bridge is outside the audio signal path. Its responsibilities:
- map a small set of user actions to supported Beatrice preset/parameter controls;
- provide stable preset names/IDs and user-configurable D6 mappings;
- show whether Beatrice and the control path are available;
- validate configuration and reject unknown actions safely.

Preferred control order:
1. built-in Beatrice preset/state functionality, if it supports the required operation;
2. documented keyboard shortcuts or host/plugin automation exposed by Wave Link;
3. a documented local control API, if upstream provides one;
4. only then consider another integration method, after compatibility and security review.

Do not use undocumented memory writes, DLL injection, process patching, or fragile UI-coordinate automation as the default design.

## 4. FIFINE D6

First verify what the installed D6 software can emit (keyboard shortcuts, media keys, hotkeys, or application actions). Map each physical button to a stable bridge action using the supported mechanism. Do not assume a direct D6 device protocol or undocumented SDK.

Rapid presses must be debounced or safely serialized. Missing presets and unavailable Beatrice instances must produce a visible error rather than silently switching to an unintended preset.

## 5. Security and reliability

- Keep control local to the current Windows user/session.
- Validate message size, schema, parameter ranges, and preset IDs.
- Do not accept arbitrary shell commands or arbitrary file paths.
- Do not log microphone audio or voice samples.
- Never interfere with the Beatrice audio-processing thread.
- Provide clear status and recover gracefully if the bridge exits.

## Decisions still open

- Whether Beatrice exposes a usable preset-selection shortcut/API.
- Whether Wave Link 3 exposes plugin parameter automation usable by the bridge.
- Exact FIFINE D6 hotkey capabilities on the user's installed software version.
- Whether a small background helper is sufficient or a VST3 wrapper is genuinely required.

These questions should be answered by inspecting the existing plugin and testing it in Wave Link before adding unnecessary audio infrastructure.
