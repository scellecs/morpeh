# Performance Comparison: Collection Benchmarks

Benchmark run on AMD Ryzen 7 2700 Eight-Core Processor  with 16 logical cores.
Unity Editor version: 2022.3.49f1

### *FastList*

| Functionality | FastList (Morpeh) | List (BCL) |
|---|--:|--:|
| `IndexerRead(10000)` | 0.115ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *0.124ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerRead(100000)` | 1.143ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *1.225ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerRead(1000000)` | 11.467ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | *14.700ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerReadDirect(10000)` | 0.106ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *0.112ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerReadDirect(100000)` | 1.055ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *1.172ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerReadDirect(1000000)` | 11.455ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *12.854ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWrite(10000)` | 0.039ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | *0.064ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWrite(100000)` | 0.403ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | *0.674ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWrite(1000000)` | 6.531ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | *9.616ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWriteDirect(10000)` | 0.030ms <span style="color:green">(2.2x)</span>&nbsp;🟢 | *0.066ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWriteDirect(100000)` | 0.567ms <span style="color:green">(1.2x)</span>&nbsp;🟢 | *0.668ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `IndexerWriteDirect(1000000)` | 6.376ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | *9.484ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Remove(10000)` | 4.545ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *4.917ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |
| `Remove(100000)` | 472.113ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *481.100ms <span style="color:red">(1.0x)</span>*&nbsp;🟠 |

---
