# Performance Comparison: Collection Benchmarks

## Environment

Benchmark run on AMD Ryzen 7 2700 Eight-Core Processor  with 16 logical cores.

Unity Editor version: 2022.3.49f1

Scripting Backend: IL2CPP

## Results

### *BitSetPerformanceTests*

| Functionality | BitSet (Morpeh) | BitArray (BCL) |
|---|--:|--:|
| `Clear(10000)` | 0.000ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 0.000ms <span style="color:green">(1.7x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.002ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 0.003ms <span style="color:green">(1.4x)</span>&nbsp;🟢 |
| `Clear(1000000)` | 0.011ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.021ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ClearManually(10000)` | 0.041ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.046ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ClearManually(100000)` | 0.410ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.464ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ClearManually(1000000)` | 4.308ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 5.013ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(10000)` | 0.114ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.120ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(100000)` | 1.143ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.204ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(1000000)` | 11.447ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 11.117ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Set(10000)` | 0.042ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.047ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(100000)` | 0.421ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.473ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(1000000)` | 4.017ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 4.433ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(10000)` | 0.077ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.072ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `SetGrow(100000)` | 0.380ms <span style="color:green">(3.9x)</span>&nbsp;🟢 | 1.486ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(1000000)` | 4.026ms <span style="color:green">(6.8x)</span>&nbsp;🟢 | 27.407ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(10000)` | 0.036ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.042ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(100000)` | 0.356ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.407ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(1000000)` | 3.924ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 4.394ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *FastListPerformanceTests*

| Functionality | FastList (Morpeh) | List (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.023ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.027ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.263ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.263ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Add(1000000)` | 2.224ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 2.546ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.064ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.039ms <span style="color:green">(1.6x)</span>&nbsp;🟢 |
| `AddGrow(100000)` | 0.731ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.752ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 4.758ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 5.009ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.001ms <span style="color:green">(5.0x)</span>&nbsp;🟢 | 0.000ms <span style="color:green">(5.0x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.007ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.000ms <span style="color:green">(24.3x)</span>&nbsp;🟢 |
| `Clear(1000000)` | 0.073ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.001ms <span style="color:green">(61.0x)</span>&nbsp;🟢 |
| `ForEach(10000)` | 0.103ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.108ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.038ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.005ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `ForEach(1000000)` | 9.516ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.221ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.059ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.118ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 9.878ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 11.875ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerReadDirect(100000)` | 1.107ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.215ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerReadDirect(1000000)` | 10.251ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 12.144ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWrite(100000)` | 0.357ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 0.592ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWrite(1000000)` | 4.619ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 7.526ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWriteDirect(100000)` | 0.299ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.585ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWriteDirect(1000000)` | 4.373ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 7.623ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 4.088ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 4.423ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 470.824ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 474.455ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAt(10000)` | 2.108ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.297ms <span style="color:green">(1.6x)</span>&nbsp;🟢 |
| `RemoveAt(100000)` | 166.070ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 165.698ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAtFast(10000)` | 1.558ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.506ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAtFast(100000)` | 180.452ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 178.396ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAtSwapBack(10000)` | 0.047ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 0.131ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBack(100000)` | 0.491ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.948ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBackFast(10000)` | 0.044ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 0.093ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBackFast(100000)` | 0.475ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.960ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveRange(10000)` | 0.004ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.004ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveRange(100000)` | 0.037ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.037ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |

---

### *IntHashMapPerformanceTests*

| Functionality | IntHashMap (Morpeh) | Dictionary (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.074ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.137ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.944ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 1.669ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 12.801ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 21.819ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.124ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 0.298ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 3.104ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 5.081ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 33.371ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | 49.694ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.004ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.007ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(100000)` | 0.084ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.163ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(1000000)` | 1.524ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 1.933ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(10000)` | 0.114ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 0.315ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(100000)` | 1.099ms <span style="color:green">(3.8x)</span>&nbsp;🟢 | 4.218ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(1000000)` | 11.102ms <span style="color:green">(11.8x)</span>&nbsp;🟢 | 131.189ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.107ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 0.183ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.075ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 1.798ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 10.595ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 18.357ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(10000)` | 0.121ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 0.334ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.665ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 4.042ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 56.734ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 152.552ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.102ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 0.260ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.855ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 3.372ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 91.319ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 162.227ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(10000)` | 0.111ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.326ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(100000)` | 1.077ms <span style="color:green">(3.8x)</span>&nbsp;🟢 | 4.096ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(1000000)` | 11.367ms <span style="color:green">(13.4x)</span>&nbsp;🟢 | 151.839ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntHashSetPerformanceTests*

| Functionality | IntHashSet (Morpeh) | HashSet (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.060ms <span style="color:green">(3.1x)</span>&nbsp;🟢 | 0.187ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.857ms <span style="color:green">(2.2x)</span>&nbsp;🟢 | 1.907ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 11.042ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 21.805ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.111ms <span style="color:green">(3.1x)</span>&nbsp;🟢 | 0.349ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 2.392ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 6.030ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 23.019ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 57.105ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.003ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.003ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.049ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.031ms <span style="color:green">(1.6x)</span>&nbsp;🟢 |
| `Clear(1000000)` | 1.054ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 1.474ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.096ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.104ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 0.973ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.107ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.773ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.632ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(10000)` | 0.109ms <span style="color:green">(3.3x)</span>&nbsp;🟢 | 0.365ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(100000)` | 1.140ms <span style="color:green">(4.1x)</span>&nbsp;🟢 | 4.719ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(1000000)` | 10.200ms <span style="color:green">(18.0x)</span>&nbsp;🟢 | 183.212ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.103ms <span style="color:green">(3.0x)</span>&nbsp;🟢 | 0.306ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.545ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 4.003ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 64.133ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 170.327ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntStackPerformanceTests*

| Functionality | IntStack (Morpeh) | Stack (BCL) |
|---|--:|--:|
| `Pop(10000)` | 0.109ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.113ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Pop(100000)` | 1.103ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.004ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `Pop(1000000)` | 9.756ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.758ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Push(10000)` | 0.024ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.024ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Push(100000)` | 0.251ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.254ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Push(1000000)` | 2.508ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 2.463ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `PushGrow(10000)` | 0.144ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.037ms <span style="color:green">(3.9x)</span>&nbsp;🟢 |
| `PushGrow(100000)` | 1.018ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.065ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `PushGrow(1000000)` | 5.292ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 5.318ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryPop(10000)` | 0.097ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.101ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryPop(100000)` | 1.010ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.056ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryPop(1000000)` | 9.864ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.897ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *LongHashMapPerformanceTests*

| Functionality | LongHashMap (Morpeh) | Dictionary (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.087ms <span style="color:green">(2.3x)</span>&nbsp;🟢 | 0.197ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 1.501ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 2.546ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 42.520ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | 62.338ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.180ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 0.455ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 3.548ms <span style="color:green">(2.2x)</span>&nbsp;🟢 | 7.786ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 64.451ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 113.344ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.006ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.005ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.069ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | 0.101ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(1000000)` | 2.374ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 2.795ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(10000)` | 0.114ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.330ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(100000)` | 1.097ms <span style="color:green">(4.0x)</span>&nbsp;🟢 | 4.351ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(1000000)` | 10.606ms <span style="color:green">(15.4x)</span>&nbsp;🟢 | 163.566ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.103ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 0.184ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.096ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 1.780ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.832ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 17.327ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(10000)` | 0.115ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 0.321ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.574ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 4.194ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 63.588ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 178.633ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.114ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 0.280ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 2.438ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 5.013ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 111.527ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | 168.164ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(10000)` | 0.106ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.309ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(100000)` | 1.043ms <span style="color:green">(3.8x)</span>&nbsp;🟢 | 3.965ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(1000000)` | 10.588ms <span style="color:green">(15.9x)</span>&nbsp;🟢 | 168.040ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---
