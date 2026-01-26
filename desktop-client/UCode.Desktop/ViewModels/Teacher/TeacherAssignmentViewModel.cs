using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
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

    public class AssignmentUserItem
    {
        public int RowNumber { get; set; }
        public string AssignmentUserId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public double? Score { get; set; }
        public double? MaxScore { get; set; }
        public int TabSwitchCount { get; set; }
        public int CapturedAICount { get; set; }
        public string? AiDetectionDetails { get; set; }
        public string StatusColor { get; set; } = string.Empty;
        public string ScoreDisplay => Score.HasValue ? $"{Score:F1}/{MaxScore:F1}" : "Chưa làm";
    }

    public class TeacherAssignmentViewModel : ViewModelBase
    {
        private readonly AssignmentService _assignmentService;
        private readonly ClassService _classService;
        private readonly ProblemService _problemService;
        private readonly NavigationService _navigationService;
        private bool _isLoading;
        private string _error = string.Empty;
        private string _assignmentId = string.Empty;
        private Assignment? _assignment;
        private Class? _classData;
        private AssignmentStatistics? _statistics;
        private int _problemsCount;
        private int _studentsCount;
        private string _searchText = string.Empty;
        private ObservableCollection<AssignmentUserItem> _allStudents = new();

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
        public ObservableCollection<AssignmentUserItem> Students { get; } = new();

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

        public string AssignmentTypeDisplay => GetAssignmentTypeDisplay(Assignment?.AssignmentType.ToString() ?? "");
        public string AssignmentStatusDisplay => GetAssignmentStatusDisplay(Assignment?.Status.ToString() ?? "");
        public string StartTimeDisplay => Assignment?.StartTime?.ToString("dd/MM/yyyy HH:mm") ?? "Không có";
        public string EndTimeDisplay => Assignment?.EndTime?.ToString("dd/MM/yyyy HH:mm") ?? "Không giới hạn";

        public ICommand RefreshCommand { get; }
        public ICommand EditAssignmentCommand { get; }
        public ICommand GradeAssignmentCommand { get; }
        public ICommand ViewProblemCommand { get; }
        public ICommand ViewStudentCommand { get; }
        public ICommand ViewSubmissionsCommand { get; }
        public ICommand DeleteProblemCommand { get; }
        public ICommand ViewAIDetailsCommand { get; }
        public ICommand EditProblemCommand { get; }

        public TeacherAssignmentViewModel(
            AssignmentService assignmentService,
            ClassService classService,
            ProblemService problemService,
            NavigationService navigationService)
        {
            _assignmentService = assignmentService;
            _classService = classService;
            _problemService = problemService;
            _navigationService = navigationService;

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            EditAssignmentCommand = new RelayCommand(_ => ExecuteEditAssignment());
            GradeAssignmentCommand = new RelayCommand(_ => ExecuteGradeAssignment());
            ViewProblemCommand = new RelayCommand(param => ExecuteViewProblem(param as string ?? ""));
            ViewStudentCommand = new RelayCommand(param => ExecuteViewStudent(param as string ?? ""));
            ViewSubmissionsCommand = new RelayCommand(param => ExecuteViewSubmissions(param as string ?? ""));
            DeleteProblemCommand = new RelayCommand(async param => await ExecuteDeleteProblem(param as string ?? ""));
            ViewAIDetailsCommand = new RelayCommand(param => ExecuteViewAIDetails(param as AssignmentUserItem));
            EditProblemCommand = new RelayCommand(param => ExecuteEditProblem(param as string ?? ""));
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
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", Error);
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
                if (Assignment == null) return;

                var classStudentsResponse = await _classService.GetClassStudentsAsync(Assignment.ClassId);
                
                var assignmentStudentsResponse = await _assignmentService.GetAssignmentStudentsAsync(_assignmentId);

                if (classStudentsResponse?.Success == true && classStudentsResponse.Data != null)
                {
                    var assignmentStudentsDict = new System.Collections.Generic.Dictionary<string, AssignmentUser>();
                    if (assignmentStudentsResponse?.Success == true && assignmentStudentsResponse.Data != null)
                    {
                        foreach (var assignmentStudent in assignmentStudentsResponse.Data)
                        {
                            assignmentStudentsDict[assignmentStudent.UserId] = assignmentStudent;
                        }
                    }

                    var sortedStudents = classStudentsResponse.Data
                        .OrderBy(s => s.StudentCode)
                        .ToList();

                    _allStudents.Clear();
                    int cnt = 0;
                    foreach (var classStudent in sortedStudents)
                    {
                        var assignmentStudent = assignmentStudentsDict.ContainsKey(classStudent.UserId) 
                            ? assignmentStudentsDict[classStudent.UserId] 
                            : null;

                        _allStudents.Add(new AssignmentUserItem
                        {
                            RowNumber = ++cnt,
                            AssignmentUserId = assignmentStudent?.AssignmentUserId ?? string.Empty,
                            UserId = classStudent.UserId,
                            FullName = classStudent.FullName ?? "N/A",
                            Email = classStudent.Email ?? "N/A",
                            StudentCode = classStudent.StudentCode ?? "N/A",
                            Status = assignmentStudent != null
                                ? GetUserStatusDisplay(assignmentStudent.Status.ToString())
                                : "Chưa tham gia",
                            StartedAt = assignmentStudent?.StartedAt,
                            SubmittedAt = null,
                            Score = assignmentStudent?.Score,
                            MaxScore = assignmentStudent?.MaxScore,
                            TabSwitchCount = assignmentStudent?.TabSwitchCount ?? 0,
                            CapturedAICount = assignmentStudent?.CapturedAICount ?? 0,
                            AiDetectionDetails = assignmentStudent?.AiDetectionDetails,
                            StatusColor = assignmentStudent != null
                                ? GetUserStatusColor(assignmentStudent.Status.ToString())
                                : "#6c757d"
                        });
                    }

                    FilterStudents();
                    StudentsCount = Students.Count;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading students: {ex.Message}");
            }
        }

        private void FilterStudents()
        {
            Students.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var student in _allStudents)
                {
                    Students.Add(student);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                foreach (var student in _allStudents)
                {
                    if (student.StudentCode.ToLower().Contains(searchLower) ||
                        student.FullName.ToLower().Contains(searchLower) ||
                        student.Email.ToLower().Contains(searchLower))
                    {
                        Students.Add(student);
                    }
                }
            }

            StudentsCount = Students.Count;
        }

        private void ExecuteEditAssignment()
        {
            var editWindow = App.ServiceProvider.GetService(typeof(Views.TeacherAssignmentEditWindow)) as Views.TeacherAssignmentEditWindow;
            if (editWindow != null)
            {
                editWindow.Owner = Application.Current.MainWindow;
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

        private async void ExecuteViewProblem(string problemId)
        {
            if (!string.IsNullOrEmpty(problemId))
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Thông báo",
                    $"Xem chi tiết problem: {problemId}\n\nChức năng đang được phát triển.");
            }
        }

        private async void ExecuteViewStudent(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Thông báo",
                    $"Xem chi tiết sinh viên: {userId}\n\nChức năng đang được phát triển.");
            }
        }

        private void ExecuteViewSubmissions(string problemId)
        {
            if (string.IsNullOrEmpty(problemId) || string.IsNullOrEmpty(_assignmentId))
            {
                return;
            }

            // Navigate to TeacherProblemSubmissionsPage
            var submissionsViewModel = new TeacherProblemSubmissionsViewModel(
                _assignmentService,
                App.ServiceProvider.GetService(typeof(SubmissionService)) as SubmissionService,
                _problemService,
                _classService,
                _navigationService);

            var submissionsPage = new Pages.TeacherProblemSubmissionsPage(submissionsViewModel);
            var parameters = new { assignmentId = _assignmentId, problemId = problemId };
            _navigationService.NavigateTo(submissionsPage, parameters);
        }

        private async Task ExecuteDeleteProblem(string problemId)
        {
            if (string.IsNullOrEmpty(problemId))
            {
                return;
            }

            var result = await GetMetroWindow()?.ShowMessageAsync(
                "Xác nhận xóa",
                "Bạn có chắc chắn muốn xóa bài này khỏi assignment?",
                MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "Xóa",
                    NegativeButtonText = "Hủy",
                    DefaultButtonFocus = MessageDialogResult.Negative
                });

            if (result == MessageDialogResult.Affirmative)
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Thông báo",
                    "Chức năng xóa bài đang được phát triển.");
            }
        }

        private async void ExecuteViewAIDetails(AssignmentUserItem? student)
        {
            if (student == null)
            {
                return;
            }

            var message = $"Thông tin AI Detection cho {student.FullName} ({student.StudentCode})\n\n";
            message += $"Số lần chuyển tab: {student.TabSwitchCount}\n";
            message += $"Số lần truy cập AI: {student.CapturedAICount}\n\n";
            
            if (!string.IsNullOrEmpty(student.AiDetectionDetails))
            {
                try
                {
                    // Parse JSON string to dictionary
                    var aiDetails = JsonSerializer.Deserialize<Dictionary<string, int>>(student.AiDetectionDetails);
                    
                    if (aiDetails != null && aiDetails.Count > 0)
                    {
                        message += "Chi tiết truy cập:\n";
                        foreach (var aiTool in aiDetails.OrderByDescending(x => x.Value))
                        {
                            message += $"  • {aiTool.Key}: {aiTool.Value} lần\n";
                        }
                    }
                    else
                    {
                        message += "Chưa có chi tiết truy cập AI.";
                    }
                }
                catch (JsonException)
                {
                    // If JSON parsing fails, show raw data
                    message += $"Chi tiết:\n{student.AiDetectionDetails}";
                }
            }
            else
            {
                message += "Chưa có chi tiết truy cập AI.";
            }

            await GetMetroWindow()?.ShowMessageAsync(
                "Chi tiết truy cập AI",
                message);
        }

        private string GetAssignmentTypeDisplay(string type)
        {
            return type switch
            {
                "HOMEWORK" => "Bài tập về nhà",
                "EXAMINATION" => "Bài kiểm tra",
                "PRACTICE" => "Luyện tập",
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

        private void ExecuteEditProblem(string problemId)
        {
            if (string.IsNullOrEmpty(problemId)) return;

            try
            {
                // Create ProblemEditPage and navigate
                var problemEditPage = App.ServiceProvider.GetService(typeof(Pages.ProblemEditPage)) as Pages.ProblemEditPage;
                if (problemEditPage != null)
                {
                    _navigationService.NavigateTo(problemEditPage, problemId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening problem editor: {ex.Message}");
            }
        }
    }
}

