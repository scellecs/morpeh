# Performance Comparison: Collection Benchmarks

Benchmark run on AMD Ryzen 7 2700 Eight-Core Processor  with 16 logical cores.

Unity Editor version: 2022.3.49f1

Scripting Backend: IL2CPP

### *FastListPerformanceTests*

| Functionality | FastList (Morpeh) | List (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.023ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *0.026ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Add(100000)` | 0.227ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | *0.264ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Add(1000000)` | 2.318ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | *2.706ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `AddGrow(10000)` | 0.029ms <span style="color:green">(2.8x)</span>&nbsp;🟢 | *0.083ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `AddGrow(100000)` | 0.657ms <span style="color:red">(0.8x)</span>&nbsp;🟠 | *0.517ms <span style="color:green">(1.3x)</span>*&nbsp;🟢 |
| `AddGrow(1000000)` | 4.247ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *4.488ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `ForEach(10000)` | 0.094ms <span style="color:green">(3.1x)</span>&nbsp;🟢 | *0.292ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `ForEach(100000)` | 0.947ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *1.069ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `ForEach(1000000)` | 9.298ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *9.829ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerRead(100000)` | 1.043ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *1.091ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerRead(1000000)` | 9.799ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | *11.873ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerReadDirect(100000)` | 0.993ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *1.044ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerReadDirect(1000000)` | 9.955ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | *11.691ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWrite(100000)` | 0.364ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | *0.631ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWrite(1000000)` | 4.867ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | *7.102ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWriteDirect(100000)` | 0.283ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | *0.600ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWriteDirect(1000000)` | 5.172ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | *7.295ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Remove(10000)` | 3.940ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *4.169ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Remove(100000)` | 525.513ms <span style="color:red">(0.9x)</span>&nbsp;🟠 | *466.496ms <span style="color:green">(1.1x)</span>*&nbsp;🟢 |
| `RemoveAt(10000)` | 1.534ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | *1.472ms <span style="color:green">(1.0x)</span>*&nbsp;🟢 |
| `RemoveAt(100000)` | 177.689ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *179.551ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtFast(10000)` | 1.482ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | *1.408ms <span style="color:green">(1.1x)</span>*&nbsp;🟢 |
| `RemoveAtFast(100000)` | 164.227ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *171.805ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtSwapBack(10000)` | 0.041ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | *0.086ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtSwapBack(100000)` | 0.451ms <span style="color:green">(2.2x)</span>&nbsp;🟢 | *1.010ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtSwapBackFast(10000)` | 0.040ms <span style="color:green">(2.2x)</span>&nbsp;🟢 | *0.088ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtSwapBackFast(100000)` | 0.524ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | *0.812ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveRange(10000)` | 0.004ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | *0.004ms <span style="color:green">(1.0x)</span>*&nbsp;🟢 |
| `RemoveRange(100000)` | 0.037ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | *0.036ms <span style="color:green">(1.0x)</span>*&nbsp;🟢 |

---

### *IntHashMapPerformanceTests*

| Functionality | IntHashMap (Morpeh) | Dictionary (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.063ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | *0.129ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Add(100000)` | 0.935ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | *1.599ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Add(1000000)` | 11.717ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | *21.328ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `AddGrow(10000)` | 0.130ms <span style="color:green">(2.4x)</span>&nbsp;🟢 | *0.313ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `AddGrow(100000)` | 2.874ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | *3.992ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `AddGrow(1000000)` | 29.424ms <span style="color:green">(2.3x)</span>&nbsp;🟢 | *67.704ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `ForEach(10000)` | 0.093ms <span style="color:green">(1.9x)</span>&nbsp;🟢 | *0.175ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `ForEach(100000)` | 1.079ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | *1.939ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `ForEach(1000000)` | 10.321ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | *18.196ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerRead(10000)` | 0.118ms <span style="color:green">(2.7x)</span>&nbsp;🟢 | *0.317ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerRead(100000)` | 1.553ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | *4.038ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerRead(1000000)` | 53.579ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | *154.409ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Remove(10000)` | 0.099ms <span style="color:green">(2.6x)</span>&nbsp;🟢 | *0.262ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Remove(100000)` | 1.856ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | *3.358ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Remove(1000000)` | 88.229ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | *159.690ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `TryGetValue(10000)` | 0.113ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | *0.334ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `TryGetValue(100000)` | 1.070ms <span style="color:green">(3.6x)</span>&nbsp;🟢 | *3.838ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `TryGetValue(1000000)` | 10.705ms <span style="color:green">(14.2x)</span>&nbsp;🟢 | *152.526ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |

---
