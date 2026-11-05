# Building

## Initial status

There is no buildable plugin target in the repository yet. The build system will be added after the VST3 SDK and Beatrice backend have been selected and audited. This avoids committing a misleading placeholder that appears to build but cannot produce a functioning effect.

## Intended environment

- Windows 11 x64
- Visual Studio 2022 with the Desktop development with C++ workload
- A supported CMake version
- A pinned Steinberg VST3 SDK version (to be selected)
- Beatrice runtime and model assets obtained from their verified upstream sources

Exact minimum versions will be recorded after a clean build is reproduced.

## Validation plan

Every implementation stage should record:
- compiler and SDK versions;
- exact configure/build commands;
- build output and warnings;
- automated test results;
- Wave Link 3 load and audio-path test results;
- measured latency and CPU/memory use.

Do not install unsigned or unverified binaries as part of troubleshooting. Build from reviewed source wherever practical.
