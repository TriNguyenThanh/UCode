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
    public class TeacherClassItem
    {
        public string ClassId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class TeacherAssignmentItem
    {
        public string AssignmentId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int Submitted { get; set; }
        public int Graded { get; set; }
        public int ProblemCount { get; set; }
        public DateTime? EndTime { get; set; }
        public string DueDateDisplay => EndTime?.ToString("dd/MM/yyyy") ?? "N/A";
        public string SubmissionDisplay => $"{Submitted}/{TotalStudents} đã nộp";
    }

    public class TeacherProblemItem
    {
        public string ProblemId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TeacherHomeViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private readonly ClassService _classService;
        private readonly AssignmentService _assignmentService;
        private readonly ProblemService _problemService;
        private NavigationService? _navigationService;
        private bool _isLoading;
        private string _teacherName = string.Empty;
        private string _teacherEmail = string.Empty;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string TeacherName
        {
            get => _teacherName;
            set => SetProperty(ref _teacherName, value);
        }

        public string TeacherEmail
        {
            get => _teacherEmail;
            set => SetProperty(ref _teacherEmail, value);
        }

        public ObservableCollection<TeacherClassItem> Classes { get; } = new();
        public ObservableCollection<TeacherAssignmentItem> RecentAssignments { get; } = new();
        public ObservableCollection<TeacherProblemItem> RecentProblems { get; } = new();

        public int TotalClasses => Classes.Count;
        public int TotalStudents { get; private set; }
        public int ActiveAssignments { get; private set; }
        public int PendingGrading { get; private set; }

        public ICommand CreateClassCommand { get; }
        public ICommand CreateProblemCommand { get; }
        public ICommand ViewClassCommand { get; }
        public ICommand ViewAssignmentCommand { get; }
        public ICommand ViewProblemCommand { get; }
        public ICommand GradeAssignmentCommand { get; }
        public ICommand ViewReportCommand { get; }
        public ICommand ViewAllAssignmentsCommand { get; }
        public ICommand ViewAllProblemsCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand RefreshCommand { get; }

        public TeacherHomeViewModel(
            AuthService authService,
            ClassService classService,
            AssignmentService assignmentService,
            ProblemService problemService)
        {
            _authService = authService;
            _classService = classService;
            _assignmentService = assignmentService;
            _problemService = problemService;

            CreateClassCommand = new RelayCommand(_ => ExecuteCreateClass());
            CreateProblemCommand = new RelayCommand(_ => ExecuteCreateProblem());
            ViewClassCommand = new RelayCommand(param => ExecuteViewClass(param as string));
            ViewAssignmentCommand = new RelayCommand(param => ExecuteViewAssignment(param as string));
            ViewProblemCommand = new RelayCommand(param => ExecuteViewProblem(param as string));
            GradeAssignmentCommand = new RelayCommand(param => ExecuteGradeAssignment(param as string));
            ViewReportCommand = new RelayCommand(param => ExecuteViewReport(param as string));
            ViewAllAssignmentsCommand = new RelayCommand(_ => ExecuteViewAllAssignments());
            ViewAllProblemsCommand = new RelayCommand(_ => ExecuteViewAllProblems());
            LogoutCommand = new RelayCommand(_ => ExecuteLogout());
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());

            var currentUser = authService.CurrentUser;
            TeacherName = currentUser?.FullName ?? "Teacher";
            TeacherEmail = currentUser?.Email ?? "teacher@example.com";
        }

        public void SetNavigationService(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                await Task.WhenAll(
                    LoadClassesAsync(),
                    LoadAssignmentsAsync(),
                    LoadProblemsAsync()
                );
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
                var response = await _classService.GetMyClassesAsync(1, 10);
                Classes.Clear();

                if (response?.Success == true && response.Data?.Items != null)
                {
                    foreach (var cls in response.Data.Items)
                    {
                        Classes.Add(new TeacherClassItem
                        {
                            ClassId = cls.ClassId,
                            Name = cls.ClassName,
                            Code = cls.ClassCode,
                            Semester = cls.Semester,
                            StudentCount = cls.StudentCount,
                            Description = cls.Description
                        });
                    }
                }

                // Calculate total students
                TotalStudents = Classes.Sum(c => c.StudentCount);

                OnPropertyChanged(nameof(TotalClasses));
                OnPropertyChanged(nameof(TotalStudents));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception loading classes: {ex.Message}");
            }
        }

        private async Task LoadAssignmentsAsync()
        {
            try
            {
                var response = await _assignmentService.GetMyAssignmentsAsync();
                RecentAssignments.Clear();

                if (response?.Success == true && response.Data != null)
                {
                    foreach (var assignment in response.Data)
                    {
                        RecentAssignments.Add(new TeacherAssignmentItem
                        {
                            AssignmentId = assignment.AssignmentId,
                            Title = assignment.Title,
                            ClassName = assignment.ClassName,
                            Type = assignment.AssignmentType.ToString(),
                            Status = assignment.Status.ToString(),
                            ProblemCount = assignment.Problems?.Count ?? 0,
                            EndTime = assignment.EndTime,
                            TotalStudents = 0, // Will be loaded separately if needed
                            Submitted = 0,
                            Graded = 0
                        });
                    }

                    // Calculate stats
                    ActiveAssignments = response.Data.Count(a => 
                        a.Status == Models.AssignmentStatus.PUBLISHED);
                    PendingGrading = response.Data.Sum(a => 
                        (a.Statistics?.Submitted ?? 0) - (a.Statistics?.Graded ?? 0));
                }

                OnPropertyChanged(nameof(ActiveAssignments));
                OnPropertyChanged(nameof(PendingGrading));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception loading assignments: {ex.Message}");
            }
        }

        private async Task LoadProblemsAsync()
        {
            try
            {
                var response = await _problemService.GetMyProblemsAsync(1, 10);
                RecentProblems.Clear();

                if (response?.Success == true && response.Data?.Items != null)
                {
                    foreach (var problem in response.Data.Items)
                    {
                        RecentProblems.Add(new TeacherProblemItem
                        {
                            ProblemId = problem.ProblemId,
                            Code = problem.Code,
                            Title = problem.Title,
                            Difficulty = problem.Difficulty.ToString(),
                            Status = problem.Status.ToString(),
                            Visibility = problem.Visibility.ToString(),
                            CreatedAt = problem.CreatedAt
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception loading problems: {ex.Message}");
            }
        }

        private void ExecuteCreateClass()
        {
            MessageBox.Show(
                "Chức năng tạo lớp học đang được phát triển.\n\nVui lòng sử dụng trang web để tạo lớp học mới.",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ExecuteCreateProblem()
        {
            var problemsWindow = App.ServiceProvider.GetService(typeof(Views.TeacherProblemsWindow)) as Views.TeacherProblemsWindow;
            if (problemsWindow != null)
            {
                problemsWindow.Show();
            }
        }

        private void ExecuteViewClass(string classId)
        {
            if (!string.IsNullOrEmpty(classId) && _navigationService != null)
            {
                var classPage = App.ServiceProvider.GetService(typeof(Pages.TeacherClassPage)) as Pages.TeacherClassPage;
                if (classPage != null)
                {
                    _navigationService.NavigateTo(classPage, classId);
                }
            }
        }

        private void ExecuteViewAssignment(string assignmentId)
        {
            if (!string.IsNullOrEmpty(assignmentId) && _navigationService != null)
            {
                var assignmentPage = App.ServiceProvider.GetService(typeof(Pages.TeacherAssignmentPage)) as Pages.TeacherAssignmentPage;
                if (assignmentPage != null)
                {
                    _navigationService.NavigateTo(assignmentPage, assignmentId);
                }
            }
        }

        private void ExecuteViewProblem(string problemId)
        {
            if (!string.IsNullOrEmpty(problemId))
            {
                MessageBox.Show(
                    $"Xem chi tiết problem: {problemId}\n\nChức năng đang được phát triển.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void ExecuteGradeAssignment(string assignmentId)
        {
            if (!string.IsNullOrEmpty(assignmentId))
            {
                var gradingWindow = App.ServiceProvider.GetService(typeof(Views.TeacherGradingWindow)) as Views.TeacherGradingWindow;
                if (gradingWindow != null)
                {
                    gradingWindow.Initialize(assignmentId);
                    gradingWindow.Show();
                }
            }
        }

        private void ExecuteViewReport(string assignmentId)
        {
            if (!string.IsNullOrEmpty(assignmentId))
            {
                MessageBox.Show(
                    $"Xem báo cáo bài tập: {assignmentId}\n\nChức năng đang được phát triển.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void ExecuteViewAllAssignments()
        {
            MessageBox.Show(
                "Trang danh sách tất cả bài tập đang được phát triển.\n\nHiện tại bạn có thể xem bài tập từ trang chủ.",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ExecuteViewAllProblems()
        {
            var problemsWindow = App.ServiceProvider.GetService(typeof(Views.TeacherProblemsWindow)) as Views.TeacherProblemsWindow;
            if (problemsWindow != null)
            {
                problemsWindow.Show();
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

                // Change shutdown mode back to explicit before closing main window
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                var loginWindow = App.ServiceProvider.GetService(typeof(Views.LoginWindow)) as Views.LoginWindow;
                loginWindow?.Show();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Views.TeacherHomeWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            }
        }
    }
}

