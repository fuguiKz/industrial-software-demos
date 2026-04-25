# Industrial Software Demos

Resume-aligned demo projects for industrial software / MES / machine vision scenarios.

## Projects

### 1. `MesDashboardDemo.Api`
- Stack: `ASP.NET Core Minimal API + static frontend + ECharts`
- Focus: OEE, Yield, CT, alert handling, equipment loading, dispatch recommendation
- Why it matches the resume:
  - shows `.NET WebAPI` and production dashboard thinking
  - uses manufacturing metrics instead of generic CRUD
  - reflects MES integration and operator / engineer facing views

### 2. `TraceabilityDemo.Api`
- Stack: `ASP.NET Core Minimal API + static frontend`
- Focus: material-image traceability, station route replay, AI review result presentation
- Why it matches the resume:
  - maps directly to material / image correlation and traceability tooling
  - shows how AOI review data can be structured for engineers
  - demonstrates business abstraction for WPF or upper-machine style systems

### 3. `SecsGemWorkbench`
- Stack: `C# console app`
- Focus: SECS/GEM scenario modeling for alarm/event collection, remote commands, wafer map exchange
- Why it matches the resume:
  - avoids low-level “remember stream/function numbers” presentation
  - emphasizes interface alignment, host action, and business meaning
  - stronger interview signal for equipment integration work

## Quick Start

### Build all

```bash
dotnet build industrial-software-demos.sln
```

### Run MES dashboard demo

```bash
dotnet run --project src/MesDashboardDemo.Api --urls http://127.0.0.1:5071
```

Open `http://127.0.0.1:5071`

### Run traceability demo

```bash
dotnet run --project src/TraceabilityDemo.Api --urls http://127.0.0.1:5072
```

Open `http://127.0.0.1:5072`

### Run SECS/GEM workbench

```bash
dotnet run --project src/SecsGemWorkbench -- overview
dotnet run --project src/SecsGemWorkbench -- alarm
dotnet run --project src/SecsGemWorkbench -- remote-command
dotnet run --project src/SecsGemWorkbench -- wafer-map --json
```

## Demo Notes

- The web demos use seeded in-memory data so they can run immediately without database setup.
- `MesDashboardDemo.Api` loads ECharts from CDN for faster setup. Core API behavior does not depend on external services.
- The goal is not full factory software simulation. The goal is to present believable, interview-ready slices of industrial software work.

## Suggested Repo Positioning

If you pin this repo on GitHub, the positioning can be:

> Industrial software demo collection focused on MES dashboards, traceability tooling, and SECS/GEM integration scenarios using C# / .NET.

## Structure

```text
src/
  MesDashboardDemo.Api/
  TraceabilityDemo.Api/
  SecsGemWorkbench/
docs/
industrial-software-demos.sln
```
