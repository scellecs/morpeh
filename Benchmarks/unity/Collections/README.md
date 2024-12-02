## Setup Instructions

Before running the benchmarks, complete these steps:

1. Project Setup:
   - Create a clean Unity project
   - Clone Morpeh into the Assets folder
   - Install package: `com.unity.test-framework.performance`

2. Project Settings Configuration:
   - Disable VSync
   - Remove all Quality Settings except one
   - Set Scripting Backend to IL2CPP
   - Remove the camera from the scene
   - Close Unity Editor (not needed anymore)

3. Running Benchmarks (Windows Example):
   - Open terminal
   - Navigate to Unity Editor folder, e.g.:
     ```
     cd "C:\Program Files\Unity\Hub\Editor\2022.3.49f1\Editor"
     ```
   - Run build and tests command:
     ```
     ./Unity.exe -runTests -batchMode -projectPath PROJECT_PATH -testPlatform StandaloneWindows64 -buildTarget StandaloneWindows64 -mtRendering -scriptingbackend=il2cpp
     ```
     Replace PROJECT_PATH with your path (e.g., M:/morpeh-2024)

4. Export Results:
   - After tests complete and Unity application closes, run:
     ```
     ./Unity.exe -batchmode -projectPath PROJECT_PATH -executeMethod Scellecs.Morpeh.Benchmarks.Collections.Editor.BenchmarkExporter.RunExport -quit
     ```
     Again, replace PROJECT_PATH with your actual project path
# Performance Comparison: Collection Benchmarks

## Environment

Benchmark run on AMD Ryzen 7 2700 Eight-Core Processor  with 16 logical cores.

Unity Editor version: 2022.3.49f1

Scripting Backend: IL2CPP

## Results

### *BitSetPerformanceTests*

