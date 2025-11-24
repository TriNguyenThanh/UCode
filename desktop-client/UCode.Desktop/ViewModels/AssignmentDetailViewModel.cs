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

                // Note: ProblemSolverPage chưa được tạo, tạm thời giữ window cho problem solver
                // TODO: Chuyển sang ProblemSolverPage khi có yêu cầu
                var problemWindow = App.ServiceProvider.GetService(typeof(Views.Students.ProblemSolverWindow)) as Views.Students.ProblemSolverWindow;
                if (problemWindow == null)
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Không thể tạo ProblemSolverWindow. Vui lòng kiểm tra DI configuration.");
                    return;
                }

                var viewModel = problemWindow.DataContext as ProblemSolverViewModel;
                if (viewModel == null)
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", "ProblemSolverViewModel không tồn tại trong DataContext");
                    return;
                }

                await viewModel.InitializeAsync(_assignmentId, problemId);
                problemWindow.Show();
            }
            catch (System.Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Lỗi khi mở Problem Solver: {ex.Message}\n\nStack trace:\n{ex.StackTrace}");
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

