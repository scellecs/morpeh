# Performance Comparison: Collection Benchmarks

Benchmark run on AMD Ryzen 7 2700 Eight-Core Processor  with 16 logical cores.

Unity Editor version: 2022.3.49f1

Scripting Backend: IL2CPP

### *FastList*

| Functionality | FastList (Morpeh) | List (BCL) |
|---|--:|--:|
| `ForEach(10000)` | 0.106ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *0.110ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `ForEach(100000)` | 1.010ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *1.115ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `ForEach(1000000)` | 9.361ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | *11.073ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerRead(100000)` | 1.043ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *1.074ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerRead(1000000)` | 9.861ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | *12.148ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerReadDirect(100000)` | 1.001ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *1.054ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerReadDirect(1000000)` | 10.471ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | *12.211ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWrite(100000)` | 0.361ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | *0.578ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWrite(1000000)` | 5.344ms <span style="color:green">(1.6x)</span>&nbsp;🟢 | *8.398ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWriteDirect(100000)` | 0.269ms <span style="color:green">(2.2x)</span>&nbsp;🟢 | *0.584ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWriteDirect(1000000)` | 4.321ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | *7.582ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Remove(10000)` | 4.363ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | *4.197ms <span style="color:green">(1.0x)</span>*&nbsp;🟢 |
| `Remove(100000)` | 476.970ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *484.376ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAt(10000)` | 1.512ms <span style="color:red">(0.9x)</span>&nbsp;🟠 | *1.429ms <span style="color:green">(1.1x)</span>*&nbsp;🟢 |
| `RemoveAt(100000)` | 153.895ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *154.698ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtFast(10000)` | 1.981ms <span style="color:red">(0.7x)</span>&nbsp;🟠 | *1.374ms <span style="color:green">(1.4x)</span>*&nbsp;🟢 |
| `RemoveAtFast(100000)` | 153.781ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *156.327ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtSwapBack(10000)` | 0.057ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | *0.079ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtSwapBack(100000)` | 0.452ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | *0.831ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtSwapBackFast(10000)` | 0.041ms <span style="color:green">(2.1x)</span>&nbsp;🟢 | *0.087ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveAtSwapBackFast(100000)` | 0.419ms <span style="color:green">(2.0x)</span>&nbsp;🟢 | *0.856ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `RemoveRange(10000)` | 0.004ms <span style="color:red">(1.0x)</span>&nbsp;🟠 | *0.004ms <span style="color:green">(1.0x)</span>*&nbsp;🟢 |
| `RemoveRange(100000)` | 0.034ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *0.035ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |

---
