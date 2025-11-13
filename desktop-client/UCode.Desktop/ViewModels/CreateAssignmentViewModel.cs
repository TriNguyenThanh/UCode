using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class CreateAssignmentViewModel : ViewModelBase
    {
        private readonly AssignmentService _assignmentService;
        private readonly ClassService _classService;
        private readonly NavigationService _navigationService;
        private bool _isLoading;
        private bool _isSaving;
        private string _error = string.Empty;
        private Class _classData;

        // Form fields
        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _assignmentType = "HOMEWORK";
        private DateTime _startTime = DateTime.Now;
        private DateTime _endTime = DateTime.Now.AddDays(7);
        private bool _noEndDate = false;
        private bool _allowLateSubmission = true;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public Class ClassData
        {
            get => _classData;
            set => SetProperty(ref _classData, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string AssignmentType
        {
            get => _assignmentType;
            set => SetProperty(ref _assignmentType, value);
        }

        public DateTime StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public DateTime EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        public bool NoEndDate
        {
            get => _noEndDate;
            set
            {
                if (SetProperty(ref _noEndDate, value))
                {
                    OnPropertyChanged(nameof(IsEndDateEnabled));
                }
            }
        }

        public bool IsEndDateEnabled => !NoEndDate;

        public bool AllowLateSubmission
        {
            get => _allowLateSubmission;
            set => SetProperty(ref _allowLateSubmission, value);
        }

        public ObservableCollection<string> AssignmentTypes { get; } = new()
        {
            "HOMEWORK",
            "EXAM",
            "PRACTICE"
        };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public CreateAssignmentViewModel(
            AssignmentService assignmentService,
            ClassService classService,
            NavigationService navigationService)
        {
            _assignmentService = assignmentService;
            _classService = classService;
            _navigationService = navigationService;

            SaveCommand = new RelayCommand(async _ => await SaveAssignmentAsync());
            CancelCommand = new RelayCommand(_ => ExecuteCancel());
        }

        public async Task InitializeAsync(string classId)
        {
            IsLoading = true;
            Error = string.Empty;

            try
            {
                var response = await _classService.GetClassByIdAsync(classId);
                if (response?.Success == true && response.Data != null)
                {
                    ClassData = response.Data;
                }
                else
                {
                    Error = "Không thể tải thông tin lớp học";
                }
            }
            catch (Exception ex)
            {
                Error = $"Lỗi: {ex.Message}";
                MessageBox.Show(Error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAssignmentAsync()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(Title))
            {
                MessageBox.Show("Vui lòng nhập tên bài tập", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!NoEndDate && EndTime <= StartTime)
            {
                MessageBox.Show("Thời gian kết thúc phải sau thời gian bắt đầu", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsSaving = true;
            Error = string.Empty;

            try
            {
                var request = new Models.CreateAssignmentRequest
                {
                    ClassId = ClassData.ClassId,
                    Title = Title.Trim(),
                    Description = Description?.Trim() ?? string.Empty,
                    AssignmentType = AssignmentType,
                    StartTime = StartTime.ToString("o"),
                    EndTime = NoEndDate ? string.Empty : EndTime.ToString("o"),
                    AllowLateSubmission = AllowLateSubmission,
                    Status = "DRAFT",
                    Problems = new System.Collections.Generic.List<Models.AssignmentProblem>()
                };

                var response = await _assignmentService.CreateAssignmentAsync(request);
                
                if (response?.Success == true && response.Data != null)
                {
                    MessageBox.Show(
                        "Tạo bài tập thành công!\n\nBạn sẽ được chuyển đến trang quản lý bài tập để thêm các bài toán.",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Close the window first
                    ExecuteCancel();

                    // Navigate to assignment detail page
                    var assignmentPage = App.ServiceProvider.GetService(typeof(Pages.TeacherAssignmentPage)) as Pages.TeacherAssignmentPage;
                    if (assignmentPage != null)
                    {
                        _navigationService.NavigateTo(assignmentPage, response.Data.AssignmentId);
                    }
                }
                else
                {
                    MessageBox.Show(
                        $"Tạo bài tập thất bại: {response?.Message}",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Error = $"Lỗi: {ex.Message}";
                MessageBox.Show(Error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void ExecuteCancel()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

        public string GetAssignmentTypeDisplay(string type)
        {
            return type switch
            {
                "HOMEWORK" => "Bài tập về nhà",
                "EXAM" => "Kiểm tra",
                "PRACTICE" => "Luyện tập",
                _ => type
            };
        }
    }
}

