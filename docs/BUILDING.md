# Building

## Current strategy

Do not build a replacement Beatrice VST3. The official plugin is maintained upstream:

https://github.com/prj-beatrice/beatrice-vst

This repository will contain only the companion/control bridge, unless tests establish that a replacement audio plugin is unavoidable.

## Intended environment

- Windows 11 x64
- Visual Studio 2022 with the Desktop development with C++ workload (if native helper code is needed)
- CMake (only if the selected helper implementation uses it)
- Wave Link 3 and an authorized installation of the official Beatrice VST3 for integration testing

## Validation plan

Record:
- exact official Beatrice plugin version and provenance;
- Wave Link 3 version and plugin discovery/load result;
- compiler and build-tool versions for the bridge;
- exact configure/build commands and warnings;
- automated test results;
- D6 button-to-action mapping;
- preset switching reliability and long-session results.

Do not download or redistribute the proprietary Beatrice inference library or model packages from this repository. Obtain the official plugin and its dependencies from authorized upstream distribution channels.