| Functionality | BitSet (Morpeh) | BitArray (BCL) |
|---|--:|--:|
| `Clear(10000)` | 0.001ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.000ms <span style="color:green">(1.9x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.001ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.002ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(1000000)` | 0.011ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 0.019ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ClearManually(10000)` | 0.036ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.040ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ClearManually(100000)` | 0.416ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.399ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `ClearManually(1000000)` | 4.950ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 5.257ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(10000)` | 0.100ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.104ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(100000)` | 0.985ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.037ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(1000000)` | 9.856ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.428ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(10000)` | 0.036ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 0.046ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(100000)` | 0.580ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.409ms <span style="color:green">(1.4x)</span>&nbsp;🟢 |
| `Set(1000000)` | 5.341ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 5.861ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(10000)` | 0.039ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.074ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(100000)` | 0.372ms <span style="color:green">(4.1x)</span>&nbsp;🟢 | 1.528ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(1000000)` | 5.515ms <span style="color:green">(5.3x)</span>&nbsp;🟢 | 29.267ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(10000)` | 0.035ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 0.045ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(100000)` | 0.354ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.398ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(1000000)` | 4.753ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 5.247ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *FastListPerformanceTests*

| Functionality | FastList (Morpeh) | List (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.020ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.023ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.249ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 0.387ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 2.181ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 2.440ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.053ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.055ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 0.536ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.401ms <span style="color:green">(1.3x)</span>&nbsp;🟢 |
| `AddGrow(1000000)` | 4.967ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 4.692ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `Clear(10000)` | 0.001ms <span style="color:green">(3.3x)</span>&nbsp;🟢 | 0.000ms <span style="color:green">(3.3x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.007ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.000ms <span style="color:green">(23.7x)</span>&nbsp;🟢 |
| `Clear(1000000)` | 0.075ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.002ms <span style="color:green">(39.7x)</span>&nbsp;🟢 |
| `ForEach(10000)` | 0.105ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.117ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.105ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.167ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 10.095ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 11.149ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.057ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.143ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 11.433ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 19.205ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerReadDirect(100000)` | 1.139ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.158ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerReadDirect(1000000)` | 10.360ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 12.336ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWrite(100000)` | 0.375ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 0.618ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWrite(1000000)` | 5.411ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 7.440ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWriteDirect(100000)` | 0.268ms <span style="color:green">(2.2x)</span>&nbsp;🟢 | 0.584ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWriteDirect(1000000)` | 5.859ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 7.172ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 4.457ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 4.223ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `Remove(100000)` | 474.100ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 482.678ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAt(10000)` | 1.357ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.283ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `RemoveAt(100000)` | 154.108ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 158.365ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtFast(10000)` | 1.321ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.299ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAtFast(100000)` | 154.545ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 152.136ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAtSwapBack(10000)` | 0.116ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.082ms <span style="color:green">(1.4x)</span>&nbsp;🟢 |
| `RemoveAtSwapBack(100000)` | 0.419ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.804ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBackFast(10000)` | 0.037ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 0.078ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBackFast(100000)` | 0.397ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.810ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveRange(10000)` | 0.004ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.004ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveRange(100000)` | 0.032ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.032ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |

---

### *IntHashMapPerformanceTests*

| Functionality | IntHashMap (Morpeh) | Dictionary (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.058ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.115ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.849ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 1.478ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 10.365ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 17.074ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.112ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 0.282ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 1.282ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 2.725ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 33.226ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 40.737ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.004ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.004ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.030ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.035ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(1000000)` | 1.443ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 1.840ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(10000)` | 0.272ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.293ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(100000)` | 0.995ms <span style="color:green">(3.5x)</span>&nbsp;🟢 | 3.498ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(1000000)` | 9.829ms <span style="color:green">(13.6x)</span>&nbsp;🟢 | 133.534ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.086ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.164ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 0.975ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 1.684ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.891ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 16.875ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(10000)` | 0.116ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 0.320ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.485ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 3.585ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 51.378ms <span style="color:green">(3.1x)</span>&nbsp;🟢 | 158.496ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.090ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 0.230ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.645ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 3.090ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 79.982ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 151.796ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(10000)` | 0.098ms <span style="color:green">(3.0x)</span>&nbsp;🟢 | 0.296ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(100000)` | 0.982ms <span style="color:green">(3.7x)</span>&nbsp;🟢 | 3.678ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(1000000)` | 9.857ms <span style="color:green">(15.4x)</span>&nbsp;🟢 | 151.932ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntHashSetPerformanceTests*

| Functionality | IntHashSet (Morpeh) | HashSet (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.052ms <span style="color:green">(3.0x)</span>&nbsp;🟢 | 0.154ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.792ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 1.868ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 9.001ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 21.245ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.102ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.294ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 1.138ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 2.830ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 15.430ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 38.740ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.004ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 0.003ms <span style="color:green">(1.3x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.022ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 0.028ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(1000000)` | 1.069ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 1.466ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.240ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.110ms <span style="color:green">(2.2x)</span>&nbsp;🟢 |
| `ForEach(100000)` | 0.957ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.036ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.702ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.298ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(10000)` | 0.098ms <span style="color:green">(3.3x)</span>&nbsp;🟢 | 0.320ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(100000)` | 0.990ms <span style="color:green">(5.0x)</span>&nbsp;🟢 | 4.989ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(1000000)` | 9.878ms <span style="color:green">(18.6x)</span>&nbsp;🟢 | 183.511ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.087ms <span style="color:green">(3.0x)</span>&nbsp;🟢 | 0.263ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.513ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 3.751ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 63.844ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 164.611ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntStackPerformanceTests*

| Functionality | IntStack (Morpeh) | Stack (BCL) |
|---|--:|--:|
| `Pop(10000)` | 0.092ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.084ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `Pop(100000)` | 0.971ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.016ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Pop(1000000)` | 9.664ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.285ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Push(10000)` | 0.069ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.024ms <span style="color:green">(2.9x)</span>&nbsp;🟢 |
| `Push(100000)` | 0.377ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.259ms <span style="color:green">(1.5x)</span>&nbsp;🟢 |
| `Push(1000000)` | 4.128ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 3.763ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `PushGrow(10000)` | 0.068ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.038ms <span style="color:green">(1.8x)</span>&nbsp;🟢 |
| `PushGrow(100000)` | 0.369ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.348ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `PushGrow(1000000)` | 5.757ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 4.457ms <span style="color:green">(1.3x)</span>&nbsp;🟢 |
| `TryPop(10000)` | 0.097ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.102ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryPop(100000)` | 1.003ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.053ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryPop(1000000)` | 9.645ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.297ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *LongHashMapPerformanceTests*

| Functionality | LongHashMap (Morpeh) | Dictionary (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.088ms <span style="color:green">(2.3x)</span>&nbsp;🟢 | 0.202ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 1.468ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 2.599ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 42.311ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 66.795ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.160ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.464ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 3.185ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 4.603ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 50.856ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 87.790ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.005ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.005ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.059ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.054ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `Clear(1000000)` | 2.356ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 2.755ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(10000)` | 0.099ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.287ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(100000)` | 0.984ms <span style="color:green">(3.9x)</span>&nbsp;🟢 | 3.829ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(1000000)` | 9.940ms <span style="color:green">(16.2x)</span>&nbsp;🟢 | 161.288ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.098ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 0.172ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.017ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 1.770ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.987ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 17.134ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(10000)` | 0.108ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 0.292ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.525ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 3.800ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 64.532ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 178.076ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.097ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 0.265ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.894ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 3.481ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 100.570ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 169.426ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(10000)` | 0.100ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.290ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(100000)` | 0.984ms <span style="color:green">(4.1x)</span>&nbsp;🟢 | 3.995ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(1000000)` | 9.834ms <span style="color:green">(16.4x)</span>&nbsp;🟢 | 161.586ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---
