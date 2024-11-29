# Performance Comparison: Collection Benchmarks

Benchmark run on AMD Ryzen 7 2700 Eight-Core Processor  with 16 logical cores.
Unity Editor version: 2022.3.49f1

### *FastList*

| Functionality | FastList<int> (Morpeh) | *List<int> (BCL)* |
|---|--:|--:|
| `{ MethodName = IndexerRead }(10000)` | 0.115ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *0.124ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |
| `{ MethodName = IndexerRead }(100000)` | 1.150ms <span style="color:green">(1.1x)</span>&nbsp;🟢 | *1.236ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |
| `{ MethodName = IndexerRead }(1000000)` | 10.101ms <span style="color:green">(1.4x)</span>&nbsp;🟢 | *14.134ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |
| `{ MethodName = IndexerWrite }(10000)` | 0.034ms <span style="color:green">(1.7x)</span>&nbsp;🟢 | *0.058ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |
| `{ MethodName = IndexerWrite }(100000)` | 0.349ms <span style="color:green">(1.8x)</span>&nbsp;🟢 | *0.612ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |
| `{ MethodName = IndexerWrite }(1000000)` | 5.941ms <span style="color:green">(1.3x)</span>&nbsp;🟢 | *7.532ms <span style="color:grey">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |

---
