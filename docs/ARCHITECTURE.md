# Architecture

## Components

### 1. VST3 plugin (Wave Link 3)

Responsibilities:
- Expose a conventional VST3 audio effect and stable parameters.
- Handle sample-rate and block-size changes.
- Maintain a bounded, preallocated audio path.
- Report the effective processing latency to the host when supported.
- Provide deterministic dry/bypass behavior during startup, shutdown, overload, and worker failure.

The audio callback is a hard real-time boundary. It must not perform file access, model initialization, memory growth, mutex waits, process creation, network access, or synchronous IPC.

### 2. Beatrice worker

Responsibilities:
- Own Beatrice model/runtime initialization and teardown.
- Validate model/runtime versions and required assets.
- Process audio outside the plugin callback.
- Report readiness, errors, and measured processing latency.
- Restart safely without requiring the host to reload the plugin, if the final design supports it.

The worker protocol must be versioned. A worker crash must not crash the host.

### 3. Audio transport

Candidate approaches include shared-memory single-producer/single-consumer ring buffers and a bounded local streaming transport. Selection is pending a measured prototype.

Requirements:
- Bounded memory and explicit queue capacity.
- No unbounded waits on either side.
- Defined behavior for underrun, overrun, discontinuity, and sample-rate mismatch.
- Explicit sequence numbers and protocol version.
- A documented latency budget.

The VST3 callback must always finish within its real-time budget. If converted audio is not ready, the plugin must use the documented fallback policy rather than wait.

### 4. Control bridge

Preset selection and status are not part of the real-time audio transport.

The control bridge should:
- Listen only on local IPC.
- Restrict access to the current user/session.
- Validate message size, schema, parameter ranges, and preset IDs.
- Expose a small allowlisted command set (select preset, query status, safe start/stop).
- Never execute shell commands or accept arbitrary file paths.

FIFINE D6 integration will initially use documented keyboard shortcuts or a local control interface only after the device's actual capabilities and software behavior are verified. No unsupported direct device protocol will be assumed.

### 5. Presets

Presets should have stable identifiers and a versioned schema. Separate user-editable values from internal runtime state. Invalid or unknown preset data must be rejected safely and must not crash the worker or plugin.

## Decisions still open

- Which Beatrice implementation/API is the supported backend.
- Whether its runtime can sustain the target sample rate and block cadence on the user's hardware.
- Whether inference is streaming and stateful, and how much look-ahead it requires.
- Audio transport implementation and buffer sizes.
- VST3 SDK/version and build integration.
- Exact FIFINE D6 mapping mechanism.

These decisions require source/API inspection and a working latency prototype; they must not be guessed.
