# Performance Comparison: Collection Benchmarks

Benchmark run on AMD Ryzen 7 2700 Eight-Core Processor  with 16 logical cores.

Unity Editor version: 2022.3.49f1

Scripting Backend: IL2CPP

### *FastList*

| Functionality | FastList (Morpeh) | List (BCL) |
|---|--:|--:|
| `Add(10000)` | 0.024ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *0.027ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Add(100000)` | 0.237ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *0.272ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Add(1000000)` | 3.028ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | *3.578ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `AddGrow(10000)` | 0.032ms <span style="color:green">(2.9x)</span>&nbsp;🟢 | *0.093ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `AddGrow(100000)` | 0.706ms <span style="color:red">(0.8x)</span>&nbsp;🟠 | *0.550ms <span style="color:green">(1.3x)</span>*&nbsp;🟢 |
| `AddGrow(1000000)` | 6.015ms <span style="color:red">(0.9x)</span>&nbsp;🟠 | *5.229ms <span style="color:green">(1.2x)</span>*&nbsp;🟢 |
| `ForEach(10000)` | 0.108ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *0.111ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `ForEach(100000)` | 1.010ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *1.127ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `ForEach(1000000)` | 9.796ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *9.975ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerRead(100000)` | 1.070ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *1.096ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerRead(1000000)` | 10.614ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *12.166ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerReadDirect(100000)` | 1.055ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *1.167ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerReadDirect(1000000)` | 9.849ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | *12.220ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWrite(100000)` | 0.425ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | *0.563ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWrite(1000000)` | 5.472ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | *7.792ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWriteDirect(100000)` | 0.299ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | *0.590ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWriteDirect(1000000)` | 4.678ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | *7.561ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Remove(10000)` | 3.998ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *4.446ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Remove(100000)` | 497.765ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *508.927ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAt(10000)` | 1.383ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | *1.353ms <span style="color:green">(1.0x)</span>*&nbsp;🟢 |
| `RemoveAt(100000)` | 157.028ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *163.958ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtFast(10000)` | 1.339ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | *1.338ms <span style="color:green">(1.0x)</span>*&nbsp;🟢 |
| `RemoveAtFast(100000)` | 158.549ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | *153.902ms <span style="color:green">(1.0x)</span>*&nbsp;🟢 |
| `RemoveAtSwapBack(10000)` | 0.043ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | *0.087ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtSwapBack(100000)` | 0.441ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | *0.864ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtSwapBackFast(10000)` | 0.041ms <span style="color:green">(3.2x)</span>&nbsp;🟢 | *0.130ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtSwapBackFast(100000)` | 0.401ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | *0.857ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveRange(10000)` | 0.006ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | *0.006ms <span style="color:green">(1.0x)</span>*&nbsp;🟢 |
| `RemoveRange(100000)` | 0.034ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | *0.033ms <span style="color:green">(1.0x)</span>*&nbsp;🟢 |

---
