using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;
using GradeRequest = UCode.Desktop.Services.GradeSubmissionRequest;

namespace UCode.Desktop.ViewModels
{
    public class SubmissionDetailViewModel : ViewModelBase
    {
        private readonly AssignmentService _assignmentService;
        private readonly SubmissionService _submissionService;
        private readonly ProblemService _problemService;
        private readonly ClassService _classService;
        private readonly NavigationService _navigationService;

        private bool _isLoading;
        private bool _isSaving;
        private string _assignmentId = string.Empty;
        private string _problemId = string.Empty;
        private string _submissionId = string.Empty;
        private Assignment _assignment;
        private Class _classData;
        private Problem _problem;
        private Submission _submission;
        private string _sourceCode = string.Empty;
        private int _score;
        private string _feedback = string.Empty;
        private string _error = string.Empty;

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

        public Problem Problem
        {
            get => _problem;
            set => SetProperty(ref _problem, value);
        }

        public Submission Submission
        {
            get => _submission;
            set => SetProperty(ref _submission, value);
        }

        public string SourceCode
        {
            get => _sourceCode;
            set => SetProperty(ref _sourceCode, value);
        }

        public int Score
        {
            get => _score;
            set => SetProperty(ref _score, value);
        }

        public string Feedback
        {
            get => _feedback;
            set => SetProperty(ref _feedback, value);
        }

        // Additional properties for display
        public string StudentName => Submission?.UserFullName ?? "N/A";
        public string StatusColor => Submission?.Status switch
        {
            "Passed" => "#28a745",
            "Failed" => "#dc3545",
            "CompilationError" => "#dc3545",
            "RuntimeError" => "#dc3545",
            "TimeLimitExceeded" => "#ffc107",
            "MemoryLimitExceeded" => "#ffc107",
            _ => "#6c757d"
        };

        public bool IsPassed => Submission?.PassedTestcase == Submission?.TotalTestcase;

        public ICommand BackCommand { get; }
        public ICommand SaveGradeCommand { get; }

        public SubmissionDetailViewModel(
            AssignmentService assignmentService,
            SubmissionService submissionService,
            ProblemService problemService,
            ClassService classService,
            NavigationService navigationService)
        {
            _assignmentService = assignmentService;
            _submissionService = submissionService;
            _problemService = problemService;
            _classService = classService;
            _navigationService = navigationService;

            BackCommand = new RelayCommand(_ => ExecuteBack());
            SaveGradeCommand = new RelayCommand(async _ => await ExecuteSaveGrade(), _ => Submission != null);
        }

        public async Task InitializeAsync(string assignmentId, string problemId, string submissionId)
        {
            _assignmentId = assignmentId;
            _problemId = problemId;
            _submissionId = submissionId;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            Error = string.Empty;

            try
            {
                // Fetch all data in parallel
                var assignmentTask = _assignmentService.GetAssignmentAsync(_assignmentId);
                var problemTask = _problemService.GetProblemAsync(_problemId);
                var submissionTask = _submissionService.GetSubmissionAsync(_submissionId);

                await Task.WhenAll(assignmentTask, problemTask, submissionTask);

                var assignmentResponse = await assignmentTask;
                var problemResponse = await problemTask;
                var submissionResponse = await submissionTask;

                if (assignmentResponse?.Success == true && assignmentResponse.Data != null)
                {
                    Assignment = assignmentResponse.Data;

                    // Load class data
                    var classResponse = await _classService.GetClassByIdAsync(Assignment.ClassId);
                    if (classResponse?.Success == true)
                    {
                        ClassData = classResponse.Data;
                    }
                }

                if (problemResponse?.Success == true && problemResponse.Data != null)
                {
                    Problem = problemResponse.Data;
                }

                if (submissionResponse?.Success == true && submissionResponse.Data != null)
                {
                    Submission = submissionResponse.Data;
                    SourceCode = Submission.SourceCode ?? "// Source code không khả dụng";
                    Score = (int)Submission.Score;          
                    Feedback = Submission.Comment ?? string.Empty;

                    OnPropertyChanged(nameof(StudentName));
                    OnPropertyChanged(nameof(StatusColor));
                    OnPropertyChanged(nameof(IsPassed));
                }
                else
                {
                    Error = submissionResponse?.Message ?? "Không thể tải dữ liệu bài nộp";
                }
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

        private async Task ExecuteSaveGrade()
        {
            if (Submission == null || Assignment == null)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Không tìm thấy bài làm để chấm điểm");
                return;
            }

            IsSaving = true;
            Error = string.Empty;

            try
            {
                var request = new GradeRequest
                {
                    SubmissionId = Submission.SubmissionId,
                    NewScore = Score,
                    Comment = string.IsNullOrWhiteSpace(Feedback) ? string.Empty : Feedback
                };

                var response = await _assignmentService.GradeSubmissionAsync(
                    Assignment.AssignmentId,
                    Submission.SubmissionId,
                    request);

                if (response?.Success == true)
                {
                    await GetMetroWindow()?.ShowMessageAsync(
                        "Thành công",
                        $"Đã lưu điểm cho sinh viên");

                    // Reload submission
                    await LoadDataAsync();
                }
                else
                {
                    Error = response?.Message ?? "Không thể lưu điểm";
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", Error);
                }
            }
            catch (Exception ex)
            {
                Error = $"Lỗi lưu điểm: {ex.Message}";
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void ExecuteBack()
        {
            _navigationService.GoBack();
        }
    }
}
