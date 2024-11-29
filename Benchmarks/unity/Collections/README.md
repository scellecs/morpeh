# Performance Comparison: Collection Benchmarks

Benchmark run on AMD Ryzen 7 2700 Eight-Core Processor  with 16 logical cores.
Unity Editor version: 2022.3.49f1

### *FastList*

| Functionality | FastList<int> (Morpeh) | *List<int> (BCL)* |
|---|--:|--:|
| `IndexerRead(10000)` | 0.114ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *0.123ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |
| `IndexerRead(100000)` | 1.142ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *1.232ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |
| `IndexerRead(1000000)` | 11.460ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | *15.524ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |
| `IndexerWrite(10000)` | 0.039ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | *0.052ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |
| `IndexerWrite(100000)` | 0.397ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | *0.668ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |
| `IndexerWrite(1000000)` | 6.236ms <span style="color:green">(1.5x)</span>&nbsp;🟢 | *9.294ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |
| `Remove(10000)` | 4.525ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *4.854ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |
| `Remove(100000)` | 526.547ms <span style="color:green">(1.0x)</span>&nbsp;🟢 | *528.558ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |

---
