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
        private System.Windows.Threading.DispatcherTimer _countdownTimer;
        private string _timeRemaining = "Đang tải...";

        public string AssignmentId => _assignmentId; // Expose for page to check

        public string TimeRemaining
        {
            get => _timeRemaining;
            set => SetProperty(ref _timeRemaining, value);
        }

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
            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
            StartAssignmentCommand = new RelayCommand(_ => StartAssignment(), _ => _assignmentUser?.Status == AssignmentUserStatus.NOT_STARTED);
            ShowAIDetectionDetailsCommand = new RelayCommand(_ => ShowAIDetectionDetails());

            // Initialize countdown timer
            _countdownTimer = new System.Windows.Threading.DispatcherTimer();
            _countdownTimer.Interval = System.TimeSpan.FromSeconds(1);
            _countdownTimer.Tick += (s, e) => UpdateTimeRemaining();
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
                OnPropertyChanged(nameof(UserScore));
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
        public ICommand RefreshCommand { get; }
        public ICommand StartAssignmentCommand { get; }
        public ICommand ShowAIDetectionDetailsCommand { get; }

        public async Task InitializeAsync(string assignmentId)
        {
            _assignmentId = assignmentId;
            IsLoading = true;
            try
            {
                await LoadAssignmentDataAsync();
                await LoadBestSubmissionsAsync();
                UpdateTimeRemaining();
                _countdownTimer.Start();
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
                    // 1. Try GetMyAssignmentDetailAsync
                    var userResponse = await _assignmentService.GetMyAssignmentDetailAsync(_assignmentId);
                    if (userResponse?.Success == true && userResponse.Data != null)
                    {
                        AssignmentUser = userResponse.Data;
                        
                        // If Assignment is included in response, use it
                        if (AssignmentUser.Assignment != null)
                        {
                            Assignment = AssignmentUser.Assignment;
                        }
                    }

                    // 2. If Assignment is still empty, try GetAssignmentAsync
                    if (Assignment == null || string.IsNullOrEmpty(Assignment.AssignmentId))
                    {
                        var assignmentResponse = await _assignmentService.GetAssignmentAsync(_assignmentId);
                        if (assignmentResponse?.Success == true && assignmentResponse.Data != null)
                        {
                            Assignment = assignmentResponse.Data;
                        }
                        else
                        {
                            // 3. Fallback to GetStudentAssignmentsAsync list
                             var listResponse = await _assignmentService.GetStudentAssignmentsAsync();
                             if (listResponse?.Success == true && listResponse.Data != null)
                             {
                                 var found = listResponse.Data.FirstOrDefault(a => a.AssignmentId == _assignmentId);
                                 if (found != null)
                                 {
                                     Assignment = found;
                                 }
                             }
                        }
                    }

                    // Populate problems if Assignment is loaded
                    if (Assignment != null && Assignment.Problems != null)
                    {
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
                        if (Assignment.Problems != null)
                        {
                            foreach (var problem in Assignment.Problems)
                            {
                                Problems.Add(problem);
                            }
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

                    problem.IsCompleted = true;
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
                var problemPage = App.ServiceProvider.GetService(typeof(Pages.ProblemSolverPage)) as Pages.ProblemSolverPage;
                if (problemPage == null)
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Không thể tạo ProblemSolverPage. Vui lòng kiểm tra DI configuration.");
                    return;
                }

                var viewModel = problemPage.DataContext as ProblemSolverViewModel;
                if (viewModel == null)
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Không thể tạo ProblemSolverViewModel.");
                    return;
                }

                await viewModel.InitializeAsync(_assignmentId, problemId);
                _navigationService.NavigateTo(problemPage);
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

        private async Task RefreshAsync()
        {
            if (string.IsNullOrEmpty(_assignmentId)) return;
            
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

        public async Task RefreshDataAsync()
        {
            await RefreshAsync();
        }

        private void UpdateTimeRemaining()
        {
            if (Assignment == null || !Assignment.EndTime.HasValue)
            {
                TimeRemaining = "Không giới hạn";
                return;
            }

            var now = System.DateTime.Now;
            var endTime = Assignment.EndTime.Value;
            var timeLeft = endTime - now;

            if (timeLeft.TotalSeconds <= 0)
            {
                TimeRemaining = "Đã hết hạn";
                _countdownTimer.Stop();
                return;
            }

            // Format: "X ngày Y giờ Z phút" or "Y giờ Z phút" or "Z phút W giây"
            if (timeLeft.TotalDays >= 1)
            {
                TimeRemaining = $"{(int)timeLeft.TotalDays} ngày {timeLeft.Hours} giờ {timeLeft.Minutes} phút";
            }
            else if (timeLeft.TotalHours >= 1)
            {
                TimeRemaining = $"{timeLeft.Hours} giờ {timeLeft.Minutes} phút {timeLeft.Seconds} giây";
            }
            else if (timeLeft.TotalMinutes >= 1)
            {
                TimeRemaining = $"{timeLeft.Minutes} phút {timeLeft.Seconds} giây";
            }
            else
            {
                TimeRemaining = $"{timeLeft.Seconds} giây";
            }
        }

        private async void ShowAIDetectionDetails()
        {
            if (AssignmentUser == null || AssignmentUser.CapturedAICount == 0)
            {
                await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Không có dữ liệu truy cập AI.");
                return;
            }

            var message = $"Thông tin AI Detection\n\n";
            message += $"Tổng số lần truy cập AI: {AssignmentUser.CapturedAICount}\n";
            message += $"Số lần chuyển tab: {AssignmentUser.TabSwitchCount}\n\n";

            if (!string.IsNullOrEmpty(AssignmentUser.AiDetectionDetails))
            {
                try
                {
                    // Parse JSON string to dictionary
                    var aiDetails = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, int>>(AssignmentUser.AiDetectionDetails);

                    if (aiDetails != null && aiDetails.Count > 0)
                    {
                        message += "Chi tiết theo công cụ AI:\n";
                        message += new string('─', 40) + "\n";
                        foreach (var detail in aiDetails)
                        {
                            message += $"• {detail.Key}: {detail.Value} requests\n";
                        }
                    }
                }
                catch
                {
                    // If JSON parsing fails, show raw data
                    message += $"Chi tiết:\n{AssignmentUser.AiDetectionDetails}";
                }
            }

            await GetMetroWindow()?.ShowMessageAsync("Chi tiết truy cập AI", message);
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
                    // if (submission.Status == "Passed")
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
                if (AssignmentUser != null && AssignmentUser.Score.HasValue)
                {
                    return AssignmentUser.Score.Value;
                }

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

