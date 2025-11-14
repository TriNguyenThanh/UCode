using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClosedXML.Excel;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using UCode.Desktop.Helpers;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class ValidationResultViewModel : INotifyPropertyChanged
    {
        public int RowNumber { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public int EnrollmentYear { get; set; }
        public string Status { get; set; } = "new";
        public string ExistingUserId { get; set; }
        public string ErrorMessage { get; set; }

        public string StatusText => Status switch
        {
            "new" => "Sẽ tạo mới",
            "exists" => "Đã tồn tại",
            "error" => ErrorMessage ?? "Lỗi",
            _ => Status
        };

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ImportExcelTabViewModel : ViewModelBase
    {
        private readonly ClassService _classService;

        private string _classId = string.Empty;
        private int _activeStep = 0;
        private bool _isLoading;
        private string _errorMessage;

        public ObservableCollection<ValidationResultViewModel> ValidationResults { get; } = new();

        public int ActiveStep
        {
            get => _activeStep;
            set
            {
                if (SetProperty(ref _activeStep, value))
                {
                    OnPropertyChanged(nameof(IsStep0));
                    OnPropertyChanged(nameof(IsStep1));
                    OnPropertyChanged(nameof(IsStep2));
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsStep0 => ActiveStep == 0;
        public bool IsStep1 => ActiveStep == 1;
        public bool IsStep2 => ActiveStep == 2;

        public int TotalStudents => ValidationResults.Count;
        public int NewCount => ValidationResults.Count(v => v.Status == "new");
        public int ExistsCount => ValidationResults.Count(v => v.Status == "exists");
        public int ErrorCount => ValidationResults.Count(v => v.Status == "error");
        
        public bool HasNewStudents => NewCount > 0;
        public bool HasExistingStudents => ExistsCount > 0;
        public bool HasErrors => ErrorCount > 0;
        public bool CanImport => (NewCount > 0 || ExistsCount > 0) && !IsLoading;
        public string ImportButtonText => IsLoading ? "Đang import..." : $"Import {NewCount + ExistsCount} sinh viên";

        public ICommand DownloadTemplateCommand { get; }
        public ICommand UploadFileCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ImportCommand { get; }

        public ImportExcelTabViewModel(ClassService classService)
        {
            _classService = classService;

            DownloadTemplateCommand = new RelayCommand(async _ => await DownloadTemplateAsync());
            UploadFileCommand = new RelayCommand(_ => UploadFile());
            ResetCommand = new RelayCommand(_ => Reset());
            ImportCommand = new RelayCommand(async _ => await ImportAsync(), _ => CanImport);
        }

        public void Initialize(string classId)
        {
            _classId = classId;
        }

        private async Task DownloadTemplateAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Lưu file mẫu",
                    FileName = "student_template.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Students");

                    // Headers - Match web version (removed Password and EnrollmentYear)
                    worksheet.Cell(1, 1).Value = "StudentCode";
                    worksheet.Cell(1, 2).Value = "FullName";
                    worksheet.Cell(1, 3).Value = "Email";
                    worksheet.Cell(1, 4).Value = "Major";

                    // Style headers
                    var headerRange = worksheet.Range(1, 1, 1, 4);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                    // Sample data
                    worksheet.Cell(2, 1).Value = "SV001";
                    worksheet.Cell(2, 2).Value = "Nguyễn Văn A";
                    worksheet.Cell(2, 3).Value = "sv001@example.com";
                    worksheet.Cell(2, 4).Value = "Công nghệ phần mềm";

                    worksheet.Cell(3, 1).Value = "SV002";
                    worksheet.Cell(3, 2).Value = "Trần Thị B";
                    worksheet.Cell(3, 3).Value = "sv002@example.com";
                    worksheet.Cell(3, 4).Value = "Khoa học máy tính";

                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(saveFileDialog.FileName);
                    await GetMetroWindow()?.ShowMessageAsync("Thành công", "Đã tải file mẫu thành công!");
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Lỗi tải file mẫu: {ex.Message}");
            }
        }

        private void UploadFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls|All Files|*.*",
                Title = "Chọn file Excel"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _ = ProcessExcelFileAsync(openFileDialog.FileName);
            }
        }

        private async Task ProcessExcelFileAsync(string filePath)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            ValidationResults.Clear();

            try
            {
                ActiveStep = 1; // Move to validation step

                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(1);
                
                var rows = worksheet.RowsUsed().Skip(1); // Skip header

                var studentData = new List<ValidationResultViewModel>();
                int rowNumber = 2;

                foreach (var row in rows)
                {
                    var result = new ValidationResultViewModel
                    {
                        RowNumber = rowNumber,
                        StudentCode = row.Cell(1).GetString().Trim(),
                        FullName = row.Cell(2).GetString().Trim(),
                        Email = row.Cell(3).GetString().Trim(),
                        Major = row.Cell(4).GetString().Trim(),
                        EnrollmentYear = DateTime.Now.Year // Default to current year
                    };

                    // Basic validation
                    if (string.IsNullOrWhiteSpace(result.StudentCode))
                    {
                        result.Status = "error";
                        result.ErrorMessage = "Thiếu mã sinh viên";
                    }
                    else if (string.IsNullOrWhiteSpace(result.FullName))
                    {
                        result.Status = "error";
                        result.ErrorMessage = "Thiếu họ tên";
                    }
                    else if (string.IsNullOrWhiteSpace(result.Email))
                    {
                        result.Status = "error";
                        result.ErrorMessage = "Thiếu email";
                    }
                    else if (!result.Email.Contains("@"))
                    {
                        result.Status = "error";
                        result.ErrorMessage = "Email không hợp lệ";
                    }
                    else
                    {
                        result.Status = "new"; // Will check with API
                    }

                    studentData.Add(result);
                    rowNumber++;
                }

                // Check which students already exist
                await ValidateStudentsAsync(studentData);

                foreach (var result in studentData)
                {
                    ValidationResults.Add(result);
                }

                ActiveStep = 2; // Move to review step

                OnPropertyChanged(nameof(TotalStudents));
                OnPropertyChanged(nameof(NewCount));
                OnPropertyChanged(nameof(ExistsCount));
                OnPropertyChanged(nameof(ErrorCount));
                OnPropertyChanged(nameof(HasNewStudents));
                OnPropertyChanged(nameof(HasExistingStudents));
                OnPropertyChanged(nameof(HasErrors));
                OnPropertyChanged(nameof(CanImport));
                OnPropertyChanged(nameof(ImportButtonText));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi xử lý file: {ex.Message}";
                ActiveStep = 0;
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", ErrorMessage);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ValidateStudentsAsync(List<ValidationResultViewModel> students)
        {
            try
            {
                // Use bulk validation API like web version
                var studentCodes = students.Where(s => s.Status != "error").Select(s => s.StudentCode).ToList();
                
                var response = await _classService.ValidateStudentsBulkAsync(studentCodes);
                
                if (response?.Success == true && response.Data != null)
                {
                    // Create lookup dictionary
                    var validationMap = response.Data.ToDictionary(v => v.StudentCode, v => v);

                    foreach (var student in students)
                    {
                        if (student.Status == "error") continue;

                        if (validationMap.TryGetValue(student.StudentCode, out var validation))
                        {
                            if (validation.Exists)
                            {
                                student.Status = "exists";
                                student.ExistingUserId = validation.UserId;
                            }
                            else
                            {
                                student.Status = "new";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validating students: {ex.Message}");
            }
        }

        private void Reset()
        {
            ActiveStep = 0;
            ValidationResults.Clear();
            ErrorMessage = string.Empty;
            
            OnPropertyChanged(nameof(TotalStudents));
            OnPropertyChanged(nameof(NewCount));
            OnPropertyChanged(nameof(ExistsCount));
            OnPropertyChanged(nameof(ErrorCount));
        }

        private async Task ImportAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // Separate new and existing students
                var newStudents = ValidationResults.Where(v => v.Status == "new").ToList();
                var existingStudents = ValidationResults.Where(v => v.Status == "exists").ToList();

                var createdUserIds = new List<string>();

                // Step 1: Create new students using bulk create API
                if (newStudents.Count > 0)
                {
                    var createRequests = newStudents.Select(s => new CreateStudentRequest
                    {
                        StudentCode = s.StudentCode,
                        FullName = s.FullName,
                        Email = s.Email,
                        Major = s.Major,
                        ClassYear = s.EnrollmentYear
                    }).ToList();

                    var createResponse = await _classService.BulkCreateStudentsAsync(createRequests);
                    
                    if (createResponse?.Success == true && createResponse.Data != null)
                    {
                        // Collect successfully created user IDs
                        createdUserIds = createResponse.Data.Results
                            .Where(r => r.Success)
                            .Select(r => r.UserId)
                            .ToList();

                        if (createResponse.Data.FailureCount > 0)
                        {
                            var failedStudents = createResponse.Data.Results.Where(r => !r.Success).ToList();
                            ErrorMessage = $"Không thể tạo {createResponse.Data.FailureCount} sinh viên:\n" +
                                          string.Join("\n", failedStudents.Select(f => $"- {f.StudentCode}: {f.ErrorMessage}"));
                        }
                    }
                    else
                    {
                        ErrorMessage = $"Lỗi tạo sinh viên: {createResponse?.Message}";
                        return;
                    }
                }

                // Step 2: Enroll all students (newly created + existing) into class
                var allUserIds = createdUserIds.Concat(existingStudents.Select(s => s.ExistingUserId).Where(id => !string.IsNullOrEmpty(id))).ToList();
                
                if (allUserIds.Count > 0)
                {
                    var enrollResponse = await _classService.BulkEnrollStudentsAsync(_classId, allUserIds);
                    
                    if (enrollResponse?.Success == true && enrollResponse.Data != null)
                    {
                        var successMsg = $"Đã import thành công!\n" +
                                       $"- Tạo mới: {createdUserIds.Count} sinh viên\n" +
                                       $"- Đã tồn tại: {existingStudents.Count} sinh viên\n" +
                                       $"- Đã thêm vào lớp: {enrollResponse.Data.SuccessCount}/{allUserIds.Count}";
                        
                        if (enrollResponse.Data.FailureCount > 0)
                        {
                            successMsg += $"\n- Lỗi thêm vào lớp: {enrollResponse.Data.FailureCount}";
                        }
                        
                        await GetMetroWindow()?.ShowMessageAsync("Thành công", successMsg);
                        Reset();
                    }
                    else
                    {
                        ErrorMessage = $"Lỗi thêm sinh viên vào lớp: {enrollResponse?.Message}";
                        await GetMetroWindow()?.ShowMessageAsync("Lỗi", ErrorMessage);
                    }
                }
                else
                {
                    await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Không có sinh viên nào để import");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi import: {ex.Message}";
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", ErrorMessage);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
