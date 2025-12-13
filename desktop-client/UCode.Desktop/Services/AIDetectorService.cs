using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using UCode.Desktop.ViewModels;
using System.Linq;

namespace UCode.Desktop.Services
{
    public class AIDetectorService
    {
        public TokenStorageService _tokenStorage;
        private ViewModelBase _viewModel;
        private Process _aiProcess;
        private FileSystemWatcher _flagWatcher;
        private string _flagPath;
        private string _aiExeName = @"ai_detector_Ucode.exe"; // tên dẫn tới file exe
        private string _aiExePath;
        private System.Timers.Timer _checkTimer;
        public AIDetectorService(TokenStorageService storageService)
        {
            _tokenStorage = storageService;
            _viewModel = new ViewModelBase();
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _flagPath = Path.Combine(appData, "UCode", "proxy_enabled.flag");
            _aiExePath = Path.Combine(appData, "UCode", _aiExeName);
        }

        public async Task<bool> ConfirmMessageAIDetector(string assignmentId)
        {
            var running = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_aiExeName)).Any();
            if (running)
                return true;

            try
            {
                var result = await _viewModel.GetMetroWindow()?.ShowMessageAsync(
                            "Lưu ý quan trọng:",
                            "   - Đây là Bài kiểm tra, trong quá trình kiểm tra sẽ bắt hết các trình sử dụng AI" + 
                            "   - Không sử dụng bạn nhé!!!!!!!!!11"  ,
                             
                            MessageDialogStyle.AffirmativeAndNegative
                        );

                if (result == MessageDialogResult.Affirmative)
                {
                    _tokenStorage.SaveFile(assignmentId, "assignment_id.dat");
                    
                    //StartFlagWatcher();
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

        public async Task<bool> StartAIDetector()
        {
            try
            {
                if (_aiProcess == null || _aiProcess.HasExited)
                {
                    _aiProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = _aiExePath,
                            Arguments = "api",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    _aiProcess.Start();
                    return true;
                }
            }
            catch (Exception ex)
            {
                await _viewModel.GetMetroWindow()?.ShowMessageAsync(
                    "Thông báo",
                    "Lỗi khởi động AIdetector: " + ex.ToString()
                );
            }
            return false;
        }

        public void StopAIDetector()
        {
            try
            {
                // Stop the timer first
                if (_checkTimer != null)
                {
                    _checkTimer.Stop();
                    _checkTimer.Dispose();
                    _checkTimer = null;
                }

                // Stop file watcher
                if (_flagWatcher != null)
                {
                    _flagWatcher.EnableRaisingEvents = false;
                    _flagWatcher.Dispose();
                    _flagWatcher = null;
                }

                // Send stop command via API
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(2);
                        var task = client.PostAsync("http://localhost:1701/stop", null);
                        task.Wait(2000); // Wait max 2 seconds
                    }
                }
                catch { /* Ignore API errors */ }

                // Stop the process
                if (_aiProcess != null && !_aiProcess.HasExited)
                {
                    try
                    {
                        // Try graceful shutdown first
                        _aiProcess.CloseMainWindow();
                        
                        // Wait up to 3 seconds for graceful exit
                        if (!_aiProcess.WaitForExit(3000))
                        {
                            // Force kill if still running
                            _aiProcess.Kill();
                            _aiProcess.WaitForExit(1000);
                        }
                    }
                    catch { /* Process might already be terminated */ }
                    finally
                    {
                        _aiProcess?.Dispose();
                        _aiProcess = null;
                    }
                }

                // Kill any remaining processes by name (in case of orphaned processes)
                try
                {
                    var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_aiExeName));
                    foreach (var proc in processes)
                    {
                        try
                        {
                            if (!proc.HasExited)
                            {
                                proc.Kill();
                                proc.WaitForExit(1000);
                            }
                            proc.Dispose();
                        }
                        catch { /* Ignore individual process errors */ }
                    }
                }
                catch { /* Ignore process enumeration errors */ }
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                Debug.WriteLine($"Error stopping AI Detector: {ex.Message}");
            }
        }

        private void StartFlagWatcher()
        {
            var dir = Path.GetDirectoryName(_flagPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _flagWatcher = new FileSystemWatcher(dir, "proxy_enabled.flag");
            _flagWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            _flagWatcher.Created += OnFlagChanged;
            _flagWatcher.Changed += OnFlagChanged;
            _flagWatcher.EnableRaisingEvents = true;
        }

        private async void OnFlagChanged(object sender, FileSystemEventArgs e)
        {
            // Đảm bảo chỉ hiện 1 lần/thông báo
            _flagWatcher.EnableRaisingEvents = false;
            await _viewModel.GetMetroWindow()?.ShowMessageAsync(
                "Thông báo",
                "Proxy vừa được tự động bật lại! Nếu bạn không thực hiện thao tác này, vui lòng kiểm tra lại thiết bị."
            );
            try { File.Delete(_flagPath); } catch { }
            _flagWatcher.EnableRaisingEvents = true;
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
                var running = Process.GetProcessesByName(
                    Path.GetFileNameWithoutExtension(_aiExeName)
                ).Any();

                if (!running)
                {
                    // Không dùng await trong Timer thread, nên gọi sync
                    StartAIDetector().Wait();
                }
            }
            catch { }
        }
    }
}