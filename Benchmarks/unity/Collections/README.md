# Performance Comparison: Collection Benchmarks

## Environment

Benchmark run on AMD Ryzen 7 2700 Eight-Core Processor  with 16 logical cores.

Unity Editor version: 2022.3.49f1

Scripting Backend: IL2CPP

## Results

### *BitSetPerformanceTests*

| Functionality | BitSet (Morpeh) | BitArray (BCL) |
|---|--:|--:|
| `Clear(10000)` | 0.001ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.001ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.002ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.002ms <span style="color:green">(1.2x)</span>&nbsp;🟢 |
| `Clear(1000000)` | 0.010ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.020ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ClearManually(10000)` | 0.037ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 0.048ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ClearManually(100000)` | 0.392ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.465ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ClearManually(1000000)` | 3.984ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 4.745ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(10000)` | 0.114ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.120ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(100000)` | 1.142ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.227ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IsSet(1000000)` | 10.551ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 11.183ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(10000)` | 0.038ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.043ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(100000)` | 0.393ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.433ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Set(1000000)` | 4.389ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 4.981ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(10000)` | 0.047ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 0.094ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(100000)` | 0.428ms <span style="color:green">(4.2x)</span>&nbsp;🟢 | 1.784ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `SetGrow(1000000)` | 4.084ms <span style="color:green">(7.0x)</span>&nbsp;🟢 | 28.480ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(10000)` | 0.038ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.044ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(100000)` | 0.409ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.426ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Unset(1000000)` | 3.981ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 4.844ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *FastListPerformanceTests*

