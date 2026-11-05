# Upstream Beatrice audit

**Audit date:** 2026-11-05  
**Purpose:** determine whether upstream Beatrice can be integrated and redistributed in this project. This is a source and metadata review, not a binary malware analysis or a legal opinion.

## Sources inspected

- Official engine/VST source: https://github.com/prj-beatrice/beatrice-vst
- Windows application wrapper: https://github.com/Project-Beatrice-V2/Beatrice-voicechanger-windows
- Windows wrapper README and its `LICENSE`, `contributors.txt`, `beatrice_loader.py`, and `beatrice_audio.py`
- Model repository: https://github.com/Project-Beatrice-V2/beatrice-voice-models

## Findings

### 1. The public source license does not grant rights to the complete engine

The `prj-beatrice/beatrice-vst` repository has an MIT `LICENSE.txt` for the source files, but its `LICENSES_BUNDLED.txt` explicitly states that the Beatrice VST program includes a **non-public inference library** and that use beyond the scope permitted by applicable law requires individual permission from Project Beatrice.

**Consequence:** do not assume that an MIT license on the visible VST source permits copying, linking, modifying, or redistributing the complete inference engine. Before shipping or bundling the engine, obtain and archive explicit written permission or authoritative license terms covering the intended use. A local personal prototype must still comply with the applicable upstream terms.

### 2. Windows wrapper is a separate application, not a ready-made Wave Link bridge

The Windows project is an Electron application with a Python audio backend. Its README describes a local REST interface on `127.0.0.1:5005`, and the backend opens its own PortAudio input/output streams at 16 kHz with 160-sample blocks.

**Consequence:** this backend cannot simply be inserted into Wave Link's VST3 signal path. Running a second independent microphone stream can cause device contention, routing complexity, and extra buffering. Reusing the UI or audio loop is not the same as implementing a host-loaded VST3 effect.

### 3. Native Beatrice API is visible only as an implementation detail in the wrapper

`beatrice_loader.py` loads a DLL from the bundled VST3 directory with `ctypes.CDLL` and declares functions such as `Beatrice20rc0_CreatePhoneExtractor`, `Beatrice20rc0_CreatePitchEstimator`, `Beatrice20rc0_CreateWaveformGenerator`, and corresponding context/resource functions.

The inspected loader demonstrates component construction and parameter-file loading. It does not, by itself, document a stable public SDK contract for streaming inference, thread safety, ABI compatibility, or redistribution.

**Consequence:** treat these symbols as an undocumented/private integration surface until the upstream maintainers confirm the supported API and its terms. Do not build the new plugin against guessed function signatures.

### 4. Model/data licensing is separate from application code

The Windows wrapper's contributor notice identifies JVS corpus content as non-commercial use only. The model repository also states that each model can carry its own terms, independent of the repository's MIT license.

**Consequence:** do not bundle JVS or community model weights by default. Every model must have its own provenance and license record, including voice/likeness permissions where applicable.

### 5. The Python audio backend is not suitable for a VST3 real-time callback

The backend uses Python, NumPy, PortAudio, queues, and a separate monitor stream. That may be workable as a standalone application, but it is not evidence that inference can safely run inside a host's real-time audio callback.

**Consequence:** keep inference out of the VST3 callback. First prove a bounded streaming interface and measure processing time, buffering, and latency in an isolated prototype.

## Decision

Proceed with a clean-room host/bridge design and upstream API investigation. Do not copy upstream binaries, model weights, or proprietary inference components into this repository.

The first technical milestone is not a full plugin: it is a reproducible proof that an authorized Beatrice runtime can accept streaming audio, produce streaming output, and meet a measured latency budget on the target Windows machine.

## Open questions for upstream maintainers

1. What exact license governs the proprietary inference library and the bundled Beatrice engine binaries?
2. Is there a supported public SDK/API for streaming inference outside the official VST host?
3. Are the exported `Beatrice20rc0_*` symbols stable and documented, and what ABI/calling convention/threading guarantees apply?
4. May a third-party VST3 wrapper load the engine, and may it be distributed for personal/non-commercial use?
5. Which model packages can be used and redistributed, and under what voice/data terms?


