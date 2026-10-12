# ⚡ TailorBench Mark1 — CPU Benchmark Tool

A WPF desktop application that benchmarks your CPU's single-core and multi-core performance, with real-time CPU usage monitoring and interactive charts.

![.NET](https://img.shields.io/badge/.NET-6.0-blue)
![WPF](https://img.shields.io/badge/WPF-Desktop_App-purple)
![LiveCharts](https://img.shields.io/badge/LiveCharts-Data_Visualization-orange)

## Features

- **Single & Multi Core Benchmarking** — Runs CPU stress tests and reports performance scores
- **Real-time CPU Monitoring** — Tracks CPU usage in real-time during the benchmark
- **Live Charts** — Visualizes both real-time CPU usage and benchmark results on an interactive graph
- **System Info Detection** — Automatically detects your processor and OS
- **Custom Number Input** — Enter and save 10 custom numbers (0–100) for testing
- **Dark Theme UI** — Clean, modern dark interface



The app features a split-panel layout:
- **Left Panel** — System info, benchmark scores, and controls
- **Right Panel** — Live line chart with CPU usage and benchmark data

## Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- Windows OS (WPF is Windows-only)
- Visual Studio 2022 (recommended) or VS Code with C# extension

## Setup

### 1. Clone the repository

```bash
git clone https://github.com/<your-username>/tailorbench.git
cd tailorbench
```

### 2. Restore NuGet packages and build

```bash
dotnet restore
dotnet build
```

### 3. Run the application

```bash
dotnet run --project TailorBench
```

Or open `TailorBench.sln` in Visual Studio and press **F5**.

### 4. Add the benchmark executable

Place your `CPUBenchmark.exe` in the build output directory (`bin/Debug/net6.0-windows/`). The app expects it in the same folder as the running executable.

## How It Works

1. **Start Benchmark** — Click the button to launch `CPUBenchmark.exe`
2. **CPU Monitoring** — A background task polls CPU usage every second and plots it live
3. **Score Parsing** — The app reads `Single:` and `Multi:` scores from the benchmark's stdout
4. **Graph Data** — After the benchmark finishes, it reads `Graph.txt` to plot benchmark results
5. **Save Numbers** — Enter 10 comma-separated numbers and save them to `SavedNumbers.txt`

## Project Structure

```
├── TailorBench.sln                 # Visual Studio solution
├── TailorBench/
│   ├── TailorBench.csproj          # Project file (.NET 6.0 WPF)
│   ├── App.xaml                    # Application entry point
│   ├── App.xaml.cs                 # Application code-behind
│   ├── MainWindow.xaml             # Main UI layout (dark theme)
│   └── MainWindow.xaml.cs          # Benchmark logic, CPU monitoring, chart binding
├── .gitignore
└── README.md
```

## Tech Stack

- **Framework**: .NET 6.0, WPF
- **Charting**: LiveCharts.Wpf
- **System Monitoring**: System.Diagnostics.PerformanceCounter
- **Language**: C#


