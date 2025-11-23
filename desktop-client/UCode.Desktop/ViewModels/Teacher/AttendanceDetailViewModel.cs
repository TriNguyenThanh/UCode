using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class AttendanceDetailViewModel : ViewModelBase
    {
        private readonly AttendanceService _attendanceService;
        private readonly ClassService _classService;
        private readonly NavigationService _navigationService;
        private string _sessionId = string.Empty;
        private bool _isLoading;
        private AttendanceSessionItem? _session;
        private string _searchText = string.Empty;
        private Timer? _searchDebounceTimer;
        private string _selectedStatus = "Tất cả";

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public AttendanceSessionItem? Session
        {
            get => _session;
            set
            {
                if (SetProperty(ref _session, value))
                {
                    OnPropertyChanged(nameof(AttendanceRate));
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // Debounce search - wait 300ms after user stops typing
                    _searchDebounceTimer?.Dispose();
                    _searchDebounceTimer = new Timer(_ =>
                    {
                        Application.Current.Dispatcher.Invoke(() => FilterRecords());
                    }, null, 300, Timeout.Infinite);
                }
            }
        }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    FilterRecords();
                }
            }
        }

        public double AttendanceRate
        {
            get
            {
                if (TotalStudents == 0) return 0;
                return (double)AttendedCount / TotalStudents * 100;
            }
        }

        public int TotalStudents { get; private set; }

        // Số người đã điểm danh (hợp lệ + không hợp lệ)
        public int AttendedCount => Records.Count(r => r.AttendedAt.HasValue);
        public int ValidCount => Records.Count(r => r.AttendedAt.HasValue && r.IsValid);
        public int InvalidCount => Records.Count(r => r.AttendedAt.HasValue && !r.IsValid);
        public int NotAttendedCount => Records.Count(r => !r.AttendedAt.HasValue);

        public ObservableCollection<AttendanceRecord> Records { get; } = new();
        public ObservableCollection<AttendanceRecord> FilteredRecords { get; } = new();
        public ObservableCollection<string> StatusFilters { get; } = new()
        {
            "Tất cả",
            "Đã điểm danh hợp lệ",
            "Đã điểm danh không hợp lệ",
            "Chưa điểm danh"
        };

        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ShowQRCommand { get; }
        public ICommand EditConfigCommand { get; }

        public AttendanceDetailViewModel(
            AttendanceService attendanceService,
            ClassService classService,
            NavigationService navigationService)
        {
            _attendanceService = attendanceService;
            _classService = classService;
            _navigationService = navigationService;

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            ExportCommand = new RelayCommand(async _ => await ExportToExcelAsync());
            BackCommand = new RelayCommand(_ => _navigationService.GoBack());
            ShowQRCommand = new RelayCommand(_ => ShowQRCode());
            EditConfigCommand = new RelayCommand(_ => EditConfig());
        }

        private void ShowQRCode()
        {
            if (Session == null) return;

            var viewModel = new AttendanceQRViewModel(Session.SessionCode, Session.Title);
            var dialog = new Views.AttendanceQRDialog(viewModel);
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        private async void EditConfig()
        {
            if (Session == null) return;

            var viewModel = new AttendanceConfigViewModel(_attendanceService, _sessionId, Session);
            var dialog = new Views.AttendanceConfigDialog(viewModel);
            dialog.Owner = System.Windows.Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                // Reload data after config update
                await LoadDataAsync();
            }
        }

        public async Task InitializeAsync(string sessionId)
        {
            _sessionId = sessionId;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                // Load session info
                var res = await _attendanceService.GetAttendanceSessionByIdAsync(_sessionId);

                if (res?.Success == true && res.Data != null)
                {
                    Session = res.Data;
                }
                else
                {
                    throw new Exception("Không tìm thấy phiên điểm danh.");
                }

                // Load all students in the class
                var classStudentsResponse = await _classService.GetClassStudentsAsync(Session.ClassId);
                
                // Load attendance records
                var attendanceResponse = await _attendanceService.GetAttendanceRecordsAsync(_sessionId);
                
                Records.Clear();
                FilteredRecords.Clear();

                // Create a dictionary of attendance records by userId for quick lookup
                var attendanceDict = new Dictionary<string, AttendanceRecord>();
                if (attendanceResponse?.Success == true && attendanceResponse.Data != null)
                {
                    foreach (var record in attendanceResponse.Data)
                    {
                        attendanceDict[record.UserId] = record;
                    }
                }

                // Merge all students with attendance records
                if (classStudentsResponse?.Success == true && classStudentsResponse.Data != null)
                {
                    TotalStudents = classStudentsResponse.Data.Count;
                    
                    // Sort students by StudentCode (MSSV) in ascending order
                    var sortedStudents = classStudentsResponse.Data.OrderBy(s => s.StudentCode).ToList();
                    // int cnt = 0;
                    foreach (var student in sortedStudents)
                    {
                        AttendanceRecord record;
                        
                        if (attendanceDict.TryGetValue(student.UserId, out var attendanceRecord))
                        {
                            // Student has attended - merge class info with attendance data
                            record = new AttendanceRecord
                            {
                                RecordId = attendanceRecord.RecordId,
                                SessionId = attendanceRecord.SessionId,
                                UserId = student.UserId,
                                StudentCode = student.StudentCode ?? "N/A",
                                FullName = student.FullName ?? "N/A",
                                AttendedAt = attendanceRecord.AttendedAt,
                                IpAddress = attendanceRecord.IpAddress,
                                Latitude = attendanceRecord.Latitude,
                                Longitude = attendanceRecord.Longitude,
                                IsValid = attendanceRecord.IsValid,
                                ValidationMessage = attendanceRecord.ValidationMessage
                            };
                        }
                        else
                        {
                            // Student hasn't attended - create a placeholder record
                            record = new AttendanceRecord
                            {
                                RecordId = string.Empty,
                                SessionId = _sessionId,
                                UserId = student.UserId,
                                StudentCode = student.StudentCode ?? "N/A",
                                FullName = student.FullName ?? "N/A",
                                AttendedAt = null,
                                IsValid = false,
                                ValidationMessage = "Chưa điểm danh"
                            };
                        }
                        
                        Records.Add(record);
                        FilteredRecords.Add(record);
                    }
                }

                OnPropertyChanged(nameof(TotalStudents));
                OnPropertyChanged(nameof(AttendedCount));
                OnPropertyChanged(nameof(AttendanceRate));
                OnPropertyChanged(nameof(ValidCount));
                OnPropertyChanged(nameof(InvalidCount));
                OnPropertyChanged(nameof(NotAttendedCount));
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Lỗi",
                    $"Không thể tải dữ liệu: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }



        private void FilterRecords()
        {
            FilteredRecords.Clear();

            var query = Records.AsEnumerable();

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(r =>
                    r.StudentCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by status
            query = SelectedStatus switch
            {
                "Đã điểm danh hợp lệ" => query.Where(r => r.AttendedAt.HasValue && r.IsValid),
                "Đã điểm danh không hợp lệ" => query.Where(r => r.AttendedAt.HasValue && !r.IsValid),
                "Chưa điểm danh" => query.Where(r => !r.AttendedAt.HasValue),
                _ => query // "Tất cả"
            };

            foreach (var record in query)
            {
                FilteredRecords.Add(record);
            }
        }

        private async Task ExportToExcelAsync()
        {
            if (Session == null || Records.Count == 0)
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Thông báo",
                    "Không có dữ liệu để xuất");
                return;
            }

            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Xuất danh sách điểm danh",
                    FileName = $"DiemDanh_{Session.SessionCode}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Danh sách điểm danh");

                        // Title
                        worksheet.Cell(1, 1).Value = "DANH SÁCH ĐIỂM DANH";
                        worksheet.Range(1, 1, 1, 8).Merge();
                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                        worksheet.Cell(1, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                        // Session Info
                        worksheet.Cell(2, 1).Value = $"Phiên: {Session.Title}";
                        worksheet.Range(2, 1, 2, 4).Merge();
                        worksheet.Cell(2, 5).Value = $"Mã: {Session.SessionCode}";
                        worksheet.Range(2, 5, 2, 8).Merge();

                        worksheet.Cell(3, 1).Value = $"Thời gian: {Session.StartTime:dd/MM/yyyy HH:mm} - {Session.EndTime:dd/MM/yyyy HH:mm}";
                        worksheet.Range(3, 1, 3, 8).Merge();

                        worksheet.Cell(4, 1).Value = $"Tổng SV: {TotalStudents} | Đã điểm danh: {AttendedCount} | Hợp lệ: {ValidCount} | Không hợp lệ: {InvalidCount} | Chưa điểm danh: {NotAttendedCount}";
                        worksheet.Range(4, 1, 4, 8).Merge();

                        // Headers
                        int headerRow = 6;
                        worksheet.Cell(headerRow, 1).Value = "STT";
                        worksheet.Cell(headerRow, 2).Value = "MSSV";
                        worksheet.Cell(headerRow, 3).Value = "Họ và tên";
                        worksheet.Cell(headerRow, 4).Value = "Thời gian điểm danh";
                        worksheet.Cell(headerRow, 5).Value = "IP Address";
                        worksheet.Cell(headerRow, 6).Value = "GPS (Latitude)";
                        worksheet.Cell(headerRow, 7).Value = "GPS (Longitude)";
                        worksheet.Cell(headerRow, 8).Value = "Trạng thái";
                        worksheet.Cell(headerRow, 9).Value = "Ghi chú";

                        // Style header
                        var headerRange = worksheet.Range(headerRow, 1, headerRow, 9);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#191970");
                        headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                        headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                        headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                        // Data rows
                        int currentRow = headerRow + 1;
                        int stt = 1;
                        foreach (var record in Records.OrderBy(r => r.AttendedAt))
                        {
                            worksheet.Cell(currentRow, 1).Value = stt++;
                            worksheet.Cell(currentRow, 2).Value = record.StudentCode;
                            worksheet.Cell(currentRow, 3).Value = record.FullName;
                            worksheet.Cell(currentRow, 4).Value = record.AttendedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";
                            worksheet.Cell(currentRow, 5).Value = record.IpAddress ?? "";
                            worksheet.Cell(currentRow, 6).Value = record.Latitude?.ToString("F6") ?? "";
                            worksheet.Cell(currentRow, 7).Value = record.Longitude?.ToString("F6") ?? "";
                            worksheet.Cell(currentRow, 8).Value = record.AttendedAt.HasValue 
                                ? (record.IsValid ? "Hợp lệ" : "Không hợp lệ")
                                : "Chưa điểm danh";
                            worksheet.Cell(currentRow, 9).Value = record.ValidationMessage ?? "";

                            // Color code status
                            var statusCell = worksheet.Cell(currentRow, 8);
                            if (!record.AttendedAt.HasValue)
                            {
                                // Chưa điểm danh - Purple
                                statusCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#9C27B0");
                                statusCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                            }
                            else if (record.IsValid)
                            {
                                // Hợp lệ - Green
                                statusCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#34C759");
                                statusCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                            }
                            else
                            {
                                // Không hợp lệ - Red
                                statusCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#FF3B30");
                                statusCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                            }
                            statusCell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                            currentRow++;
                        }

                        // Auto-fit columns
                        worksheet.Columns().AdjustToContents();

                        // Add borders to data
                        var dataRange = worksheet.Range(headerRow, 1, currentRow - 1, 9);
                        dataRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    await GetMetroWindow()?.ShowMessageAsync(
                        "Thành công",
                        $"Đã xuất {Records.Count} bản ghi điểm danh ra file Excel!");
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Lỗi",
                    $"Không thể xuất Excel: {ex.Message}");
            }
        }
    }
}
