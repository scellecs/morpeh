# Performance Comparison: Collection Benchmarks

## Environment

Benchmark run on AMD Ryzen 7 2700 Eight-Core Processor  with 16 logical cores.

Unity Editor version: 2022.3.49f1

Scripting Backend: IL2CPP

## Results

### *BitSetPerformanceTests*

| Functionality | BitSet (Morpeh) | BitArray (BCL) |
|---|--:|--:|
| `Clear(10000)` | 0.001ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.001ms <span style="color:green">(1.2x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.002ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 0.003ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(1000000)` | 0.011ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 0.018ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ClearManually(10000)` | 0.036ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.041ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ClearManually(100000)` | 0.353ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.403ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ClearManually(1000000)` | 3.819ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 4.552ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(10000)` | 0.099ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.104ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(100000)` | 1.030ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.037ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(1000000)` | 11.103ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 11.036ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Set(10000)` | 0.036ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.043ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(100000)` | 0.356ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.400ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(1000000)` | 3.952ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 4.304ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(10000)` | 0.036ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.073ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(100000)` | 0.360ms <span style="color:green">(4.3x)</span>&nbsp;🟢 | 1.540ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(1000000)` | 7.207ms <span style="color:green">(4.2x)</span>&nbsp;🟢 | 30.100ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(10000)` | 0.038ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.044ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(100000)` | 0.408ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.465ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(1000000)` | 5.635ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 7.263ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *FastListPerformanceTests*

| Functionality | FastList (Morpeh) | List (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.020ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.023ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.200ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.234ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 2.868ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 2.929ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.025ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.030ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 0.237ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.267ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 3.462ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 4.382ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.001ms <span style="color:green">(3.7x)</span>&nbsp;🟢 | 0.000ms <span style="color:green">(3.7x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.007ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.000ms <span style="color:green">(17.8x)</span>&nbsp;🟢 |
| `Clear(1000000)` | 0.067ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.000ms <span style="color:green">(133.0x)</span>&nbsp;🟢 |
| `ForEach(10000)` | 0.100ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.107ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 0.936ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.998ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.357ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 9.968ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 0.985ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.050ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 9.848ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 11.503ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerReadDirect(100000)` | 0.975ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.055ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerReadDirect(1000000)` | 10.283ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 13.933ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWrite(100000)` | 0.392ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | 0.573ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWrite(1000000)` | 6.420ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 7.265ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWriteDirect(100000)` | 0.274ms <span style="color:green">(2.3x)</span>&nbsp;🟢 | 0.644ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWriteDirect(1000000)` | 5.895ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 7.427ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 3.882ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 4.706ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 467.721ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 472.000ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAt(10000)` | 1.342ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.319ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAt(100000)` | 156.450ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 153.073ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAtFast(10000)` | 1.340ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.294ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAtFast(100000)` | 161.261ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 173.612ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBack(10000)` | 0.047ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.092ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBack(100000)` | 0.476ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 0.861ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBackFast(10000)` | 0.038ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 0.080ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBackFast(100000)` | 0.456ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 0.812ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveRange(10000)` | 0.004ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.004ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `RemoveRange(100000)` | 0.032ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.032ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |

---

### *IntHashMapPerformanceTests*

| Functionality | IntHashMap (Morpeh) | Dictionary (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.058ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.119ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.871ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 1.515ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 9.618ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 16.818ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.104ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 0.254ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 1.281ms <span style="color:green">(2.2x)</span>&nbsp;🟢 | 2.788ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 18.822ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 46.713ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.003ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.004ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.030ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.036ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(1000000)` | 1.454ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 1.869ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(10000)` | 0.099ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 0.272ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(100000)` | 0.983ms <span style="color:green">(3.6x)</span>&nbsp;🟢 | 3.502ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(1000000)` | 9.825ms <span style="color:green">(13.3x)</span>&nbsp;🟢 | 130.396ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.098ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 0.167ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 0.978ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 1.673ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.790ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 16.759ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(10000)` | 0.109ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 0.292ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.460ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 3.673ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 50.578ms <span style="color:green">(3.0x)</span>&nbsp;🟢 | 154.064ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.090ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 0.232ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.661ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 3.078ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 93.301ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 156.889ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(10000)` | 0.109ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.314ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(100000)` | 1.141ms <span style="color:green">(3.6x)</span>&nbsp;🟢 | 4.125ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(1000000)` | 11.354ms <span style="color:green">(14.4x)</span>&nbsp;🟢 | 163.790ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntHashSetPerformanceTests*

| Functionality | IntHashSet (Morpeh) | HashSet (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.070ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.199ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.857ms <span style="color:green">(2.3x)</span>&nbsp;🟢 | 1.990ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 9.758ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 20.410ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.089ms <span style="color:green">(3.1x)</span>&nbsp;🟢 | 0.274ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 1.142ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 2.951ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 15.448ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 41.224ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.003ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.003ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.026ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.032ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(1000000)` | 1.066ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 1.498ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.106ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.110ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.045ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.113ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 10.319ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.971ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(10000)` | 0.113ms <span style="color:green">(3.2x)</span>&nbsp;🟢 | 0.358ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(100000)` | 1.059ms <span style="color:green">(4.5x)</span>&nbsp;🟢 | 4.747ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(1000000)` | 10.616ms <span style="color:green">(18.3x)</span>&nbsp;🟢 | 194.604ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.103ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 0.292ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.624ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 4.056ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 68.435ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 183.160ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntStackPerformanceTests*

| Functionality | IntStack (Morpeh) | Stack (BCL) |
|---|--:|--:|
| `Pop(10000)` | 0.101ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.109ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Pop(100000)` | 1.050ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.083ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Pop(1000000)` | 11.255ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 11.026ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Push(10000)` | 0.028ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.028ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Push(100000)` | 0.258ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.271ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Push(1000000)` | 3.179ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 3.213ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `PushGrow(10000)` | 0.034ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.034ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `PushGrow(100000)` | 0.311ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.306ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `PushGrow(1000000)` | 3.981ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 4.371ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryPop(10000)` | 0.104ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.116ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryPop(100000)` | 0.991ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.104ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryPop(1000000)` | 10.221ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.964ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *LongHashMapPerformanceTests*

| Functionality | LongHashMap (Morpeh) | Dictionary (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.095ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 0.197ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 1.518ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 3.013ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 45.063ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | 67.734ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.149ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 0.394ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 2.107ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 4.472ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 54.120ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 94.933ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.007ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.006ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.108ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 0.143ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(1000000)` | 2.366ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 2.804ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(10000)` | 0.106ms <span style="color:green">(3.6x)</span>&nbsp;🟢 | 0.377ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(100000)` | 1.059ms <span style="color:green">(4.4x)</span>&nbsp;🟢 | 4.613ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(1000000)` | 10.714ms <span style="color:green">(15.1x)</span>&nbsp;🟢 | 161.658ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.105ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 0.180ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.052ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 1.860ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 10.521ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 17.961ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(10000)` | 0.117ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 0.305ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.655ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 4.400ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 64.810ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 179.284ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.099ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 0.248ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.764ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 3.202ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 100.128ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 170.772ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(10000)` | 0.100ms <span style="color:green">(3.3x)</span>&nbsp;🟢 | 0.327ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(100000)` | 1.056ms <span style="color:green">(4.3x)</span>&nbsp;🟢 | 4.537ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(1000000)` | 10.554ms <span style="color:green">(17.3x)</span>&nbsp;🟢 | 182.319ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---
