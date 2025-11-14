using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
        public int ClassYear { get; set; }
        public string Status { get; set; } = "new"; // new, exists, error
        public string? ExistingUserId { get; set; }
        public string? ErrorMessage { get; set; }

        public string StatusText => Status switch
        {
            "new" => "Sẽ tạo mới",
            "exists" => "Đã tồn tại",
            "error" => ErrorMessage ?? "Lỗi",
            _ => Status
        };

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class ImportExcelTabViewModel : ViewModelBase
    {
        private readonly ClassService _classService;

        private string _classId = string.Empty;
        private int _activeStep = 0;
        private bool _isLoading;
        private string? _errorMessage;

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

        public string? ErrorMessage
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

            DownloadTemplateCommand = new RelayCommand(_ => DownloadTemplate());
            UploadFileCommand = new RelayCommand(_ => UploadFile());
            ResetCommand = new RelayCommand(_ => Reset());
            ImportCommand = new RelayCommand(async _ => await ImportAsync());
        }

        public void Initialize(string classId)
        {
            _classId = classId;
        }

        private void DownloadTemplate()
        {
            // TODO: Implement Excel template download using ClosedXML
            MessageBox.Show(
                "Excel Template Download\n\n" +
                "TODO: Implement using ClosedXML NuGet package\n\n" +
                "Template columns:\n" +
                "- StudentCode\n" +
                "- FullName\n" +
                "- Email\n" +
                "- Password\n" +
                "- Major\n" +
                "- EnrollmentYear\n\n" +
                "See: client/app/components/AddStudentDialog/ImportExcelTab.tsx (handleDownloadTemplate)\n" +
                "Install: <PackageReference Include=\"ClosedXML\" Version=\"0.102.1\" />",
                "Feature Not Implemented",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
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
            ErrorMessage = null;

            try
            {
                // TODO: Implement Excel parsing using ClosedXML
                // Read file, parse rows, validate, populate ValidationResults

                MessageBox.Show(
                    $"Excel Processing\n\n" +
                    $"File: {filePath}\n\n" +
                    "TODO: Parse Excel file using ClosedXML\n" +
                    "1. Read rows from Excel\n" +
                    "2. Validate each row\n" +
                    "3. Call API to check if students exist\n" +
                    "4. Populate ValidationResults\n\n" +
                    "See: client/app/components/AddStudentDialog/ImportExcelTab.tsx\n" +
                    "(handleFileUpload + handleValidate)",
                    "Feature Not Implemented",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Mock: Move to step 1, then step 2
                ActiveStep = 1;
                await Task.Delay(1500); // Simulate validation
                ActiveStep = 2;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi xử lý file: {ex.Message}";
                ActiveStep = 0;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Reset()
        {
            ActiveStep = 0;
            ValidationResults.Clear();
            ErrorMessage = null;
        }

        private async Task ImportAsync()
        {
            // TODO: Implement bulk create + bulk enroll
            MessageBox.Show(
                "Import Students Implementation\n\n" +
                "TODO:\n" +
                "1. Call StudentService.BulkCreateStudentsAsync for new students\n" +
                "2. Call ClassService.BulkEnrollStudentsAsync for all students\n\n" +
                "See: client/app/components/AddStudentDialog/ImportExcelTab.tsx (handleImport)\n" +
                "and client/app/services/studentService.ts (bulkCreateStudents)\n" +
                "and client/app/services/classService.ts (bulkEnrollStudents)",
                "Feature Not Implemented",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            await Task.CompletedTask;
        }
    }
}

