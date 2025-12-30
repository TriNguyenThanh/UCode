using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using UCode.Desktop.ViewModels;
using TitaniumProxy;

namespace UCode.Desktop.Services
{
    public class AIDetectorService : IDisposable
    {
        private ViewModelBase _viewModel;
        private ProxyService _proxyService;
        private System.Timers.Timer _checkTimer;
        private System.Timers.Timer _statsTimer;
        private AssignmentService _assignmentService;
        private string _currentAssignmentId;

        public AIDetectorService(AssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
            _viewModel = new ViewModelBase();
            _proxyService = new ProxyService();

            // Subscribe to AI detection events
            _proxyService.OnAiDetected += OnAiDetected;
            _proxyService.OnProxyReEnabled += OnProxyReEnabled;
        }

        /// <summary>
        /// Whether the AI detector (proxy) is currently running
        /// </summary>
        public bool IsRunning => _proxyService?.IsRunning ?? false;

        /// <summary>
        /// Get current detection statistics
        /// </summary>
        public Dictionary<string, int> GetStats() => _proxyService?.GetStats() ?? new Dictionary<string, int>();

        /// <summary>
        /// Get detailed statistics with DNS/Proxy breakdown
        /// </summary>
        public Dictionary<string, (int Total, int DNS, int Proxy, int UniqueHosts)> GetDetailedStats()
            => _proxyService?.GetDetailedStats() ?? new Dictionary<string, (int, int, int, int)>();

        /// <summary>
        /// Get statistics and clear counters
        /// </summary>
        public Dictionary<string, int> GetStatsAndClear()
            => _proxyService?.GetStatsAndClear() ?? new Dictionary<string, int>();

        public async Task<bool> ConfirmMessageAIDetector(string assignmentId)
        {
            if (IsRunning)
                return true;

            try
            {
                var result = await _viewModel.GetMetroWindow()?.ShowMessageAsync(
                            "Lưu ý quan trọng:",
                            "   - Đây là Bài kiểm tra, trong quá trình kiểm tra sẽ bắt hết các trình sử dụng AI" +
                            "   - Không sử dụng bạn nhé!!!!!!!!!11",

                            MessageDialogStyle.AffirmativeAndNegative
                        );

                if (result == MessageDialogResult.Affirmative)
                {
                    _currentAssignmentId = assignmentId;
                    return await StartAIDetector();
                }
            }
            catch (Exception ex)
            {
                await _viewModel.GetMetroWindow()?.ShowMessageAsync(
                            "Lỗi",
                            "Lỗi trong quá trình xác nhận" + ex.ToString()
                        );
            }

            return false;
        }

        public Task<bool> StartAIDetector()
        {
            try
            {
                if (!_proxyService.IsRunning)
                {
                    // Start proxy with AI detection mode
                    bool started = _proxyService.Start(
                        port: 8888,
                        filterAi: true,
                        mode: ProxyMode.Detect,
                        autoInstallCert: true
                    );

                    if (started)
                    {
                        Debug.WriteLine("AI Detector (Proxy) started successfully");

                        // Start stats reporting timer (every 5 seconds)
                        StartStatsReporting();

                        return Task.FromResult(true);
                    }
                    else
                    {
                        Debug.WriteLine("Failed to start AI Detector (Proxy)");
                    }
                }
                else
                {
                    // Already running
                    return Task.FromResult(true);
                }
            }
            catch (Exception ex)
            {
                _viewModel.GetMetroWindow()?.ShowMessageAsync(
                    "Thông báo",
                    "Lỗi khởi động AIdetector: " + ex.ToString()
                );
            }
            return Task.FromResult(false);
        }

        public void StopAIDetector()
        {
            try
            {
                // Stop the auto monitor timer
                if (_checkTimer != null)
                {
                    _checkTimer.Stop();
                    _checkTimer.Dispose();
                    _checkTimer = null;
                }

                // Stop the stats reporting timer
                StopStatsReporting();

                // Stop the proxy service
                _proxyService?.Stop();

                // Clear assignment id
                _currentAssignmentId = null;

                Debug.WriteLine("AI Detector (Proxy) stopped");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping AI Detector: {ex.Message}");
            }
        }

        #region Stats Reporting

        /// <summary>
        /// Start the timer to send stats to server every 5 seconds
        /// </summary>
        private void StartStatsReporting()
        {
            if (_statsTimer != null)
            {
                _statsTimer.Stop();
                _statsTimer.Dispose();
            }

            _statsTimer = new System.Timers.Timer(5000); // 5 seconds
            _statsTimer.Elapsed += async (s, e) => await SendStatsToServerAsync();
            _statsTimer.AutoReset = true;
            _statsTimer.Start();

            Debug.WriteLine("Stats reporting started (every 5 seconds)");
        }

        /// <summary>
        /// Stop the stats reporting timer
        /// </summary>
        private void StopStatsReporting()
        {
            if (_statsTimer != null)
            {
                _statsTimer.Stop();
                _statsTimer.Dispose();
                _statsTimer = null;
                Debug.WriteLine("Stats reporting stopped");
            }
        }

        /// <summary>
        /// Send AI detection stats to server via AssignmentService
        /// </summary>
        private async Task SendStatsToServerAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentAssignmentId))
                {
                    Debug.WriteLine("No assignment ID available, skipping stats send");
                    return;
                }

                // Get stats and clear (so we don't send the same stats twice)
                var stats = _proxyService?.GetStats();

                if (stats == null || stats.Count == 0)
                {
                    Debug.WriteLine("No stats to send");
                    return;
                }

                Debug.WriteLine($"Sending stats to server: {string.Join(", ", stats)}");

                // Send to server via AssignmentService
                var response = await _assignmentService.IncrementAIDetectionAsync(_currentAssignmentId, stats);

                if (response?.Success == true)
                {
                    _proxyService.ClearStats();
                    Debug.WriteLine("Stats sent successfully");
                }
                else
                {
                    Debug.WriteLine($"Failed to send stats: {response?.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending stats to server: {ex.Message}");
            }
        }

        #endregion

        private void OnAiDetected(string service, string hostname)
        {
            Debug.WriteLine($"AI Detected: {service} - {hostname}");
        }

        private void OnProxyReEnabled(int count)
        {
            Debug.WriteLine($"Proxy re-enabled by watchdog. Count: {count}");
        }

        public void StartAutoMonitor()
        {
            _checkTimer = new System.Timers.Timer(1 * 60 * 1000); // 1 phút
            _checkTimer.Elapsed += (s, e) => CheckAndRestartAIDetector();
            _checkTimer.AutoReset = true;
            _checkTimer.Start();
        }

        private void CheckAndRestartAIDetector()
        {
            try
            {
                if (!_proxyService.IsRunning)
                {
                    StartAIDetector().Wait();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in auto monitor: {ex.Message}");
            }
        }

        public void Dispose()
        {
            StopAIDetector();

            if (_proxyService != null)
            {
                _proxyService.OnAiDetected -= OnAiDetected;
                _proxyService.OnProxyReEnabled -= OnProxyReEnabled;
                _proxyService.Dispose();
                _proxyService = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}