using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Wpf;

namespace WpfApp2
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ChartValues<double> _chartValues;
        private ChartValues<double> _benchmarkValues;
        private string _processorName;
        private string _operatingSystem;
        private double _singleCoreScore;
        private double _multiCoreScore;
        private bool _isBenchmarkRunning;

        private readonly string BenchmarkPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CPUBenchmark.exe");
        private readonly PerformanceCounter cpuCounter;

        public ChartValues<double> ChartValues
        {
            get => _chartValues;
            set
            {
                _chartValues = value;
                OnPropertyChanged();
            }
        }

        public ChartValues<double> BenchmarkValues
        {
            get => _benchmarkValues;
            set
            {
                _benchmarkValues = value;
                OnPropertyChanged();
            }
        }

        public string ProcessorName
        {
            get => _processorName;
            set
            {
                _processorName = value;
                OnPropertyChanged();
            }
        }

        public string OperatingSystem
        {
            get => _operatingSystem;
            set
            {
                _operatingSystem = value;
                OnPropertyChanged();
            }
        }

        public double SingleCoreScore
        {
            get => _singleCoreScore;
            set
            {
                _singleCoreScore = value;
                OnPropertyChanged();
            }
        }

        public double MultiCoreScore
        {
            get => _multiCoreScore;
            set
            {
                _multiCoreScore = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            ChartValues = new ChartValues<double>();
            BenchmarkValues = new ChartValues<double>();
            DataContext = this;
            GetSystemInfo();

            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing performance counter: {ex.Message}");
            }
        }

        private void GetSystemInfo()
        {
            try
            {
                ProcessorName = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Unknown Processor";
                OperatingSystem = Environment.OSVersion.ToString();
                OperatingSystem += Environment.Is64BitOperatingSystem ? " 64-bit" : " 32-bit";
            }
            catch (Exception ex)
            {
                ProcessorName = "Unable to detect";
                OperatingSystem = "Unable to detect";
                MessageBox.Show($"Error getting system info: {ex.Message}", "System Info Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void StartBenchmark_Click(object sender, RoutedEventArgs e)
        {
            if (_isBenchmarkRunning) return;

            _isBenchmarkRunning = true;
            StartButton.IsEnabled = false;
            ChartValues.Clear();
            BenchmarkValues.Clear();

            try
            {
                if (!File.Exists(BenchmarkPath))
                {
                    MessageBox.Show($"Error: CPUBenchmark.exe not found at:\n{BenchmarkPath}", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var monitoringTask = MonitorCPUUsageAsync();

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = BenchmarkPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(BenchmarkPath)
                    }
                };

                process.OutputDataReceived += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        ParseBenchmarkOutput(args.Data);
                    }
                };

                process.Exited += (s, args) =>
                {
                    _isBenchmarkRunning = false;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StartButton.IsEnabled = true;
                        PlotGraphFromFile();
                    });
                };

                process.EnableRaisingEvents = true;
                process.Start();
                process.BeginOutputReadLine();

                await process.WaitForExitAsync();

                _isBenchmarkRunning = false;
                await monitoringTask;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running benchmark: {ex.Message}", "Execution Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isBenchmarkRunning = false;
                StartButton.IsEnabled = true;
            }
        }

        private async Task MonitorCPUUsageAsync()
        {
            if (cpuCounter == null) return;

            while (_isBenchmarkRunning)
            {
                try
                {
                    var cpuUsage = cpuCounter.NextValue();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ChartValues.Add(cpuUsage); // Add real-time CPU usage to the graph
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error monitoring CPU: {ex.Message}");
                }

                await Task.Delay(1000); // Wait for 1 second before collecting the next data point
            }
        }

        private void ParseBenchmarkOutput(string output)
        {
            try
            {
                if (output.Contains("Single:"))
                {
                    var score = ParseScore(output, "Single:");
                    Application.Current.Dispatcher.Invoke(() => SingleCoreScore = score);
                }
                else if (output.Contains("Multi:"))
                {
                    var score = ParseScore(output, "Multi:");
                    Application.Current.Dispatcher.Invoke(() => MultiCoreScore = score);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing benchmark output: {ex.Message}");
            }
        }

        private double ParseScore(string output, string prefix)
        {
            var scoreStr = output.Substring(output.IndexOf(prefix) + prefix.Length).Trim();
            return double.TryParse(scoreStr, out double score) ? score : 0;
        }

        private void PlotGraphFromFile()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Graph.txt");

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Graph data file not found!", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    BenchmarkValues.Clear(); // Clear previous benchmark values

                    foreach (var line in lines)
                    {
                        Debug.WriteLine($"Reading line: {line}");

                        // Extract the numeric value from the line
                        var colonIndex = line.IndexOf(':');
                        if (colonIndex >= 0 && double.TryParse(line.Substring(colonIndex + 1), out double value))
                        {
                            BenchmarkValues.Add(value); // Add values to benchmark graph
                            Debug.WriteLine($"Parsed benchmark value: {value}");
                        }
                        else
                        {
                            Debug.WriteLine("Skipping invalid line format");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading graph data: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveNumbers_Click(object sender, RoutedEventArgs e)
        {
            string userInput = NumberInput.Text;
            var numbers = userInput.Split(',').Select(s => s.Trim()).ToList();

            if (numbers.Count != 10 || numbers.Any(n => !double.TryParse(n, out _)))
            {
                MessageBox.Show("Please enter exactly 10 numbers between 0 and 100, separated by commas.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SavedNumbers.txt");
                File.WriteAllLines(filePath, numbers);
                NumberInput.Clear();
                MessageBox.Show("Numbers saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving numbers: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
