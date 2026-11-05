# Roadmap

## Stage 0 — Existing plugin verification

- [x] Identify the official Beatrice 2 VST3 repository.
- [x] Confirm that upstream contains a native VST3 processor, controller, parameter schema, and editor.
- [x] Review the bundled inference-library and model licensing notices.
- [ ] Obtain/install the official Beatrice VST3 package from its authorized source.
- [ ] Verify discovery, loading, and audio processing in Wave Link 3.
- [ ] Record plugin version, host behavior, sample rate, channel layout, latency, and CPU use.

**Exit criteria:** the official plugin is confirmed working (or concrete incompatibilities are documented) in the user's Wave Link 3 setup.

## Stage 1 — Preset control feasibility

- [ ] Inspect the plugin's existing preset/state behavior and exposed parameters.
- [ ] Determine whether preset selection can be triggered through supported shortcuts or Wave Link parameter automation.
- [ ] Verify FIFINE D6 button output capabilities and mapping options.
- [ ] Choose the least invasive reliable control path.

**Exit criteria:** one physical D6 button can reliably trigger one intended Beatrice action without disrupting audio.

## Stage 2 — Build the control bridge

- [ ] Implement a small Windows helper only for functionality not already provided by Beatrice/Wave Link/D6 software.
- [ ] Add stable preset IDs and validated user configuration.
- [ ] Add status reporting and useful error messages.
- [ ] Add safe handling for rapid repeated presses, missing presets, and unavailable plugin/host.

**Exit criteria:** preset switching is repeatable and does not interfere with the audio signal path.

## Stage 3 — Tests and packaging

- [ ] Automated tests for configuration parsing, action mapping, debounce, and error handling.
- [ ] Verify startup/shutdown and recovery behavior.
- [ ] Test a long session in Wave Link 3 with repeated preset changes.
- [ ] Reproducible Windows x64 build and installation instructions.
- [ ] Dependency/license inventory and security review of distributable artifacts.

**Exit criteria:** a tested companion utility with documented compatibility and rollback instructions.

## Explicitly out of scope unless testing proves it necessary

- Reimplementing Beatrice inference.
- Building a replacement voice-conversion VST3.
- Creating a separate audio streaming worker or second microphone capture path.
