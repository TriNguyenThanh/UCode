using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ICommand NavigateToClassCommand { get; }
        public ICommand NavigateToAssignmentCommand { get; }

        public MainViewModel(AuthService authService, ApiService apiService)
        {
            _authService = authService;
            _apiService = apiService;

            LogoutCommand = new RelayCommand(_ => ExecuteLogout());
            NavigateToClassCommand = new RelayCommand<string>(NavigateToClass);
            NavigateToAssignmentCommand = new RelayCommand<string>(NavigateToAssignment);

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
                                Semester = cls.Semester
                            });
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"Loaded {Classes.Count} enrolled classes for student");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Load enrolled classes failed: {response?.Message}");
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
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Load classes failed: {response?.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception loading classes: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Không thể tải danh sách lớp học: {ex.Message}");
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
                                
                                UpcomingAssignments.Add(new AssignmentItem
                                {
                                    Id = assignment.AssignmentId,
                                    Title = assignment.Title,
                                    ClassName = assignment.ClassName ?? "Unknown Class",
                                    DaysLeft = daysLeft > 0 ? daysLeft : 0,
                                    ProblemCount = assignment.TotalProblems ?? 0,
                                    TotalPoints = assignment.TotalPoints
                                });
                            }
                        }
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

        private void NavigateToClass(string classId)
        {
            if (string.IsNullOrEmpty(classId)) return;

            var classWindow = App.ServiceProvider.GetService(typeof(Views.ClassDetailWindow)) as Views.ClassDetailWindow;
            if (classWindow != null)
            {
                var viewModel = classWindow.DataContext as ClassDetailViewModel;
                if (viewModel != null)
                {
                    _ = viewModel.InitializeAsync(classId);
                    classWindow.Show();
                }
            }
        }

        private void NavigateToAssignment(string assignmentId)
        {
            if (string.IsNullOrEmpty(assignmentId)) return;

            var assignmentWindow = App.ServiceProvider.GetService(typeof(Views.AssignmentDetailWindow)) as Views.AssignmentDetailWindow;
            if (assignmentWindow != null)
            {
                var viewModel = assignmentWindow.DataContext as AssignmentDetailViewModel;
                if (viewModel != null)
                {
                    _ = viewModel.InitializeAsync(assignmentId);
                    assignmentWindow.Show();
                }
            }
        }

        private async void ExecuteLogout()
        {
            var result = await GetMetroWindow()?.ShowMessageAsync(
                "Đăng xuất",
                "Bạn có chắc muốn đăng xuất?",
                MessageDialogStyle.AffirmativeAndNegative
            );

            if (result == MessageDialogResult.Affirmative)
            {
                _authService.Logout();

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
}

