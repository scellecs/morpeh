# Performance Comparison: Collection Benchmarks

Benchmark run on AMD Ryzen 7 2700 Eight-Core Processor  with 16 logical cores.

Unity Editor version: 2022.3.49f1

Scripting Backend: IL2CPP

### *BitSetPerformanceTests*

| Functionality | BitSet (Morpeh) | BitArray (BCL) |
|---|--:|--:|
| `IsSet(10000)` | 0.108ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.117ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(100000)` | 1.080ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.050ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `IsSet(1000000)` | 10.991ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 12.100ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(10000)` | 0.039ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.044ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(100000)` | 0.391ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.465ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(1000000)` | 4.071ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 4.509ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(10000)` | 0.045ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.084ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(100000)` | 0.453ms <span style="color:green">(3.4x)</span>&nbsp;🟢 | 1.537ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(1000000)` | 6.560ms <span style="color:green">(4.4x)</span>&nbsp;🟢 | 28.868ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(10000)` | 0.037ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.041ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(100000)` | 0.355ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.398ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(1000000)` | 3.760ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 4.349ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *FastListPerformanceTests*

| Functionality | FastList (Morpeh) | List (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.051ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.023ms <span style="color:green">(2.2x)</span>&nbsp;🟢 |
| `Add(100000)` | 0.223ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 0.403ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 4.001ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 3.236ms <span style="color:green">(1.2x)</span>&nbsp;🟢 |
| `AddGrow(10000)` | 0.079ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.050ms <span style="color:green">(1.6x)</span>&nbsp;🟢 |
| `AddGrow(100000)` | 0.512ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.478ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `AddGrow(1000000)` | 4.898ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 5.292ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.100ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.105ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.018ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.100ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.817ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.521ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.051ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.109ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 10.449ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 12.037ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerReadDirect(100000)` | 0.986ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.089ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerReadDirect(1000000)` | 9.871ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 12.172ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWrite(100000)` | 0.349ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 0.617ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWrite(1000000)` | 5.258ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | 8.032ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWriteDirect(100000)` | 0.291ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.578ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWriteDirect(1000000)` | 5.416ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 7.760ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 4.084ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 4.219ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 477.714ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 485.458ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAt(10000)` | 1.372ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.316ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAt(100000)` | 155.167ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 155.069ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAtFast(10000)` | 1.384ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.359ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAtFast(100000)` | 157.008ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 153.543ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAtSwapBack(10000)` | 0.040ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.078ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBack(100000)` | 0.463ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 0.952ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBackFast(10000)` | 0.046ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.088ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBackFast(100000)` | 0.434ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.875ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveRange(10000)` | 0.004ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.004ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveRange(100000)` | 0.036ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 0.057ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntHashMapPerformanceTests*

| Functionality | IntHashMap (Morpeh) | Dictionary (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.063ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.118ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.855ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 1.506ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 10.067ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 17.748ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.119ms <span style="color:green">(2.3x)</span>&nbsp;🟢 | 0.279ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 1.304ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 2.706ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 34.831ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 47.891ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(10000)` | 0.107ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 0.297ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(100000)` | 1.042ms <span style="color:green">(4.1x)</span>&nbsp;🟢 | 4.250ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(1000000)` | 9.829ms <span style="color:green">(13.8x)</span>&nbsp;🟢 | 135.404ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.103ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 0.168ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.012ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 1.673ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 10.092ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 17.017ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(10000)` | 0.109ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 0.287ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.473ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 3.836ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 54.832ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 155.701ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.117ms <span style="color:green">(3.1x)</span>&nbsp;🟢 | 0.367ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.899ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 3.225ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 87.156ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 158.278ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(10000)` | 0.147ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.284ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(100000)` | 0.984ms <span style="color:green">(3.7x)</span>&nbsp;🟢 | 3.685ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(1000000)` | 10.315ms <span style="color:green">(14.7x)</span>&nbsp;🟢 | 151.248ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntHashSetPerformanceTests*

| Functionality | IntHashSet (Morpeh) | HashSet (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.055ms <span style="color:green">(3.0x)</span>&nbsp;🟢 | 0.164ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.860ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 2.097ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 9.807ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 21.081ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.104ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.303ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 1.269ms <span style="color:green">(2.2x)</span>&nbsp;🟢 | 2.835ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 16.770ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 42.304ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.111ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.120ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.118ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.184ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.787ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.577ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(10000)` | 0.129ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 0.334ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(100000)` | 1.085ms <span style="color:green">(4.8x)</span>&nbsp;🟢 | 5.190ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(1000000)` | 10.681ms <span style="color:green">(17.6x)</span>&nbsp;🟢 | 187.947ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.214ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.259ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.578ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 3.880ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 62.093ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 168.537ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntStackPerformanceTests*

| Functionality | IntStack (Morpeh) | Stack (BCL) |
|---|--:|--:|
| `Pop(10000)` | 0.106ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.109ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Pop(100000)` | 1.017ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.085ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Pop(1000000)` | 9.806ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 10.023ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Push(10000)` | 0.034ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.034ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Push(100000)` | 0.401ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.251ms <span style="color:green">(1.6x)</span>&nbsp;🟢 |
| `Push(1000000)` | 4.048ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 3.505ms <span style="color:green">(1.2x)</span>&nbsp;🟢 |
| `PushGrow(10000)` | 0.068ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.036ms <span style="color:green">(1.9x)</span>&nbsp;🟢 |
| `PushGrow(100000)` | 0.478ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.462ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `PushGrow(1000000)` | 5.204ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 4.912ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `TryPop(10000)` | 0.113ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.113ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `TryPop(100000)` | 1.103ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 1.549ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryPop(1000000)` | 10.318ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 10.702ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *LongHashMapPerformanceTests*

| Functionality | LongHashMap (Morpeh) | Dictionary (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.087ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 0.217ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 1.467ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 2.663ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 45.399ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 63.805ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.171ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 0.442ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 3.044ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | 4.455ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 50.508ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 92.224ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(10000)` | 0.105ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.302ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(100000)` | 1.042ms <span style="color:green">(3.5x)</span>&nbsp;🟢 | 3.691ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(1000000)` | 11.430ms <span style="color:green">(11.8x)</span>&nbsp;🟢 | 134.776ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.114ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 0.189ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.005ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 1.713ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.814ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 16.694ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(10000)` | 0.115ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 0.319ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.491ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 4.075ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 60.260ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 161.498ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.098ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 0.241ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.795ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 3.213ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 95.138ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 157.743ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(10000)` | 0.102ms <span style="color:green">(3.0x)</span>&nbsp;🟢 | 0.306ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(100000)` | 1.009ms <span style="color:green">(3.8x)</span>&nbsp;🟢 | 3.832ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(1000000)` | 9.878ms <span style="color:green">(15.3x)</span>&nbsp;🟢 | 151.565ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---
