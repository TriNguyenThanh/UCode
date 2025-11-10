using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class ClassItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
    }

    public class AssignmentItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int DaysLeft { get; set; }
        public int ProblemCount { get; set; }
        public int TotalPoints { get; set; }
    }

    public class PracticeCategoryItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ProblemCount { get; set; }
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private readonly ApiService _apiService;
        private string _userEmail = string.Empty;
        private string _userName = string.Empty;
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string UserEmail
        {
            get => _userEmail;
            set => SetProperty(ref _userEmail, value);
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public ObservableCollection<ClassItem> Classes { get; } = new();
        public ObservableCollection<AssignmentItem> UpcomingAssignments { get; } = new();
        public ObservableCollection<PracticeCategoryItem> PracticeCategories { get; } = new();

        public int UpcomingAssignmentsCount => UpcomingAssignments.Count;

        public ICommand LogoutCommand { get; }

        public MainViewModel(AuthService authService, ApiService apiService)
        {
            _authService = authService;
            _apiService = apiService;

            LogoutCommand = new RelayCommand(_ => ExecuteLogout());

            // Set user info
            var currentUser = authService.CurrentUser;
            UserEmail = currentUser?.Email ?? "user@example.com";
            UserName = currentUser?.Email?.Split('@')[0] ?? "User";
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                await LoadClassesAsync();
                await LoadUpcomingAssignmentsAsync();
                LoadPracticeCategories();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadClassesAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<PagedResponse<Class>>("/api/v1/classes");

                Classes.Clear();
                
                if (response?.Success == true && response.Data?.Items != null)
                {
                    foreach (var cls in response.Data.Items)
                    {
                        Classes.Add(new ClassItem
                        {
                            Id = cls.ClassId,
                            Name = cls.ClassName,
                            Code = cls.ClassCode,
                            TeacherName = cls.TeacherName,
                            Semester = cls.Semester
                        });
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Load classes failed: {response?.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception loading classes: {ex.Message}");
                MessageBox.Show($"Không thể tải danh sách lớp học: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task LoadUpcomingAssignmentsAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<PagedResponse<Assignment>>("/api/v1/assignments/upcoming");

                UpcomingAssignments.Clear();
                
                if (response?.Success == true && response.Data?.Items != null)
                {
                    foreach (var assignment in response.Data.Items)
                    {
                        var daysLeft = (assignment.DueDate - DateTime.Now).Days;
                        
                        UpcomingAssignments.Add(new AssignmentItem
                        {
                            Id = assignment.AssignmentId,
                            Title = assignment.Title,
                            ClassName = assignment.ClassName ?? "Unknown Class",
                            DaysLeft = daysLeft > 0 ? daysLeft : 0,
                            ProblemCount = assignment.ProblemCount,
                            TotalPoints = assignment.TotalPoints
                        });
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Load assignments failed: {response?.Message}");
                }

                OnPropertyChanged(nameof(UpcomingAssignmentsCount));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception loading assignments: {ex.Message}");
                UpcomingAssignments.Clear();
                OnPropertyChanged(nameof(UpcomingAssignmentsCount));
            }
        }

        private async void LoadPracticeCategories()
        {
            try
            {
                var response = await _apiService.GetAsync<List<PracticeCategory>>("/api/v1/practice/categories");

                PracticeCategories.Clear();
                
                if (response?.Success == true && response.Data != null)
                {
                    foreach (var category in response.Data)
                    {
                        PracticeCategories.Add(new PracticeCategoryItem
                        {
                            Id = category.CategoryId,
                            Name = category.Name,
                            Icon = category.Icon ?? "�",
                            Description = category.Description ?? string.Empty,
                            ProblemCount = category.ProblemCount
                        });
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Load practice categories failed: {response?.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception loading practice categories: {ex.Message}");
                PracticeCategories.Clear();
            }
        }

        private void ExecuteLogout()
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất?",
                "Đăng xuất",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                _authService.Logout();

                // Close main window and show login window
                var loginWindow = App.ServiceProvider.GetService(typeof(Views.LoginWindow)) as Views.LoginWindow;
                loginWindow?.Show();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Views.MainWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            }
        }
    }
}

