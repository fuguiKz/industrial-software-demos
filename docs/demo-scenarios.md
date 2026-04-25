# Demo Scenarios

## MES Dashboard Demo

Use this when someone asks:

- 你做过什么 WebAPI / 看板类项目？
- 你怎么把产线数据做成工程/制造都能看的东西？
- OEE、良率、CT 这些指标你理解到什么程度？

Recommended explanation:

1. The API exposes overview, hourly trend, defect mix, alert feed, and equipment loading.
2. The UI is intentionally framed as an engineer-facing dashboard rather than a BI screenshot.
3. Dispatch recommendation is added to show decision support, not just reporting.

## Traceability Demo

Use this when someone asks:

- 你做过物料影像绑定、回溯、异常追踪吗？
- 机器视觉复判怎么和生产数据打通？
- 上位机 / 工程工具你具体做过什么？

Recommended explanation:

1. Search by serial or lot.
2. Replay the station route and equipment path.
3. Show how AOI image review, AI result, and engineering judgement can live in one object model.

## SECS/GEM Workbench

Use this when someone asks:

- 你对 SECS/GEM 理解到什么程度？
- 你做的是背协议还是做设备对接？
- 你怎么理解 wafer map、报警事件、远程命令这些接口？

Recommended explanation:

1. This demo is scenario-oriented, not packet-memorization oriented.
2. Alarm/event, remote command, and wafer map are framed as host-equipment business workflows.
3. The focus is on validation, host action, and traceability binding.

## WPF Traceability Studio

Use this when someone asks:

- 你 WPF / MVVM 具体做过什么？
- 你说的“物料-影像关联分析系统”是什么样子？
- AOI 复判和工程人员使用界面怎么结合？

Recommended explanation:

1. It is intentionally built as an engineer workstation, not a visual toy.
2. Search, route replay, image cards, AI decision and engineer notes are all bound through MVVM.
3. The design demonstrates how a traceability tool can stay maintainable while still being workflow-oriented.

## WinForms EAP Gate Tool

Use this when someone asks:

- 你 WinForms 做过什么，不只是“做过界面”吧？
- 你提到的 Lock/Release 逻辑能具体点吗？
- 设备上控、recipe 校验、Wafer Map 复核在工具里怎么体现？

Recommended explanation:

1. The tool models a real gate decision screen for production or engineering support.
2. It turns recipe checks, tooling readiness, wafer map validation and hold flags into a release decision.
3. The payload preview and event log help explain host-command thinking, not just form controls.
