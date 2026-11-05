# Beatrice Wave Link Bridge

A Windows x64 companion utility for using the **existing official Beatrice 2 VST3** in Elgato Wave Link 3, with optional preset switching from the FIFINE D6.

> **Status: integration and control-layer development.** The official Beatrice VST3 already exists upstream. This repository is not intended to recreate its voice-conversion engine or replace the official plugin.

## Existing Beatrice plugin

- Official source: https://github.com/prj-beatrice/beatrice-vst
- It is a native C++20 VST3 plugin with its own audio processor, controller, parameters, and UI.
- Wave Link compatibility must be verified on the target Windows installation; VST3 format support alone is not a compatibility guarantee.
- The official package/runtime and model files must be obtained from their authorized upstream distribution. This repository will not bundle proprietary inference libraries or model weights.

## What this repository adds

1. **Compatibility and installation validation** — confirm the official Beatrice VST3 is discovered and can process audio in Wave Link 3.
2. **Preset/control bridge** — provide reliable ways to select Beatrice presets without interfering with the audio path.
3. **FIFINE D6 integration** — map D6 buttons to supported keyboard shortcuts or another verified control mechanism.
4. **Diagnostics** — show bridge status and report missing plugin/preset/control configuration without touching the audio stream.

## Important design boundary

The official Beatrice VST3 performs voice conversion itself. The bridge must not create a second microphone stream, duplicate audio processing, or insert an unnecessary inference worker. Its control path is separate from the VST3 audio path and must never block audio processing.

## Repository layout

- `docs/ARCHITECTURE.md` — integration boundaries and control design.
- `docs/ROADMAP.md` — implementation stages and acceptance criteria.
- `docs/BUILDING.md` — Windows build and validation plan.
- `docs/UPSTREAM_AUDIT.md` — official plugin, runtime, and license findings.
- `SECURITY.md` — local-control and supply-chain policy.
- `src/` and `tests/` — bridge implementation and tests.

## Non-goals

- Reimplementing Beatrice voice conversion.
- Copying or redistributing the official proprietary inference library, binaries, or model weights.
- Replacing the official Beatrice VST3 with a second VST3 wrapper unless host testing proves that a wrapper is genuinely necessary.
- Claiming zero latency; the official plugin's latency and CPU use must be measured in the actual Wave Link setup.

## Development principles

- Keep the bridge lightweight and separate from the audio callback.
- Prefer documented plugin controls and supported keyboard shortcuts over reverse-engineered memory/process manipulation.
- No network dependency at runtime.
- Validate all local IPC messages and preset identifiers.
- Keep generated binaries, model weights, caches, and personal audio out of Git.

## License

No project-wide license has been granted yet. Third-party components retain their own licenses. This repository does not grant rights to redistribute upstream Beatrice code or assets.
