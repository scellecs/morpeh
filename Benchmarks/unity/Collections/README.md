# Performance Comparison: Collection Benchmarks

Benchmark run on AMD Ryzen 7 2700 Eight-Core Processor  with 16 logical cores.

Unity Editor version: 2022.3.49f1

Scripting Backend: IL2CPP

### *BitSetPerformanceTests*

| Functionality | BitSet (Morpeh) | BitArray (BCL) |
|---|--:|--:|
| `IsSet(10000)` | 0.110ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.120ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(100000)` | 1.068ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.052ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `IsSet(1000000)` | 11.387ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 12.107ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(10000)` | 0.042ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.052ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(100000)` | 0.425ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.455ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(1000000)` | 6.232ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 5.875ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `SetGrow(10000)` | 0.043ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.083ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(100000)` | 0.433ms <span style="color:green">(3.9x)</span>&nbsp;🟢 | 1.683ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(1000000)` | 4.325ms <span style="color:green">(7.2x)</span>&nbsp;🟢 | 31.183ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(10000)` | 0.041ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 0.052ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(100000)` | 0.382ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.467ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(1000000)` | 4.371ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 5.535ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *FastListPerformanceTests*

| Functionality | FastList (Morpeh) | List (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.024ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.027ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.431ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 0.605ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 4.234ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 2.855ms <span style="color:green">(1.5x)</span>&nbsp;🟢 |
| `AddGrow(10000)` | 0.079ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.059ms <span style="color:green">(1.3x)</span>&nbsp;🟢 |
| `AddGrow(100000)` | 0.664ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.520ms <span style="color:green">(1.3x)</span>&nbsp;🟢 |
| `AddGrow(1000000)` | 4.616ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 5.145ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.108ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.112ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.074ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.087ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 11.038ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 10.979ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `IndexerRead(100000)` | 1.057ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.214ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 10.843ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 13.920ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerReadDirect(100000)` | 1.098ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.122ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerReadDirect(1000000)` | 10.835ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 13.137ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWrite(100000)` | 0.374ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 0.642ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWrite(1000000)` | 5.638ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 7.848ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWriteDirect(100000)` | 0.319ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 0.679ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWriteDirect(1000000)` | 4.712ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 8.845ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 4.185ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 4.164ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Remove(100000)` | 481.627ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 474.832ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAt(10000)` | 1.377ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.335ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAt(100000)` | 157.093ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 158.258ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtFast(10000)` | 1.375ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.350ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAtFast(100000)` | 155.707ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 169.826ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBack(10000)` | 0.046ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.092ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBack(100000)` | 0.446ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.871ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBackFast(10000)` | 0.041ms <span style="color:green">(3.3x)</span>&nbsp;🟢 | 0.133ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBackFast(100000)` | 0.584ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | 0.876ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveRange(10000)` | 0.006ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.006ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveRange(100000)` | 0.034ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.035ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |

---

### *IntHashMapPerformanceTests*

| Functionality | IntHashMap (Morpeh) | Dictionary (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.067ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.132ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.940ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 1.496ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 10.100ms <span style="color:green">(2.3x)</span>&nbsp;🟢 | 22.776ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.125ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 0.297ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 1.448ms <span style="color:green">(3.7x)</span>&nbsp;🟢 | 5.303ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 35.436ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 48.603ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(10000)` | 0.161ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.304ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(100000)` | 1.150ms <span style="color:green">(3.3x)</span>&nbsp;🟢 | 3.800ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(1000000)` | 10.554ms <span style="color:green">(12.7x)</span>&nbsp;🟢 | 134.131ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.158ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.180ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.059ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 1.836ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 10.317ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 17.846ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(10000)` | 0.129ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 0.318ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.557ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 4.037ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 53.169ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 154.399ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.099ms <span style="color:green">(3.5x)</span>&nbsp;🟢 | 0.343ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.801ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 3.386ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 91.304ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 159.283ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(10000)` | 0.104ms <span style="color:green">(3.0x)</span>&nbsp;🟢 | 0.310ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(100000)` | 1.132ms <span style="color:green">(4.2x)</span>&nbsp;🟢 | 4.789ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(1000000)` | 11.405ms <span style="color:green">(14.0x)</span>&nbsp;🟢 | 159.511ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntHashSetPerformanceTests*

| Functionality | IntHashSet (Morpeh) | HashSet (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.058ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 0.161ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.804ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 1.930ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 9.899ms <span style="color:green">(2.2x)</span>&nbsp;🟢 | 21.444ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.118ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 0.308ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 1.213ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 3.010ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 18.823ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 50.015ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.111ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.118ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.114ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.187ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 11.140ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 11.907ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(10000)` | 0.114ms <span style="color:green">(4.4x)</span>&nbsp;🟢 | 0.503ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(100000)` | 1.140ms <span style="color:green">(4.3x)</span>&nbsp;🟢 | 4.860ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(1000000)` | 11.400ms <span style="color:green">(19.4x)</span>&nbsp;🟢 | 221.009ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.101ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.293ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 2.074ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 4.896ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 80.959ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 204.726ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntStackPerformanceTests*

| Functionality | IntStack (Morpeh) | Stack (BCL) |
|---|--:|--:|
| `Pop(10000)` | 0.114ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.117ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Pop(100000)` | 1.134ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.163ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Pop(1000000)` | 11.433ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 11.640ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Push(10000)` | 0.088ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.028ms <span style="color:green">(3.2x)</span>&nbsp;🟢 |
| `Push(100000)` | 0.575ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.278ms <span style="color:green">(2.1x)</span>&nbsp;🟢 |
| `Push(1000000)` | 7.163ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 3.227ms <span style="color:green">(2.2x)</span>&nbsp;🟢 |
| `PushGrow(10000)` | 0.092ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.044ms <span style="color:green">(2.1x)</span>&nbsp;🟢 |
| `PushGrow(100000)` | 0.621ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.436ms <span style="color:green">(1.4x)</span>&nbsp;🟢 |
| `PushGrow(1000000)` | 7.717ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 5.103ms <span style="color:green">(1.5x)</span>&nbsp;🟢 |
| `TryPop(10000)` | 0.114ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.119ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryPop(100000)` | 1.139ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.193ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryPop(1000000)` | 11.339ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 11.914ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *LongHashMapPerformanceTests*

| Functionality | LongHashMap (Morpeh) | Dictionary (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.101ms <span style="color:green">(2.3x)</span>&nbsp;🟢 | 0.233ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 1.651ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 3.024ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 51.170ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 65.988ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.169ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 0.421ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 2.818ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 4.546ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 54.523ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 92.517ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(10000)` | 0.111ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.317ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(100000)` | 1.060ms <span style="color:green">(4.2x)</span>&nbsp;🟢 | 4.501ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(1000000)` | 11.150ms <span style="color:green">(15.9x)</span>&nbsp;🟢 | 176.976ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.115ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 0.194ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.147ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 1.969ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 11.364ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 19.485ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(10000)` | 0.125ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 0.335ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.844ms <span style="color:green">(3.1x)</span>&nbsp;🟢 | 5.640ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 69.220ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 187.558ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.115ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 0.278ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 2.190ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 4.464ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 97.998ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 174.392ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(10000)` | 0.106ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.308ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(100000)` | 1.060ms <span style="color:green">(3.9x)</span>&nbsp;🟢 | 4.143ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(1000000)` | 10.265ms <span style="color:green">(17.1x)</span>&nbsp;🟢 | 175.196ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---
