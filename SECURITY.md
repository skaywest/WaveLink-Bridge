# Security Policy

## Scope

This project is intended to process microphone audio locally on Windows. Security review is required for the plugin, worker process, IPC, model/runtime packages, and any downloaded dependencies.

## Reporting

Please do not publish exploitable vulnerabilities or private audio samples in public issues. Use GitHub's private vulnerability reporting feature if it is enabled for this repository; otherwise contact the repository owner privately.

## Current limitations

- No release binaries are currently provided or certified.
- Source review does not establish that an external binary or model is safe.
- Third-party artifacts must be pinned, provenance-checked, license-checked, and scanned before being bundled.
- IPC must be local-only, access-controlled, and validated against malformed or oversized messages.
- The plugin must not accept arbitrary paths, commands, or code through preset/control messages.

## Audio privacy

The intended runtime is offline. Microphone audio, intermediate features, and converted audio must not be transmitted to remote services. Diagnostic logging must not record raw audio or sensitive speech content.
