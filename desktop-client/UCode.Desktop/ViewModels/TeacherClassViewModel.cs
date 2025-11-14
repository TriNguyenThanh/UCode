using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class ClassStudentItem
    {
        public string UserId { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public int? ClassYear { get; set; }
    }

    public class TeacherClassViewModel : ViewModelBase
    {
        private readonly ClassService _classService;
        private readonly AssignmentService _assignmentService;
        private NavigationService? _navigationService;
        private bool _isLoading;
        private string _error = string.Empty;
        private string _classId = string.Empty;
        private Class? _currentClass;
        private string _searchText = string.Empty;
        private int _selectedTabIndex;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public Class? CurrentClass
        {
            get => _currentClass;
            set => SetProperty(ref _currentClass, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterStudents();
                }
            }
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public ObservableCollection<ClassStudentItem> Students { get; } = new();
        public ObservableCollection<ClassStudentItem> FilteredStudents { get; } = new();
        public ObservableCollection<Assignment> Assignments { get; } = new();

        public int TotalStudents => Students.Count;
        public int TotalAssignments => Assignments.Count;
        public int ActiveAssignments => Assignments.Count(a => a.Status == AssignmentStatus.PUBLISHED);

        public ICommand RefreshCommand { get; }
        public ICommand AddStudentCommand { get; }
        public ICommand RemoveStudentCommand { get; }
        public ICommand CreateAssignmentCommand { get; }
        public ICommand ViewAssignmentCommand { get; }
        public ICommand EditClassCommand { get; }

        public TeacherClassViewModel(
            ClassService classService,
            AssignmentService assignmentService)
        {
            _classService = classService;
            _assignmentService = assignmentService;

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            AddStudentCommand = new RelayCommand(_ => ExecuteAddStudent());
            RemoveStudentCommand = new RelayCommand(param => ExecuteRemoveStudent(param as string ?? string.Empty));
            CreateAssignmentCommand = new RelayCommand(_ => ExecuteCreateAssignment());
            ViewAssignmentCommand = new RelayCommand(param => ExecuteViewAssignment(param as string ?? string.Empty));
            EditClassCommand = new RelayCommand(_ => ExecuteEditClass());
        }

        public async Task InitializeAsync(string classId)
        {
            _classId = classId;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            Error = string.Empty;

            try
            {
                await Task.WhenAll(
                    LoadClassInfoAsync(),
                    LoadStudentsAsync(),
                    LoadAssignmentsAsync()
                );

                OnPropertyChanged(nameof(TotalStudents));
                OnPropertyChanged(nameof(TotalAssignments));
                OnPropertyChanged(nameof(ActiveAssignments));
            }
            catch (Exception ex)
            {
                Error = $"Lỗi tải dữ liệu: {ex.Message}";
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadClassInfoAsync()
        {
            try
            {
                var response = await _classService.GetClassByIdAsync(_classId);
                if (response?.Success == true && response.Data != null)
                {
                    CurrentClass = response.Data;
                }
                else
                {
                    Error = "Không thể tải thông tin lớp học";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading class info: {ex.Message}");
            }
        }

        private async Task LoadStudentsAsync()
        {
            try
            {
                var response = await _classService.GetClassStudentsAsync(_classId);
                Students.Clear();
                FilteredStudents.Clear();

                if (response?.Success == true && response.Data != null)
                {
                    foreach (var student in response.Data)
                    {
                        var item = new ClassStudentItem
                        {
                            UserId = student.UserId,
                            StudentCode = student.StudentCode,
                            FullName = student.FullName,
                            Email = student.Email,
                            Major = student.Major ?? "N/A",
                            ClassYear = student.ClassYear
                        };
                        Students.Add(item);
                        FilteredStudents.Add(item);
                    }
                }

                OnPropertyChanged(nameof(TotalStudents));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading students: {ex.Message}");
            }
        }

        private async Task LoadAssignmentsAsync()
        {
            try
            {
                var response = await _assignmentService.GetAssignmentsByClassAsync(_classId);
                Assignments.Clear();

                if (response?.Success == true && response.Data != null)
                {
                    foreach (var assignment in response.Data)
                    {
                        Assignments.Add(assignment);
                    }
                }

                OnPropertyChanged(nameof(TotalAssignments));
                OnPropertyChanged(nameof(ActiveAssignments));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading assignments: {ex.Message}");
            }
        }

        private void FilterStudents()
        {
            FilteredStudents.Clear();

            var query = Students.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(s =>
                    s.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    s.StudentCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var student in query)
            {
                FilteredStudents.Add(student);
            }
        }

        private async void ExecuteAddStudent()
        {
            if (string.IsNullOrEmpty(_classId)) return;

            try
            {
                var viewModel = new AddStudentDialogViewModel(_classId, _classService);
                var dialog = new Views.AddStudentDialog(viewModel)
                {
                    Owner = Application.Current.MainWindow
                };
                
                if (dialog.ShowDialog() == true)
                {
                    _ = LoadStudentsAsync();
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Lỗi",
                    $"Không thể mở cửa sổ thêm sinh viên: {ex.Message}\n\n{ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"Error opening AddStudentDialog: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async void ExecuteRemoveStudent(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            var student = Students.FirstOrDefault(s => s.UserId == userId);
            if (student == null) return;

            var result = await GetMetroWindow()?.ShowMessageAsync(
                "Xác nhận xóa",
                $"Bạn có chắc muốn xóa sinh viên {student.FullName} khỏi lớp?",
                MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
            {
                IsLoading = true;
                try
                {
                    var response = await _classService.RemoveStudentFromClassAsync(_classId, userId);
                    if (response?.Success == true)
                    {
                        await GetMetroWindow()?.ShowMessageAsync(
                            "Thành công",
                            "Xóa sinh viên thành công!");
                        await LoadStudentsAsync();
                    }
                    else
                    {
                        await GetMetroWindow()?.ShowMessageAsync(
                            "Lỗi",
                            $"Xóa sinh viên thất bại: {response?.Message}");
                    }
                }
                catch (Exception ex)
                {
                    await GetMetroWindow()?.ShowMessageAsync(
                        "Lỗi",
                        $"Lỗi xóa sinh viên: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async void ExecuteCreateAssignment()
        {
            if (string.IsNullOrEmpty(_classId)) return;

            try
            {
                var viewModel = new CreateAssignmentViewModel(
                    _assignmentService,
                    _classService,
                    _navigationService ?? new NavigationService());

                var dialog = new Views.CreateAssignmentWindow(viewModel);
                dialog.Owner = Application.Current.MainWindow;
                dialog.Initialize(_classId);

                if (dialog.ShowDialog() == true)
                {
                    // Reload assignments after creation
                    _ = LoadAssignmentsAsync();
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Lỗi",
                    $"Lỗi mở trang tạo bài tập: {ex.Message}");
            }
        }

        public void SetNavigationService(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        private void ExecuteViewAssignment(string assignmentId)
        {
            if (!string.IsNullOrEmpty(assignmentId) && _navigationService != null)
            {
                var assignmentPage = App.ServiceProvider?.GetService(typeof(Pages.TeacherAssignmentPage)) as Pages.TeacherAssignmentPage;
                if (assignmentPage != null)
                {
                    _navigationService.NavigateTo(assignmentPage, assignmentId);
                }
            }
        }

        private async void ExecuteEditClass()
        {
            await GetMetroWindow()?.ShowMessageAsync(
                "Thông báo",
                "Chức năng chỉnh sửa lớp học đang được phát triển.\n\nSử dụng trang web để chỉnh sửa thông tin lớp.");
        }

        // Helper methods for display
        public static string GetAssignmentTypeDisplay(AssignmentType type)
        {
            return type switch
            {
                AssignmentType.HOMEWORK => "Bài tập về nhà",
                AssignmentType.EXAMINATION => "Bài kiểm tra",
                AssignmentType.PRACTICE => "Luyện tập",
                _ => type.ToString()
            };
        }

        public static string GetAssignmentStatusDisplay(AssignmentStatus status)
        {
            return status switch
            {
                AssignmentStatus.DRAFT => "Nháp",
                AssignmentStatus.PUBLISHED => "Đã giao",
                AssignmentStatus.CLOSED => "Đã đóng",
                _ => status.ToString()
            };
        }

        public static string GetStatusColor(AssignmentStatus status)
        {
            return status switch
            {
                AssignmentStatus.PUBLISHED => "#28a745",
                AssignmentStatus.DRAFT => "#ffc107",
                AssignmentStatus.CLOSED => "#6c757d",
                _ => "#6c757d"
            };
        }
    }
}

