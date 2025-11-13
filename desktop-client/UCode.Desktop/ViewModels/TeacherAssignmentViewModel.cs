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
    public class AssignmentProblemItem
    {
        public string ProblemId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int Points { get; set; }
        public int OrderIndex { get; set; }
        public string DifficultyColor { get; set; } = string.Empty;
    }

    public class AssignmentStudentItem
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public double? Score { get; set; }
        public double? MaxScore { get; set; }
        public string StatusColor { get; set; } = string.Empty;
        public string ScoreDisplay => Score.HasValue ? $"{Score:F1}/{MaxScore:F1}" : "Chưa làm";
    }

    public class TeacherAssignmentViewModel : ViewModelBase
    {
        private readonly AssignmentService _assignmentService;
        private readonly ClassService _classService;
        private readonly ProblemService _problemService;
        private bool _isLoading;
        private string _error = string.Empty;
        private string _assignmentId = string.Empty;
        private Assignment? _assignment;
        private Class? _classData;
        private AssignmentStatistics? _statistics;
        private int _problemsCount;
        private int _studentsCount;

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

        public Assignment? Assignment
        {
            get => _assignment;
            set => SetProperty(ref _assignment, value);
        }

        public Class? ClassData
        {
            get => _classData;
            set => SetProperty(ref _classData, value);
        }

        public AssignmentStatistics? Statistics
        {
            get => _statistics;
            set => SetProperty(ref _statistics, value);
        }

        public ObservableCollection<AssignmentProblemItem> Problems { get; } = new();
        public ObservableCollection<AssignmentStudentItem> Students { get; } = new();

        public int ProblemsCount
        {
            get => _problemsCount;
            set => SetProperty(ref _problemsCount, value);
        }

        public int StudentsCount
        {
            get => _studentsCount;
            set => SetProperty(ref _studentsCount, value);
        }

        public string AssignmentTypeDisplay => GetAssignmentTypeDisplay(Assignment?.AssignmentType.ToString() ?? "");
        public string AssignmentStatusDisplay => GetAssignmentStatusDisplay(Assignment?.Status.ToString() ?? "");
        public string StartTimeDisplay => Assignment?.StartTime?.ToString("dd/MM/yyyy HH:mm") ?? "Không có";
        public string EndTimeDisplay => Assignment?.EndTime?.ToString("dd/MM/yyyy HH:mm") ?? "Không giới hạn";

        public ICommand RefreshCommand { get; }
        public ICommand EditAssignmentCommand { get; }
        public ICommand GradeAssignmentCommand { get; }
        public ICommand ViewProblemCommand { get; }
        public ICommand ViewStudentCommand { get; }

        public TeacherAssignmentViewModel(
            AssignmentService assignmentService,
            ClassService classService,
            ProblemService problemService)
        {
            _assignmentService = assignmentService;
            _classService = classService;
            _problemService = problemService;

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            EditAssignmentCommand = new RelayCommand(_ => ExecuteEditAssignment());
            GradeAssignmentCommand = new RelayCommand(_ => ExecuteGradeAssignment());
            ViewProblemCommand = new RelayCommand(param => ExecuteViewProblem(param as string ?? ""));
            ViewStudentCommand = new RelayCommand(param => ExecuteViewStudent(param as string ?? ""));
        }

        public async Task InitializeAsync(string assignmentId)
        {
            _assignmentId = assignmentId;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            Error = string.Empty;

            try
            {
                // Load assignment
                var assignmentResponse = await _assignmentService.GetAssignmentAsync(_assignmentId);
                if (assignmentResponse?.Success != true || assignmentResponse.Data == null)
                {
                    Error = "Không thể tải thông tin bài tập";
                    return;
                }

                Assignment = assignmentResponse.Data;
                OnPropertyChanged(nameof(AssignmentTypeDisplay));
                OnPropertyChanged(nameof(AssignmentStatusDisplay));
                OnPropertyChanged(nameof(StartTimeDisplay));
                OnPropertyChanged(nameof(EndTimeDisplay));

                // Load class data
                var classResponse = await _classService.GetClassByIdAsync(Assignment.ClassId);
                if (classResponse?.Success == true)
                {
                    ClassData = classResponse.Data;
                }

                // Load statistics
                var statsResponse = await _assignmentService.GetAssignmentStatisticsAsync(_assignmentId);
                if (statsResponse?.Success == true)
                {
                    Statistics = statsResponse.Data;
                }

                // Load problems
                await LoadProblemsAsync();

                // Load students
                await LoadStudentsAsync();
            }
            catch (Exception ex)
            {
                Error = $"Lỗi tải dữ liệu: {ex.Message}";
                MessageBox.Show(Error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Task LoadProblemsAsync()
        {
            Problems.Clear();

            if (Assignment?.Problems == null) return Task.CompletedTask;

            // Use problem basics from assignment detail API (no need to call individual problem APIs)
            foreach (var assignmentProblem in Assignment.Problems)
            {
                Problems.Add(new AssignmentProblemItem
                {
                    ProblemId = assignmentProblem.ProblemId,
                    Code = assignmentProblem.Code,
                    Title = assignmentProblem.Title,
                    Difficulty = assignmentProblem.Difficulty.ToString(),
                    Points = assignmentProblem.Points,
                    OrderIndex = assignmentProblem.OrderIndex,
                    DifficultyColor = GetDifficultyColor(assignmentProblem.Difficulty.ToString())
                });
            }

            ProblemsCount = Problems.Count;
            return Task.CompletedTask;
        }

        private async Task LoadStudentsAsync()
        {
            Students.Clear();

            try
            {
                var response = await _assignmentService.GetAssignmentStudentsAsync(_assignmentId);
                if (response?.Success == true && response.Data != null)
                {
                    foreach (var student in response.Data)
                    {
                        Students.Add(new AssignmentStudentItem
                        {
                            UserId = student.UserId,
                            FullName = student.User?.FullName ?? "N/A",
                            StudentCode = student.User?.StudentCode ?? "N/A",
                            Status = GetUserStatusDisplay(student.Status.ToString()),
                            StartedAt = student.StartedAt,
                            SubmittedAt = null, // Can be derived from status
                            Score = student.Score,
                            MaxScore = student.MaxScore,
                            StatusColor = GetUserStatusColor(student.Status.ToString())
                        });
                    }

                    StudentsCount = Students.Count;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading students: {ex.Message}");
            }
        }

        private void ExecuteEditAssignment()
        {
            var editWindow = App.ServiceProvider.GetService(typeof(Views.TeacherAssignmentEditWindow)) as Views.TeacherAssignmentEditWindow;
            if (editWindow != null)
            {
                editWindow.Initialize(_assignmentId);
                editWindow.ShowDialog();
                
                // Reload data after editing
                _ = LoadDataAsync();
            }
        }

        private void ExecuteGradeAssignment()
        {
            // Open grading window
            var gradingWindow = App.ServiceProvider.GetService(typeof(Views.TeacherGradingWindow)) as Views.TeacherGradingWindow;
            if (gradingWindow != null)
            {
                gradingWindow.Initialize(_assignmentId);
                gradingWindow.Show();
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

        private void ExecuteViewStudent(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                MessageBox.Show(
                    $"Xem chi tiết sinh viên: {userId}\n\nChức năng đang được phát triển.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private string GetAssignmentTypeDisplay(string type)
        {
            return type switch
            {
                "HOMEWORK" => "Bài tập về nhà",
                "EXAM" => "Bài kiểm tra",
                "PRACTICE" => "Luyện tập",
                "CONTEST" => "Thi đấu",
                _ => type ?? "N/A"
            };
        }

        private string GetAssignmentStatusDisplay(string status)
        {
            return status switch
            {
                "DRAFT" => "Nháp",
                "PUBLISHED" => "Đã giao",
                "CLOSED" => "Đã đóng",
                _ => status ?? "N/A"
            };
        }

        private string GetUserStatusDisplay(string status)
        {
            return status switch
            {
                "NOT_STARTED" => "Chưa bắt đầu",
                "IN_PROGRESS" => "Đang làm",
                "SUBMITTED" => "Đã nộp",
                "GRADED" => "Đã chấm",
                _ => status
            };
        }

        private string GetUserStatusColor(string status)
        {
            return status switch
            {
                "NOT_STARTED" => "#6c757d",
                "IN_PROGRESS" => "#ffc107",
                "SUBMITTED" => "#17a2b8",
                "GRADED" => "#28a745",
                _ => "#6c757d"
            };
        }

        private string GetDifficultyColor(string difficulty)
        {
            return difficulty switch
            {
                "EASY" => "#28a745",
                "MEDIUM" => "#ffc107",
                "HARD" => "#dc3545",
                _ => "#6c757d"
            };
        }
    }
}

