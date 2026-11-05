# Beatrice Wave Link Bridge

A Windows x64 project to integrate local Beatrice voice conversion with Elgato Wave Link 3 as a VST3 effect, with optional preset control from the FIFINE D6.

> **Status: planning and architecture validation.** This repository does not yet contain a working VST3 binary or a verified Beatrice runtime integration.

## Goals

- Run as a VST3 effect that Wave Link 3 can load.
- Keep real-time audio processing safe: no model loading, disk I/O, blocking IPC, or unbounded work on the audio callback.
- Run Beatrice inference outside the plugin's real-time audio thread.
- Provide predictable bypass, latency reporting, silence/startup handling, and recovery if the inference worker stops.
- Expose stable preset/control actions that can later be mapped to FIFINE D6 buttons.
- Keep model files and third-party binaries out of source control unless their licenses and provenance are verified.

## Proposed architecture

1. **VST3 host plugin** — Wave Link-facing audio interface, parameters, bypass, and bounded audio queues.
2. **Local inference worker** — isolated process responsible for Beatrice model/runtime lifecycle and inference.
3. **Control bridge** — local authenticated IPC for preset selection and status; separate from the audio data path.
4. **Preset layer** — versioned user presets and stable IDs suitable for D6 mapping.

The exact audio transport, buffering strategy, and Beatrice API will be selected only after inspecting the target Beatrice implementation and measuring its latency/throughput.

## Repository layout

- `docs/ARCHITECTURE.md` — system boundaries and real-time constraints.
- `docs/ROADMAP.md` — staged implementation and acceptance criteria.
- `docs/BUILDING.md` — planned Windows build environment and validation.
- `SECURITY.md` — reporting and supply-chain policy.
- `src/` — project source (to be added after the API and SDK choices are validated).
- `tests/` — automated tests (to be added with the first testable components).

## Current non-goals

- No redistribution of Beatrice model weights, DLLs, or prebuilt plugins until exact licenses and redistribution terms are checked.
- No claim that upstream Beatrice VST3 binaries are compatible with Wave Link 3 without host validation.
- No guarantee of zero latency. Voice conversion necessarily introduces buffering and inference latency; this must be measured.

## Development principles

- C++ for the VST3 host-facing component unless validation shows a compelling reason otherwise.
- No network dependency at runtime.
- Fail closed to dry/bypass audio on worker failure.
- Never block or allocate unbounded memory in the audio processing callback.
- Keep generated binaries, model weights, caches, and personal audio out of Git.

## License

No project-wide license has been granted yet. Third-party components retain their own licenses. Do not assume this repository grants rights to redistribute upstream Beatrice code or assets.