## Additional verification — official Beatrice 2 documentation and build recipe

The official Beatrice website's Beatrice 2 FAQ states that the model architecture is published and that third parties may create software that converts voices using Beatrice 2 models. It separately says that use of the **official inference library** requires contacting Project Beatrice. This distinction matters: an independently implemented inference engine may be possible in principle, but the public model architecture is not the same thing as a redistributable official runtime.

The official `prj-beatrice/beatrice-vst` build files were also inspected:

- The plugin is C++20 and uses Steinberg's VST3 SDK as a submodule.
- Its CMake target links an imported static `beatricelib` (`beatrice.lib` on Windows).
- The Makefile downloads that Windows library from the Beatrice Hugging Face release location; it is not built from the visible source tree.
- The release packaging target can also download and bundle the JVS model package.
- The bundled-license notice explicitly restricts use of the non-public inference library absent individual permission.

**Updated consequence:** the official VST source is useful for understanding the host-side structure and build setup, but copying its DSP implementation or linking its `beatrice.lib` is not an acceptable default for this project. We must either (a) obtain explicit written permission and supported integration terms from Project Beatrice, or (b) implement/use a separately licensed inference backend compatible with the published model format. The second path is a materially larger engineering task and requires its own correctness and performance validation.

Official documentation reviewed:
- https://prj-beatrice.com/ — Beatrice 2 FAQ and release information
- https://github.com/prj-beatrice/beatrice-vst/blob/main/CMakeLists.txt
- https://github.com/prj-beatrice/beatrice-vst/blob/main/Makefile
- https://github.com/prj-beatrice/beatrice-vst/blob/main/LICENSES_BUNDLED.txt

This remains a source/build-recipe inspection only. No upstream binary, downloaded library, model, or installer was executed or malware-scanned.


## Revised project decision — 2026-11-05

A direct inspection of the official repository confirms that **a complete Beatrice 2 VST3 plugin already exists upstream**. The project must not spend time recreating its VST3 processor or implementing a second inference pipeline.

Inspected files:
- `src/vst/processor.h` and `src/vst/processor.cc`: native VST3 audio processor, mono/stereo bus handling, float32 processing, state persistence, and latency reporting.
- `src/vst/controller.cc`: VST3 parameter registration and host state synchronization.
- `src/common/parameter_schema.h`: parameter IDs include model, voice, pitch/formant, gain, intonation, voice-morph controls, and latency reporting.
- `src/vst/editor.cc`: the model is selected through a file selector in the plugin UI.
- `CMakeLists.txt` and `Makefile`: official build recipe and dependency download behavior.

The visible source contains automatable numeric/list parameters and host state persistence. The model file path is a string parameter handled by the plugin editor/state code, not exposed as a normal automatable VST parameter in the controller registration. No explicit global hotkey or standalone preset-switching API was found in the inspected source. Therefore D6 control feasibility still needs a targeted check; it must not be assumed that the model selector can be changed directly by host automation.

**Superseding decision:** use the official Beatrice VST3 as the audio effect and build only a small companion/control bridge if Wave Link 3 and FIFINE D6 do not already provide the required control path. Do not create a separate inference worker, second microphone stream, or replacement VST3 unless direct compatibility testing demonstrates a real need.

The proprietary inference-library restriction remains relevant to redistribution. The safest initial approach is for the user to obtain/install the official plugin through its authorized upstream distribution; this repository should not bundle the upstream plugin, proprietary library, or model assets.

The revised first milestone is:
1. verify the official Beatrice VST3 loads and processes audio in Wave Link 3;
2. identify the exact Beatrice settings that need switching (model, voice, or parameter state);
3. verify D6 hotkey output and determine the least invasive supported trigger mechanism;
4. implement only the missing control glue.

This revised scope replaces the earlier proposal to build a new VST3 host plugin plus isolated Beatrice inference worker.
