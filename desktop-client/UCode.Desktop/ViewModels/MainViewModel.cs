using System;
using System.Collections.Generic;
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
    public class ClassItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public int StudentCount { get; set; }
    }

    public class AssignmentItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string AssignmentType { get; set; } = string.Empty;
        public AssignmentType RawAssignmentType { get; set; }
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
        private readonly NavigationService _navigationService;
        private readonly AIDetectorService _aiDetectorService;
        private string _userEmail = string.Empty;
        private string _userName = string.Empty;
        private bool _isLoading;
        private bool _isNavigationBarVisible = true;
        private bool _canGoBack;
        private string _activeTab = "Home"; // Default to Home

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsNavigationBarVisible
        {
            get => _isNavigationBarVisible;
            set => SetProperty(ref _isNavigationBarVisible, value);
        }

        public bool CanGoBack
        {
            get => _canGoBack;
            set => SetProperty(ref _canGoBack, value);
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

        public string ActiveTab
        {
            get => _activeTab;
            set => SetProperty(ref _activeTab, value);
        }

        public ObservableCollection<ClassItem> Classes { get; } = new();
        public ObservableCollection<AssignmentItem> UpcomingAssignments { get; } = new();
        public ObservableCollection<PracticeCategoryItem> PracticeCategories { get; } = new();

        public int UpcomingAssignmentsCount => UpcomingAssignments.Count;

        public ICommand LogoutCommand { get; }
        public ICommand NavigateToClassCommand { get; }
        public ICommand NavigateToAssignmentCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToAssignmentsCommand { get; }
        public ICommand NavigateToSubmissionsCommand { get; }

        public MainViewModel(AuthService authService, ApiService apiService, NavigationService navigationService, AIDetectorService aiDetectorService)
        {
            _authService = authService;
            _apiService = apiService;
            _navigationService = navigationService;
            _aiDetectorService = aiDetectorService;
            LogoutCommand = new RelayCommand(_ => ExecuteLogout());
            NavigateToClassCommand = new RelayCommand<string>(NavigateToClass);
            NavigateToAssignmentCommand = new RelayCommand<string>(NavigateToAssignment);
            GoBackCommand = new RelayCommand(_ => ExecuteGoBack(), _ => CanGoBack);
            NavigateToHomeCommand = new RelayCommand(_ => ExecuteNavigateToHome());
            NavigateToAssignmentsCommand = new RelayCommand(_ => ExecuteNavigateToAssignments());
            NavigateToSubmissionsCommand = new RelayCommand(_ => ExecuteNavigateToSubmissions());

            // Subscribe to navigation events
            _navigationService.Navigated += OnNavigated;
            _navigationService.CanGoBackChanged += OnCanGoBackChanged;

            // Set user info
            var currentUser = authService.CurrentUser;
            UserEmail = currentUser?.Email ?? "user@example.com";
            UserName = currentUser?.Username ?? "User";
            _aiDetectorService = aiDetectorService;

            ActiveTab = "Home";
        }

        // ...

        private void ExecuteNavigateToAssignments()
        {
            ActiveTab = "Assignments";
            var page = new Pages.Students.StudentAssignmentsPage();
            _navigationService.NavigateTo(page, null, false); // Don't add to stack
        }

        private void ExecuteNavigateToSubmissions()
        {
            ActiveTab = "Submissions";
            var page = new Pages.Students.StudentSubmissionsPage();
            _navigationService.NavigateTo(page, null, false); // Don't add to stack
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                await LoadUserProfileAsync();
                await LoadClassesAsync();
                await LoadUpcomingAssignmentsAsync();
                LoadPracticeCategories();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnNavigated(object? sender, System.Windows.Controls.UserControl page)
        {
            // Hide navigation bar for ProblemSolverPage
            if (page is Pages.ProblemSolverPage)
            {
                IsNavigationBarVisible = false;
            }
            else
            {
                IsNavigationBarVisible = true;
            }
        }

        private void OnCanGoBackChanged(object? sender, bool canGoBack)
        {
            CanGoBack = canGoBack;
        }

        private void ExecuteGoBack()
        {
            _navigationService.GoBack();
        }

        private void ExecuteNavigateToHome()
        {
            ActiveTab = "Home";
            // Clear navigation stack and go back to home
            _navigationService.ClearNavigationStack();
        }

        private async Task LoadUserProfileAsync()
        {
            try
            {
                var currentUser = _authService.CurrentUser;
                if (currentUser?.Role.ToString().ToLower() == "student")
                {
                    var response = await _apiService.GetAsync<StudentResponse>("/api/v1/students/me");
                    if (response?.Success == true && response.Data != null)
                    {
                        if (!string.IsNullOrEmpty(response.Data.FullName))
                        {
                            UserName = response.Data.FullName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profile: {ex.Message}");
            }
        }

        private async Task LoadClassesAsync()
        {
            try
            {
                var currentUser = _authService.CurrentUser;
                Classes.Clear();

                if (currentUser?.Role.ToString().ToLower() == "student")
                {
                    // Student: Get enrolled classes - returns ApiResponse<List<Class>>
                    var response = await _apiService.GetAsync<List<Class>>("/api/v1/classes/enrolled");

                    if (response?.Success == true && response.Data != null)
                    {
                        foreach (var cls in response.Data)
                        {
                            Classes.Add(new ClassItem
                            {
                                Id = cls.ClassId,
                                Name = cls.ClassName,
                                Code = cls.ClassCode,
                                TeacherName = cls.TeacherName,
                                Semester = cls.Semester,
                                StudentCount = cls.StudentCount
                            });
                        }
                    }
                }
                else
                {
                    // Teacher/Admin: Get all classes with pagination
                    var response = await _apiService.GetAsync<PagedResponse<Class>>("/api/v1/classes");

                    if (response?.Success == true && response.Data != null)
                    {
                        foreach (var cls in response.Data.Items ?? new List<Class>())
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
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception loading classes: {ex.Message}");
            }
        }

        private async Task LoadUpcomingAssignmentsAsync()
        {
            try
            {
                var currentUser = _authService.CurrentUser;
                ApiResponse<List<Assignment>> response = null;

                if (currentUser?.Role.ToString().ToLower() == "student")
                {
                    var assignmentService = App.ServiceProvider.GetService(typeof(AssignmentService)) as AssignmentService;
                    response = await assignmentService.GetStudentAssignmentsAsync();
                }
                else
                {
                    response = await _apiService.GetAsync<List<Assignment>>("/api/v1/assignments/my-assignments");
                }

                UpcomingAssignments.Clear();

                if (response?.Success == true && response.Data != null)
                {
                    var now = DateTime.Now;
                    var sevenDaysLater = now.AddDays(7);

                    foreach (var assignment in response.Data)
                    {
                        if (assignment.EndTime.HasValue)
                        {
                            var dueDate = assignment.EndTime.Value;
                            if (dueDate <= sevenDaysLater && dueDate > now)
                            {
                                var daysLeft = (dueDate - now).Days;

                                // Convert AssignmentType enum to string
                                string typeDisplay = assignment.AssignmentType switch
                                {
                                    AssignmentType.HOMEWORK => "Bài tập về nhà",
                                    AssignmentType.EXAMINATION => "Bài kiểm tra",
                                    AssignmentType.PRACTICE => "Luyện tập",
                                    _ => "Bài tập"
                                };

                                UpcomingAssignments.Add(new AssignmentItem
                                {
                                    Id = assignment.AssignmentId,
                                    Title = assignment.Title,
                                    ClassName = assignment.ClassName ?? "Unknown Class",
                                    AssignmentType = typeDisplay,
                                    RawAssignmentType = assignment.AssignmentType,
                                    DaysLeft = daysLeft > 0 ? daysLeft : 0,
                                    ProblemCount = assignment.TotalProblems ?? 0,
                                    TotalPoints = assignment.TotalPoints
                                });
                            }
                        }
                    }
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

        private void LoadPracticeCategories()
        {
            // Use mock data to match React client
            PracticeCategories.Clear();

            PracticeCategories.Add(new PracticeCategoryItem
            {
                Id = "algorithms",
                Name = "Thuật toán",
                Icon = "🧮",
                Description = "Luyện tập các thuật toán cơ bản đến nâng cao",
                ProblemCount = 150
            });

            PracticeCategories.Add(new PracticeCategoryItem
            {
                Id = "data-structures",
                Name = "Cấu trúc dữ liệu",
                Icon = "📦",
                Description = "Ngăn xếp, hàng đợi, cây, đồ thị và hơn thế nữa",
                ProblemCount = 120
            });

            PracticeCategories.Add(new PracticeCategoryItem
            {
                Id = "sql",
                Name = "SQL",
                Icon = "🗄️",
                Description = "Truy vấn cơ sở dữ liệu từ cơ bản đến nâng cao",
                ProblemCount = 80
            });

            PracticeCategories.Add(new PracticeCategoryItem
            {
                Id = "ai",
                Name = "Trí tuệ nhân tạo",
                Icon = "🤖",
                Description = "Các bài toán về AI và Machine Learning",
                ProblemCount = 45
            });
        }

        private void NavigateToClass(string classId)
        {
            if (string.IsNullOrEmpty(classId)) return;

            try
            {
                var classDetailPage = new Views.Students.ClassDetailPage();
                _navigationService.NavigateTo(classDetailPage, classId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        private async void NavigateToAssignment(string assignmentId)
        {
            if (string.IsNullOrEmpty(assignmentId)) return;

            var assignment = UpcomingAssignments.FirstOrDefault(a => a.Id == assignmentId);
            //var response = await _assignmentService.GetAssignmentAsync(assignmentId);

            //if (response.Success && response.Data != null)
            //{
            //var assignment = response.Data;

            if (assignment.AssignmentType == AssignmentType.EXAMINATION.ToString())
            {
                if (await _aiDetectorService.ConfirmMessageAIDetector(assignmentId) == false)
                {
                    return;
                }
                _aiDetectorService.StartAutoMonitor();
            }

            //}

            try
            {
                var assignmentDetailPage = new Views.Students.AssignmentDetailPage();
                _navigationService.NavigateTo(assignmentDetailPage, assignmentId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        private async void ExecuteLogout()
        {
            var metroWindow = GetMetroWindow();
            MessageDialogResult result;

            if (metroWindow != null)
            {
                result = await metroWindow.ShowMessageAsync(
                    "Đăng xuất",
                    "Bạn có chắc muốn đăng xuất?",
                    MessageDialogStyle.AffirmativeAndNegative
                );
            }
            else
            {
                // Fallback to MessageBox for UCodeWindow
                var messageResult = MessageBox.Show(
                    "Bạn có chắc muốn đăng xuất?",
                    "Đăng xuất",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );
                result = messageResult == MessageBoxResult.Yes
                    ? MessageDialogResult.Affirmative
                    : MessageDialogResult.Negative;
            }

            if (result == MessageDialogResult.Affirmative)
            {
                _authService.Logout();

                // Clear navigation stack
                _navigationService.ClearNavigationStack();

                // Change shutdown mode back to explicit before closing main window
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

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

    // Helper class for student response
    public class StudentResponse
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public string StudentCode { get; set; }
        public string Email { get; set; }
    }
}
