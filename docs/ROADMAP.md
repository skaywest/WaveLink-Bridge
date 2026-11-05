# Roadmap

## Stage 0 — Identify the correct 112-voice engine

- [x] Identify the Windows app that bundles the 112 voices.
- [x] Inspect its documented local API and voice-set selection.
- [x] Confirm voice selection uses speaker_index within the currently loaded set.
- [ ] Install and test the upstream Beatrice app on the target Windows 11 PC.
- [ ] Verify model counts: JVS 100, Official 4, Classic Old TTS 8.
- [ ] Confirm the exact name and direction (capture/playback) of Beatrice's virtual audio endpoint.

## Stage 1 — Integrate Beatrice with the existing Wave Link 3 setup

- [ ] Record exact Windows playback/recording endpoint names for Beatrice and Wave Link 3.
- [ ] Confirm the user's actual chain: FIFINE K688 (XLR) -> MaonoCaster G1 Neo -> Wave Link 3 -> downstream Mixes/effects/output.
- [ ] Inspect Beatrice's virtual endpoint and determine how it participates in the existing Wave Link routing; do not presume it is a Wave Link endpoint.
- [ ] Preserve the existing FabFilter/TDR/LoudMax effects in their current Mixes placement and settings.
- [ ] Ensure the raw mic is not unintentionally doubled and there is no feedback loop.
- [ ] Verify monitoring, latency, sample rate, channel count, and stability.
- [ ] Do not add VB-CABLE unless a concrete endpoint test proves it necessary.


## Stage 2 — Control bridge

- [x] Implement a Windows helper for the documented Beatrice local API.
- [x] Add configurable global hotkeys for voice selection and LIVE/BYPASS.
- [x] Validate voice indices against /status and serialize model changes.
- [ ] Build and run the helper on Windows 11.
- [x] Confirm FIFINE D6 software supports assigning keyboard shortcut combinations (official help center).
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
