using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class StudentSubmissionItem
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public string SubmittedAtDisplay => SubmittedAt?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa nộp";
        public double? Score { get; set; }
        public double? MaxScore { get; set; }
        public string ScoreDisplay => Score.HasValue ? $"{Score:F1}/{MaxScore:F1}" : "Chưa chấm";
    }

    public class ProblemSubmissionItem
    {
        public string ProblemId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int PassedTestCases { get; set; }
        public int TotalTestCases { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
    }

    public class TeacherGradingViewModel : ViewModelBase
    {
        private readonly AssignmentService _assignmentService;
        private readonly SubmissionService _submissionService;
        private readonly ProblemService _problemService;
        private readonly ClassService _classService;

        private bool _isLoading;
        private bool _isSaving;
        private string _error = string.Empty;
        private string _assignmentId = string.Empty;
        private Assignment _assignment;
        private Class _classData;
        private int _currentStudentIndex;
        private int _currentProblemIndex;
        private string _sourceCode = string.Empty;
        private double _score;
        private string _feedback = string.Empty;

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

        public Assignment Assignment
        {
            get => _assignment;
            set => SetProperty(ref _assignment, value);
        }

        public Class ClassData
        {
            get => _classData;
            set => SetProperty(ref _classData, value);
        }

        public int CurrentStudentIndex
        {
            get => _currentStudentIndex;
            set
            {
                if (SetProperty(ref _currentStudentIndex, value))
                {
                    OnPropertyChanged(nameof(CurrentStudent));
                    OnPropertyChanged(nameof(CurrentStudentDisplay));
                    _ = LoadCurrentStudentSubmissionsAsync();
                }
            }
        }

        public int CurrentProblemIndex
        {
            get => _currentProblemIndex;
            set
            {
                if (SetProperty(ref _currentProblemIndex, value))
                {
                    OnPropertyChanged(nameof(CurrentProblem));
                    OnPropertyChanged(nameof(CurrentSubmission));
                    _ = LoadCurrentSubmissionCodeAsync();
                }
            }
        }

        public string SourceCode
        {
            get => _sourceCode;
            set => SetProperty(ref _sourceCode, value);
        }

        public double Score
        {
            get => _score;
            set => SetProperty(ref _score, value);
        }

        public string Feedback
        {
            get => _feedback;
            set => SetProperty(ref _feedback, value);
        }

        public ObservableCollection<AssignmentUser> Students { get; } = new();
        public ObservableCollection<Problem> Problems { get; } = new();
        public ObservableCollection<BestSubmission> BestSubmissions { get; } = new();
        public ObservableCollection<ProblemSubmissionItem> ProblemSubmissions { get; } = new();

        public AssignmentUser CurrentStudent => Students.ElementAtOrDefault(CurrentStudentIndex);
        public Problem CurrentProblem => Problems.ElementAtOrDefault(CurrentProblemIndex);
        public BestSubmission CurrentSubmission => BestSubmissions.FirstOrDefault(s => s.ProblemId == CurrentProblem?.ProblemId);

        public string CurrentStudentDisplay => $"Sinh viên {CurrentStudentIndex + 1}/{Students.Count}";
        public bool CanNavigatePrevious => CurrentStudentIndex > 0;
        public bool CanNavigateNext => CurrentStudentIndex < Students.Count - 1;

        public ICommand PreviousStudentCommand { get; }
        public ICommand NextStudentCommand { get; }
        public ICommand SaveGradeCommand { get; }
        public ICommand SelectProblemCommand { get; }

        public TeacherGradingViewModel(
            AssignmentService assignmentService,
            SubmissionService submissionService,
            ProblemService problemService,
            ClassService classService)
        {
            _assignmentService = assignmentService;
            _submissionService = submissionService;
            _problemService = problemService;
            _classService = classService;

            PreviousStudentCommand = new RelayCommand(_ => ExecutePreviousStudent(), _ => CanNavigatePrevious);
            NextStudentCommand = new RelayCommand(_ => ExecuteNextStudent(), _ => CanNavigateNext);
            SaveGradeCommand = new RelayCommand(async _ => await ExecuteSaveGradeAsync(), _ => CurrentSubmission != null);
            SelectProblemCommand = new RelayCommand(param => ExecuteSelectProblem(param));
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

                // Load class
                var classResponse = await _classService.GetClassByIdAsync(Assignment.ClassId);
                if (classResponse?.Success == true)
                {
                    ClassData = classResponse.Data;
                }

                // Load students
                var studentsResponse = await _assignmentService.GetAssignmentStudentsAsync(_assignmentId);
                if (studentsResponse?.Success == true && studentsResponse.Data != null)
                {
                    Students.Clear();
                    foreach (var student in studentsResponse.Data)
                    {
                        Students.Add(student);
                    }
                }

                // Load problems
                Problems.Clear();
                if (Assignment.Problems != null)
                {
                    foreach (var assignmentProblem in Assignment.Problems)
                    {
                        var problemResponse = await _problemService.GetProblemAsync(assignmentProblem.ProblemId);
                        if (problemResponse?.Success == true && problemResponse.Data != null)
                        {
                            Problems.Add(problemResponse.Data);
                        }
                    }
                }

                if (Students.Count > 0)
                {
                    CurrentStudentIndex = 0;
                }
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

        private async Task LoadCurrentStudentSubmissionsAsync()
        {
            if (CurrentStudent == null || Assignment == null) return;

            IsLoading = true;
            try
            {
                BestSubmissions.Clear();
                ProblemSubmissions.Clear();

                foreach (var problem in Problems)
                {
                    try
                    {
                        var response = await _submissionService.GetBestSubmissionsAsync(
                            Assignment.AssignmentId,
                            problem.ProblemId,
                            1,
                            100);

                        if (response?.Success == true && response.Data != null)
                        {
                            var studentSubmission = response.Data.FirstOrDefault(s => s.UserId == CurrentStudent.UserId);
                            if (studentSubmission != null)
                            {
                                BestSubmissions.Add(studentSubmission);

                                ProblemSubmissions.Add(new ProblemSubmissionItem
                                {
                                    ProblemId = problem.ProblemId,
                                    Title = problem.Title,
                                    PassedTestCases = studentSubmission.PassedTestCases ?? 0,
                                    TotalTestCases = studentSubmission.TotalTestCases ?? 0,
                                    Status = studentSubmission.Status,
                                    StatusColor = GetStatusColor(studentSubmission.Status)
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading submission for problem {problem.ProblemId}: {ex.Message}");
                    }
                }

                if (BestSubmissions.Count > 0)
                {
                    CurrentProblemIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Error = $"Lỗi tải bài làm: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCurrentSubmissionCodeAsync()
        {
            if (CurrentSubmission == null)
            {
                SourceCode = "// Sinh viên chưa nộp bài cho câu hỏi này";
                Score = 0;
                Feedback = string.Empty;
                return;
            }

            IsLoading = true;
            try
            {
                SourceCode = CurrentSubmission.SolutionCode ?? "// Source code không khả dụng";
                Score = CurrentSubmission.Score ?? 0;
                Feedback = CurrentSubmission.TeacherFeedback ?? string.Empty;
            }
            catch (Exception ex)
            {
                SourceCode = $"// Lỗi tải source code: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteSaveGradeAsync()
        {
            if (CurrentSubmission == null)
            {
                MessageBox.Show("Không tìm thấy bài làm để chấm điểm", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsSaving = true;
            Error = string.Empty;

            try
            {
                var request = new GradeSubmissionRequest
                {
                    Score = Score,
                    TeacherFeedback = string.IsNullOrWhiteSpace(Feedback) ? null : Feedback
                };

                var response = await _assignmentService.GradeSubmissionAsync(
                    Assignment.AssignmentId,
                    CurrentSubmission.SubmissionId,
                    request);

                if (response?.Success == true)
                {
                    MessageBox.Show(
                        $"Đã lưu điểm cho {CurrentStudent.User?.FullName ?? "sinh viên"}",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Reload submissions
                    await LoadCurrentStudentSubmissionsAsync();
                }
                else
                {
                    Error = response?.Message ?? "Không thể lưu điểm. API có thể chưa sẵn sàng.";
                    MessageBox.Show(Error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Error = $"Lỗi lưu điểm: {ex.Message}";
                MessageBox.Show(Error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void ExecutePreviousStudent()
        {
            if (CanNavigatePrevious)
            {
                CurrentStudentIndex--;
            }
        }

        private void ExecuteNextStudent()
        {
            if (CanNavigateNext)
            {
                CurrentStudentIndex++;
            }
        }

        private void ExecuteSelectProblem(object parameter)
        {
            if (parameter is int index)
            {
                CurrentProblemIndex = index;
            }
        }

        private string GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "passed" => "#28a745",
                "failed" => "#dc3545",
                _ => "#ffc107"
            };
        }
    }
}

