using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class AssignmentDetailViewModel : ViewModelBase
    {
        private readonly AssignmentService _assignmentService;
        private readonly AuthService _authService;
        private readonly ProblemService _problemService;
        private readonly SubmissionService _submissionService;
        private readonly NavigationService _navigationService;
        private Assignment _assignment;
        private AssignmentUser _assignmentUser;
        private bool _isLoading;
        private string _assignmentId;

        public AssignmentDetailViewModel(AssignmentService assignmentService, AuthService authService, ProblemService problemService, SubmissionService submissionService, NavigationService navigationService)
        {
            _assignmentService = assignmentService;
            _authService = authService;
            _problemService = problemService;
            _submissionService = submissionService;
            _navigationService = navigationService;
            _assignment = new Assignment();

            NavigateToProblemCommand = new RelayCommand<string>(NavigateToProblem);
            NavigateBackCommand = new RelayCommand(_ => NavigateBack());
            StartAssignmentCommand = new RelayCommand(_ => StartAssignment(), _ => _assignmentUser?.Status == AssignmentUserStatus.NOT_STARTED);
        }

        public Assignment Assignment
        {
            get => _assignment;
            set => SetProperty(ref _assignment, value);
        }

        public AssignmentUser AssignmentUser
        {
            get => _assignmentUser;
            set
            {
                SetProperty(ref _assignmentUser, value);
                ((RelayCommand)StartAssignmentCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ObservableCollection<Models.AssignmentProblemDetail> Problems { get; } = new();
        public ObservableCollection<BestSubmission> ProblemSubmissions { get; } = new();

        public ICommand NavigateToProblemCommand { get; }
        public ICommand NavigateBackCommand { get; }
        public ICommand StartAssignmentCommand { get; }

        public async Task InitializeAsync(string assignmentId)
        {
            _assignmentId = assignmentId;
            IsLoading = true;
            try
            {
                await LoadAssignmentDataAsync();
                await LoadBestSubmissionsAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadAssignmentDataAsync()
        {
            try
            {
                var currentUser = _authService.CurrentUser;
                if (currentUser?.Role.ToString().ToLower() == "student")
                {
                    var userResponse = await _assignmentService.GetMyAssignmentDetailAsync(_assignmentId);
                    if (userResponse?.Success == true && userResponse.Data != null)
                    {
                        AssignmentUser = userResponse.Data;
                    }

                    var assignmentResponse = await _assignmentService.GetAssignmentAsync(_assignmentId);
                    if (assignmentResponse?.Success == true && assignmentResponse.Data != null)
                    {
                        Assignment = assignmentResponse.Data;
                        Problems.Clear();
                        foreach (var problem in Assignment.Problems)
                        {
                            Problems.Add(problem);
                        }
                    }
                }
                else
                {
                    var assignmentResponse = await _assignmentService.GetAssignmentAsync(_assignmentId);
                    if (assignmentResponse?.Success == true && assignmentResponse.Data != null)
                    {
                        Assignment = assignmentResponse.Data;
                        Problems.Clear();
                        foreach (var problem in Assignment.Problems)
                        {
                            Problems.Add(problem);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Lỗi khi tải bài tập: {ex.Message}");
            }
        }

        private async Task LoadBestSubmissionsAsync()
        {
            try
            {
                var currentUser = _authService.CurrentUser;
                if (currentUser?.Role.ToString().ToLower() == "student" && Problems.Count > 0)
                {
                    var problemIds = new System.Collections.Generic.List<string>();
                    foreach (var problem in Problems)
                    {
                        problemIds.Add(problem.ProblemId);
                    }

                    var response = await _assignmentService.GetBestSubmissionsAsync(_assignmentId, problemIds);
                    if (response?.Success == true && response.Data != null)
                    {
                        ProblemSubmissions.Clear();
                        foreach (var submission in response.Data)
                        {
                            ProblemSubmissions.Add(submission);
                        }

                        // Update problem details with submission info
                        UpdateProblemDetails();

                        OnPropertyChanged(nameof(CompletedProblemsCount));
                        OnPropertyChanged(nameof(ProgressPercentage));
                        OnPropertyChanged(nameof(UserScore));
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading submissions: {ex.Message}");
            }
        }

        private void UpdateProblemDetails()
        {
            System.Diagnostics.Debug.WriteLine($"UpdateProblemDetails: Problems.Count = {Problems.Count}, Submissions.Count = {ProblemSubmissions.Count}");

            foreach (var problem in Problems)
            {
                var submission = GetBestSubmissionForProblem(problem.ProblemId);
                if (submission != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Problem {problem.Code}: Status={submission.Status}, Testcases={submission.PassedTestcase}/{submission.TotalTestcase}, Score={submission.Score}, Submissions={submission.TotalSubmission}");

                    problem.IsCompleted = submission.Status == "Passed";
                    problem.PassedTestcases = submission.PassedTestcase;
                    problem.TotalTestcases = submission.TotalTestcase;
                    problem.EarnedPoints = (int)submission.Score;
                    problem.SubmissionCount = submission.TotalSubmission;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Problem {problem.Code}: No submission found");

                    problem.IsCompleted = false;
                    problem.PassedTestcases = 0;
                    problem.TotalTestcases = 0;
                    problem.EarnedPoints = 0;
                    problem.SubmissionCount = 0;
                }
            }
        }

        private async void StartAssignment()
        {
            try
            {
                var response = await _assignmentService.StartAssignmentAsync(_assignmentId);
                if (response?.Success == true && response.Data != null)
                {
                    AssignmentUser = response.Data;
                    await GetMetroWindow()?.ShowMessageAsync("Thành công", "Bạn đã bắt đầu bài tập!");
                }
                else
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Không thể bắt đầu bài tập: {response?.Message}");
                }
            }
            catch (System.Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Lỗi khi bắt đầu bài tập: {ex.Message}");
            }
        }

        private async void NavigateToProblem(string problemId)
        {
            try
            {
                if (string.IsNullOrEmpty(problemId))
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Problem ID không hợp lệ");
                    return;
                }

                if (string.IsNullOrEmpty(_assignmentId))
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Assignment ID không hợp lệ");
                    return;
                }

                // Navigate to ProblemSolverPage
                var problemSolverPage = App.ServiceProvider.GetService(typeof(Views.Students.ProblemSolverPage)) as Views.Students.ProblemSolverPage;
                if (problemSolverPage == null)
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Không thể tạo ProblemSolverPage.");
                    return;
                }

                var viewModel = App.ServiceProvider.GetService(typeof(ProblemSolverViewModel)) as ProblemSolverViewModel;
                if (viewModel == null)
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Không thể tạo ProblemSolverViewModel.");
                    return;
                }

                problemSolverPage.DataContext = viewModel;
                await viewModel.InitializeAsync(_assignmentId, problemId);

                _navigationService.NavigateTo(problemSolverPage);
            }
            catch (System.Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Lỗi khi mở Problem Solver: {ex.Message}");
            }
        }

        private void NavigateBack()
        {
            _navigationService.GoBack();
        }

        public BestSubmission GetBestSubmissionForProblem(string problemId)
        {
            foreach (var submission in ProblemSubmissions)
            {
                if (submission.ProblemId == problemId)
                    return submission;
            }
            return null;
        }

        public int GetDaysUntilDue()
        {
            if (!Assignment.EndTime.HasValue) return 0;
            var now = System.DateTime.Now;
            var diff = Assignment.EndTime.Value - now;
            return diff.Days;
        }

        public int CompletedProblemsCount
        {
            get
            {
                int count = 0;
                foreach (var submission in ProblemSubmissions)
                {
                    if (submission.Status == "Passed")
                        count++;
                }
                return count;
            }
        }

        public double ProgressPercentage
        {
            get
            {
                if (Problems.Count == 0) return 0;
                return (double)CompletedProblemsCount / Problems.Count * 100;
            }
        }

        public double UserScore
        {
            get
            {
                double totalScore = 0;
                foreach (var submission in ProblemSubmissions)
                {
                    totalScore += submission.Score;
                }
                return totalScore;
            }
        }
    }
}