| Functionality | FastList (Morpeh) | List (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.022ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.025ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.402ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.390ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Add(1000000)` | 3.499ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 3.225ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `AddGrow(10000)` | 0.055ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.051ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `AddGrow(100000)` | 0.312ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 0.509ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 4.886ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 5.211ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.001ms <span style="color:green">(3.0x)</span>&nbsp;🟢 | 0.000ms <span style="color:green">(3.0x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.007ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.000ms <span style="color:green">(23.3x)</span>&nbsp;🟢 |
| `Clear(1000000)` | 0.081ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.001ms <span style="color:green">(59.9x)</span>&nbsp;🟢 |
| `ForEach(10000)` | 0.098ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 0.130ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.016ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.036ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.656ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 10.032ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.051ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 1.113ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 9.892ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 12.082ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerReadDirect(100000)` | 0.869ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 1.054ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerReadDirect(1000000)` | 9.862ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 12.364ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWrite(100000)` | 0.356ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 0.576ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWrite(1000000)` | 4.495ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 7.792ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWriteDirect(100000)` | 0.286ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 0.591ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerWriteDirect(1000000)` | 4.963ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | 7.708ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 3.985ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 4.212ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 489.890ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 490.348ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAt(10000)` | 1.508ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.479ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAt(100000)` | 157.328ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 157.393ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtFast(10000)` | 1.416ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 1.299ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `RemoveAtFast(100000)` | 157.645ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 157.035ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveAtSwapBack(10000)` | 0.058ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 0.080ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBack(100000)` | 0.471ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 0.983ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBackFast(10000)` | 0.040ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | 0.085ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveAtSwapBackFast(100000)` | 0.417ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.808ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `RemoveRange(10000)` | 0.006ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.006ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `RemoveRange(100000)` | 0.032ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.034ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntHashMapPerformanceTests*

| Functionality | IntHashMap (Morpeh) | Dictionary (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.059ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 0.114ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.868ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 1.511ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 10.066ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 18.322ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.118ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 0.277ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 1.410ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | 2.763ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 35.582ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 42.016ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.005ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.004ms <span style="color:green">(1.4x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.030ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | 0.044ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(1000000)` | 1.460ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 1.883ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(10000)` | 0.098ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.281ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(100000)` | 0.983ms <span style="color:green">(3.6x)</span>&nbsp;🟢 | 3.562ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(1000000)` | 10.622ms <span style="color:green">(13.0x)</span>&nbsp;🟢 | 138.161ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.096ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 0.173ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 0.978ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 1.688ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.785ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 16.786ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(10000)` | 0.109ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 0.308ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.463ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 3.691ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 49.100ms <span style="color:green">(3.2x)</span>&nbsp;🟢 | 155.518ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.099ms <span style="color:green">(3.3x)</span>&nbsp;🟢 | 0.325ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.650ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 3.115ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 85.496ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 161.718ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(10000)` | 0.102ms <span style="color:green">(3.1x)</span>&nbsp;🟢 | 0.316ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(100000)` | 1.036ms <span style="color:green">(3.6x)</span>&nbsp;🟢 | 3.685ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(1000000)` | 9.829ms <span style="color:green">(16.4x)</span>&nbsp;🟢 | 161.170ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntHashSetPerformanceTests*

| Functionality | IntHashSet (Morpeh) | HashSet (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.052ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.151ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 0.803ms <span style="color:green">(2.3x)</span>&nbsp;🟢 | 1.864ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 9.392ms <span style="color:green">(2.3x)</span>&nbsp;🟢 | 21.813ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.132ms <span style="color:green">(2.2x)</span>&nbsp;🟢 | 0.291ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 1.198ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 2.824ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 17.647ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 44.629ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.004ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.004ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.023ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | 0.029ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(1000000)` | 1.064ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | 1.478ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.102ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 0.112ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 1.003ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.023ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.772ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.930ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(10000)` | 0.104ms <span style="color:green">(3.1x)</span>&nbsp;🟢 | 0.325ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(100000)` | 0.984ms <span style="color:green">(4.3x)</span>&nbsp;🟢 | 4.181ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Has(1000000)` | 9.826ms <span style="color:green">(19.5x)</span>&nbsp;🟢 | 191.387ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.087ms <span style="color:green">(3.1x)</span>&nbsp;🟢 | 0.275ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.511ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 3.747ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 77.794ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | 187.460ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *IntStackPerformanceTests*

| Functionality | IntStack (Morpeh) | Stack (BCL) |
|---|--:|--:|
| `Pop(10000)` | 0.098ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.101ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Pop(100000)` | 0.960ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.985ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Pop(1000000)` | 9.542ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | 10.563ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Push(10000)` | 0.025ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.025ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Push(100000)` | 0.326ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.254ms <span style="color:green">(1.3x)</span>&nbsp;🟢 |
| `Push(1000000)` | 3.945ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 3.659ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `PushGrow(10000)` | 0.069ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.038ms <span style="color:green">(1.8x)</span>&nbsp;🟢 |
| `PushGrow(100000)` | 0.472ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.411ms <span style="color:green">(1.1x)</span>&nbsp;🟢 |
| `PushGrow(1000000)` | 6.551ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 4.687ms <span style="color:green">(1.4x)</span>&nbsp;🟢 |
| `TryPop(10000)` | 0.099ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 0.114ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryPop(100000)` | 1.064ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 1.089ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryPop(1000000)` | 10.093ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 10.260ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---

### *LongHashMapPerformanceTests*

| Functionality | LongHashMap (Morpeh) | Dictionary (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.089ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 0.228ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(100000)` | 1.460ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 2.752ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Add(1000000)` | 44.894ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | 68.625ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(10000)` | 0.166ms <span style="color:green">(2.5x)</span>&nbsp;🟢 | 0.410ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(100000)` | 3.565ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 4.415ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `AddGrow(1000000)` | 52.246ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | 95.779ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Clear(10000)` | 0.005ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | 0.005ms <span style="color:green">(1.0x)</span>&nbsp;🟢 |
| `Clear(100000)` | 0.100ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | 0.085ms <span style="color:green">(1.2x)</span>&nbsp;🟢 |
| `Clear(1000000)` | 2.306ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | 2.743ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(10000)` | 0.098ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.283ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(100000)` | 0.984ms <span style="color:green">(4.0x)</span>&nbsp;🟢 | 3.905ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ContainsKey(1000000)` | 9.851ms <span style="color:green">(16.7x)</span>&nbsp;🟢 | 164.190ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(10000)` | 0.098ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 0.168ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(100000)` | 0.980ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 1.672ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `ForEach(1000000)` | 9.735ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | 16.746ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(10000)` | 0.108ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | 0.291ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(100000)` | 1.513ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 3.921ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `IndexerRead(1000000)` | 64.634ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | 181.993ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(10000)` | 0.097ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | 0.250ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(100000)` | 1.771ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 3.323ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `Remove(1000000)` | 92.255ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | 177.482ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(10000)` | 0.114ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | 0.337ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(100000)` | 1.056ms <span style="color:green">(3.8x)</span>&nbsp;🟢 | 4.041ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |
| `TryGetValue(1000000)` | 9.835ms <span style="color:green">(17.9x)</span>&nbsp;🟢 | 176.503ms <span style="color:red">(1.0x)</span>&nbsp;🟠 |

---
