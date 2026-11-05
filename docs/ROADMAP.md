# Roadmap

## Stage 0 — Repository and upstream audit

- [x] Create a separate repository for this project.
- [x] Record project goals and safety constraints.
- [ ] Inspect the exact upstream Beatrice source, API, license, model requirements, and runtime dependencies.
- [ ] Identify a supported VST3 SDK/toolchain combination.
- [ ] Confirm Wave Link 3 host behavior and plugin discovery requirements.

**Exit criteria:** an evidence-based integration design, with no unverified binary or license assumptions.

## Stage 1 — Audio and inference feasibility

- [ ] Build a minimal VST3 test effect and verify it loads in Wave Link 3.
- [ ] Prototype Beatrice inference in a separate worker.
- [ ] Measure cold-start time, steady-state processing time, buffering needs, and end-to-end latency.
- [ ] Verify behavior at supported sample rates and variable block sizes.
- [ ] Define fallback behavior for worker startup, overload, and crash.

**Exit criteria:** repeatable real-time-safe audio prototype with measured latency.

## Stage 2 — Stable bridge

- [ ] Implement bounded audio transport.
- [ ] Implement versioned, access-controlled local control IPC.
- [ ] Add parameter validation, worker lifecycle handling, and diagnostics.
- [ ] Add automated protocol and stress tests.

**Exit criteria:** no host blocking or crash under worker failure and malformed control messages.

## Stage 3 — Presets and FIFINE D6

- [ ] Implement versioned preset storage.
- [ ] Define stable preset IDs and safe switching semantics.
- [ ] Verify a practical D6 trigger path on the target Windows setup.
- [ ] Test rapid repeated presses and missing/invalid preset states.

**Exit criteria:** reliable preset switching without audio-thread blocking.

## Stage 4 — Packaging and release validation

- [ ] Reproducible Windows x64 build.
- [ ] Dependency and license inventory.
- [ ] Static analysis and security scan of distributable artifacts.
- [ ] Host compatibility and long-duration audio tests.
- [ ] Document installation, removal, rollback, and troubleshooting.

**Exit criteria:** release package with checksums, provenance, and honest compatibility notes.
