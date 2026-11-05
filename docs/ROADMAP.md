# Roadmap

## Stage 0 — Identify the correct 112-voice engine

- [x] Identify the Windows app that bundles the 112 voices.
- [x] Inspect its documented local API and voice-set selection.
- [x] Confirm voice selection uses speaker_index within the currently loaded set.
- [ ] Install and test the upstream Beatrice app on the target Windows 11 PC.
- [ ] Verify model counts: JVS 100, Official 4, Classic Old TTS 8.

## Stage 1 — Audio routing into Wave Link 3

- [ ] Set Beatrice input to FIFINE K688.
- [ ] Set Beatrice output to VB-CABLE Input.
- [ ] Add VB-CABLE Output as a Wave Link 3 input channel.
- [ ] Confirm the raw mic is not mixed a second time.
- [ ] Measure latency and check for feedback, crackling, and channel mismatch.

## Stage 2 — Control bridge

- [x] Implement a Windows helper for the documented Beatrice local API.
- [x] Add configurable global hotkeys for voice selection and LIVE/BYPASS.
- [x] Validate voice indices against /status and serialize model changes.
- [ ] Build and run the helper on Windows 11.
- [ ] Confirm FIFINE D6 can emit the configured hotkeys.
- [ ] Map physical D6 buttons to the selected hotkeys.

## Stage 3 — Hardening and packaging

- [ ] Add automated tests for config validation, hotkey parsing, model switching, and invalid indices.
- [ ] Add tray/status UI and recovery diagnostics if useful.
- [ ] Verify clean startup/shutdown and repeated rapid voice changes.
- [ ] Run a long Wave Link 3 session and measure CPU/latency.
- [ ] Produce a reproducible Windows x64 package and installation guide.

## Acceptance criteria

- Converted Beatrice output is heard through the intended Wave Link 3 channel without duplicate raw mic.
- Every built-in voice set can be selected.
- A D6 button changes to its assigned voice reliably.
- LIVE/BYPASS toggles without stopping audio.
- No second inference engine, DLL injection, or undocumented process manipulation is used.
