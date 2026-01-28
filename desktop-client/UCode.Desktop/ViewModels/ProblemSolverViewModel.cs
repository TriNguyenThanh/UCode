using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class ProblemSolverViewModel : ViewModelBase
    {
        private readonly ProblemService _problemService;
        private readonly SubmissionService _submissionService;
        private readonly AssignmentService _assignmentService;
        private readonly AuthService _authService;
        private readonly NavigationService _navigationService;
        private Problem _problem;
        private bool _isLoading;
        private string _assignmentId;
        private string _problemId;
        private string _code;
        private ProblemLanguage _selectedLanguage;
        private string _output;
        private bool _isRunning;
        private bool _isSubmitting;
        private bool _hasRunSuccessfully;
        private string _lastRunCode;
        private CodeEditorHelper _editorHelper;
        private bool _isAssignmentClosed;

        public ProblemSolverViewModel(ProblemService problemService, SubmissionService submissionService, AuthService authService, NavigationService navigationService, AssignmentService assignmentService)
        {
            _problemService = problemService;
            _submissionService = submissionService;
            _authService = authService;
            _navigationService = navigationService;
            _assignmentService = assignmentService;
            _problem = new Problem();
            _code = "// Your code here";
            _output = string.Empty;

            NavigateBackCommand = new RelayCommand(_ => NavigateBack());
            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
            RunCodeCommand = new RelayCommand(_ => RunCode());
            SubmitCodeCommand = new RelayCommand(_ => SubmitCode());
            ResetCodeCommand = new RelayCommand(_ => ResetCode());
            ViewSubmissionDetailCommand = new RelayCommand<Submission>(submission => ViewSubmissionDetail(submission));
            ViewTestCaseResultsCommand = new RelayCommand<Submission>(submission => ViewTestCaseResults(submission));
        }

        /// <summary>
        /// Editor helper for managing read-only regions
        /// </summary>
        public CodeEditorHelper EditorHelper
        {
            get => _editorHelper;
            set
            {
                _editorHelper = value;
                // Apply current template if language is already selected
                if (SelectedLanguage != null)
                {
                    ApplyCodeTemplate();
                }
            }
        }

        public Problem Problem
        {
            get => _problem;
            set => SetProperty(ref _problem, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsAssignmentClosed
        {
            get => _isAssignmentClosed;
            set
            {
                if (SetProperty(ref _isAssignmentClosed, value))
                {
                    OnPropertyChanged(nameof(CanSubmit));
                }
            }
        }

        /// <summary>
        /// Returns true if the assignment is not closed and user can submit
        /// </summary>
        public bool CanSubmit => !IsAssignmentClosed;

        public string Code
        {
            get => _code;
            set
            {
                if (SetProperty(ref _code, value))
                {
                    OnPropertyChanged(nameof(FullCode));
                    ((RelayCommand)RunCodeCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)SubmitCodeCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string _headCode;
        public string HeadCode
        {
            get => _headCode;
            set
            {
                if (SetProperty(ref _headCode, value))
                {
                    OnPropertyChanged(nameof(FullCode));
                }
            }
        }

        private string _tailCode;
        public string TailCode
        {
            get => _tailCode;
            set
            {
                if (SetProperty(ref _tailCode, value))
                {
                    OnPropertyChanged(nameof(FullCode));
                }
            }
        }

        public string FullCode
        {
            get
            {
                // If EditorHelper is available, get code from it (includes any user edits)
                if (_editorHelper != null)
                {
                    return _editorHelper.GetFullCode();
                }

                // Fallback to manual concatenation
                var sb = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(HeadCode))
                {
                    sb.AppendLine(HeadCode);
                }
                sb.Append(Code);
                if (!string.IsNullOrEmpty(TailCode))
                {
                    sb.AppendLine();
                    sb.Append(TailCode);
                }
                return sb.ToString();
            }
            set
            {
                // Simple logic to extract body: remove Head from start and Tail from end
                var newBody = value;

                if (!string.IsNullOrEmpty(HeadCode))
                {
                    var headTrimmed = HeadCode.TrimEnd('\r', '\n');
                    if (newBody.StartsWith(headTrimmed))
                    {
                        // Try to find where Head ends
                        // This is tricky if user edits the boundary. 
                        // For now, we assume user respects the boundary or we accept the mess.
                        // Better approach: Don't rely on string matching if we want to enforce read-only.
                        // But for "seamless copy", this is the trade-off.
                    }
                }

                // Actually, for the requirement "Head and Tail are read-only", 
                // doing this in ViewModel setter is too late and clunky.
                // But let's at least allow binding to FullCode.

                // If we bind to FullCode, we need to update Code.
                // Let's try a simpler approach: Just update Code.
                // We will rely on the View to prevent editing Head/Tail if possible, 
                // or just let it be and fix it on Run/Submit.

                // However, the user specifically asked for "Head and Tail read-only".
                // So we will implement the restriction in the View (Code-behind).
                // Here we just need to update Code correctly.

                string currentHead = HeadCode ?? string.Empty;
                string currentTail = TailCode ?? string.Empty;

                // Normalize line endings for comparison could be needed, but let's try direct.
                if (newBody.StartsWith(currentHead) && newBody.EndsWith(currentTail))
                {
                    var bodyLength = newBody.Length - currentHead.Length - currentTail.Length;
                    if (bodyLength >= 0)
                    {
                        var extractedBody = newBody.Substring(currentHead.Length, bodyLength);
                        // Trim leading/trailing newlines that might have been added by the join
                        Code = extractedBody.Trim('\r', '\n');
                    }
                }
                else
                {
                    // If structure is broken, we might just treat everything as Code 
                    // or try to salvage. For now, let's just update Code with whatever is not Head/Tail
                    // This is imperfect.
                    Code = value;
                }
            }
        }

        public ProblemLanguage SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                SetProperty(ref _selectedLanguage, value);
                // Update code template when language changes
                if (value != null)
                {
                    HeadCode = value.Head;
                    TailCode = value.Tail;
                    Code = GetCodeTemplate(value);
                    Output = string.Empty;
                    HasRunSuccessfully = false;
                    LastRunCode = string.Empty;

                    // Apply template to editor with read-only regions
                    ApplyCodeTemplate();
                }
            }
        }

        public ObservableCollection<ProblemLanguage> AvailableLanguages { get; } = new();

        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public bool IsSubmitting
        {
            get => _isSubmitting;
            set => SetProperty(ref _isSubmitting, value);
        }

        public bool HasRunSuccessfully
        {
            get => _hasRunSuccessfully;
            set => SetProperty(ref _hasRunSuccessfully, value);
        }

        public string LastRunCode
        {
            get => _lastRunCode;
            set => SetProperty(ref _lastRunCode, value);
        }

        public ObservableCollection<TestCase> SampleTestCases { get; } = new();

        public ObservableCollection<Submission> Submissions { get; } = new();

        public ICommand NavigateBackCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand RunCodeCommand { get; }
        public ICommand SubmitCodeCommand { get; }
        public ICommand ResetCodeCommand { get; }
        public ICommand ViewSubmissionDetailCommand { get; }
        public ICommand ViewTestCaseResultsCommand { get; }

        public async Task InitializeAsync(string assignmentId, string problemId)
        {
            _assignmentId = assignmentId;
            _problemId = problemId;
            IsLoading = true;
            IsAssignmentClosed = false; // Reset state
            try
            {
                // Check assignment status
                if (!string.IsNullOrEmpty(_assignmentId) && _assignmentService != null)
                {
                    var assignmentResponse = await _assignmentService.GetAssignmentAsync(_assignmentId);
                    if (assignmentResponse?.Success == true && assignmentResponse.Data != null)
                    {
                        IsAssignmentClosed = assignmentResponse.Data.Status == AssignmentStatus.CLOSED;
                    }
                }

                await LoadProblemDataAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadProblemDataAsync()
        {
            try
            {
                var currentUser = _authService.CurrentUser;
                ApiResponse<Problem> response;

                if (currentUser?.Role.ToString().ToLower() == "student")
                {
                    response = await _problemService.GetProblemForStudentAsync(_problemId);
                }
                else
                {
                    response = await _problemService.GetProblemAsync(_problemId);
                }

                if (response?.Success == true && response.Data != null)
                {
                    Problem = response.Data;

                    // Load available languages
                    AvailableLanguages.Clear();
                    if (Problem.ProblemLanguages != null)
                    {
                        foreach (var lang in Problem.ProblemLanguages)
                        {
                            AvailableLanguages.Add(lang);
                        }
                        // Set default language
                        if (AvailableLanguages.Count > 0)
                        {
                            SelectedLanguage = AvailableLanguages[0];
                        }
                    }

                    // Load sample test cases
                    SampleTestCases.Clear();
                    if (Problem.DatasetSample?.TestCases != null)
                    {
                        foreach (var testCase in Problem.DatasetSample.TestCases)
                        {
                            SampleTestCases.Add(testCase);
                        }
                    }

                    // Load submissions
                    await LoadSubmissionsAsync();
                }
                else
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Không thể tải đề bài: {response?.Message}");
                }
            }
            catch (System.Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Lỗi khi tải đề bài: {ex.Message}");
            }
        }

        private async Task LoadSubmissionsAsync()
        {
            try
            {
                var currentUser = _authService.CurrentUser;
                if (currentUser?.Role.ToString().ToLower() == "student")
                {
                    var response = await _submissionService.GetSubmissionsByProblemAsync(_problemId);
                    if (response?.Success == true && response.Data != null)
                    {
                        Submissions.Clear();
                        // Sort by SubmittedAt descending (newest first)
                        var sortedSubmissions = response.Data.OrderByDescending(s => s.SubmittedAt);
                        foreach (var submission in sortedSubmissions)
                        {
                            Submissions.Add(submission);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading submissions: {ex.Message}");
            }
        }

        private string GetCodeTemplate(ProblemLanguage language)
        {
            if (language == null) return "// Your code here";
            return language.Body ?? "// Your code here";
        }

        /// <summary>
        /// Applies the code template to the editor with read-only head and tail
        /// </summary>
        private void ApplyCodeTemplate()
        {
            if (_editorHelper != null && SelectedLanguage != null)
            {
                var head = HeadCode ?? string.Empty;
                var body = Code ?? string.Empty;
                var tail = TailCode ?? string.Empty;

                _editorHelper.SetCodeTemplate(head, body, tail);

                // Update syntax highlighting based on language
                _editorHelper.SetSyntaxHighlighting(SelectedLanguage.LanguageCode);
            }
        }

        private async void RunCode()
        {
            if (IsRunning || IsSubmitting) return;

            if (string.IsNullOrEmpty(Code))
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Vui lòng nhập code");
                return;
            }

            IsRunning = true;
            Output = "Đang chạy thử...";

            try
            {
                var request = new RunCodeRequest
                {
                    ProblemId = _problemId,
                    LanguageId = SelectedLanguage.LanguageId,
                    SourceCode = FullCode,
                    AssignmentId = _assignmentId
                };

                var response = await _submissionService.RunCodeAsync(request);
                if (response?.Success == true && response.Data != null)
                {
                    var submissionId = response.Data.SubmissionId;
                    var status = response.Data.Status;

                    // Check if submission failed immediately
                    if (status == "Failed" || status == "CompilationError" || status == "Error")
                    {
                        var errorMsg = "❌ Lỗi khi chạy thử:\n\n";
                        errorMsg += $"Trạng thái: {status}\n";
                        if (!string.IsNullOrEmpty(response.Data.ErrorMessage))
                        {
                            errorMsg += $"\nChi tiết lỗi:\n{response.Data.ErrorMessage}";
                        }
                        if (!string.IsNullOrEmpty(response.Message))
                        {
                            errorMsg += $"\n\n{response.Message}";
                        }
                        Output = errorMsg;
                        return;
                    }

                    await PollSubmissionResult(submissionId, false);
                }
                else
                {
                    var errorMsg = $"❌ Lỗi khi chạy thử: {response?.Message ?? "Không rõ lỗi"}";
                    if (response?.Data?.ErrorMessage != null)
                    {
                        errorMsg += $"\n\nChi tiết: {response.Data.ErrorMessage}";
                    }
                    Output = errorMsg;
                }
            }
            catch (System.Exception ex)
            {
                Output = $"Lỗi khi chạy thử: {ex.Message}";
            }
            finally
            {
                IsRunning = false;
            }
        }

        private async void SubmitCode()
        {
            if (IsRunning || IsSubmitting) return;

            if (string.IsNullOrEmpty(Code))
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Vui lòng nhập code");
                return;
            }

            IsSubmitting = true;
            Output = "Đang nộp bài...";

            try
            {
                var request = new SubmitCodeRequest
                {
                    ProblemId = _problemId,
                    LanguageId = SelectedLanguage.LanguageId,
                    SourceCode = FullCode,
                    AssignmentId = _assignmentId
                };

                var response = await _submissionService.SubmitCodeAsync(request);
                if (response?.Success == true && response.Data != null)
                {
                    var submissionId = response.Data.SubmissionId;
                    var status = response.Data.Status;

                    // Check if submission failed immediately
                    if (status == "Failed" || status == "CompilationError" || status == "Error")
                    {
                        var errorMsg = "❌ Lỗi khi nộp bài:\n\n";
                        errorMsg += $"Trạng thái: {status}\n";
                        if (!string.IsNullOrEmpty(response.Data.ErrorMessage))
                        {
                            errorMsg += $"\nChi tiết lỗi:\n{response.Data.ErrorMessage}";
                        }
                        if (!string.IsNullOrEmpty(response.Message))
                        {
                            errorMsg += $"\n\n{response.Message}";
                        }
                        Output = errorMsg;

                        // Still reload submissions to show the failed submission in history
                        await LoadSubmissionsAsync();
                        return;
                    }

                    await PollSubmissionResult(submissionId, true);
                }
                else
                {
                    var errorMsg = $"❌ Lỗi khi nộp bài: {response?.Message ?? "Không rõ lỗi"}";
                    if (response?.Data?.ErrorMessage != null)
                    {
                        errorMsg += $"\n\nChi tiết: {response.Data.ErrorMessage}";
                    }
                    Output = errorMsg;
                }
            }
            catch (System.Exception ex)
            {
                Output = $"Lỗi khi nộp bài: {ex.Message}";
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        private async void ResetCode()
        {
            var result = await GetMetroWindow()?.ShowMessageAsync("Xác nhận", "Bạn có chắc chắn muốn reset code về trạng thái ban đầu?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                Code = GetCodeTemplate(SelectedLanguage);
                Output = string.Empty;
                HasRunSuccessfully = false;
                LastRunCode = string.Empty;
            }
        }

        private async Task PollSubmissionResult(string submissionId, bool isSubmit)
        {
            int maxAttempts = 20;
            int attempts = 0;
            int delay = 1000;

            while (attempts < maxAttempts)
            {
                await Task.Delay(delay);
                attempts++;

                try
                {
                    var response = await _submissionService.GetSubmissionAsync(submissionId);
                    if (response?.Success == true && response.Data != null)
                    {
                        var submission = response.Data;
                        if (submission.Status == "Pending" || submission.Status == "Running" || submission.Status == "InQueue")
                        {
                            if (!isSubmit)
                            {
                                var lines = Output.Split('\n');
                                if (lines.Length > 0 && lines[lines.Length - 1].StartsWith("Đang xử lý"))
                                {
                                    lines[lines.Length - 1] = $"Đang xử lý... ({attempts}s)";
                                    Output = string.Join("\n", lines);
                                }
                                else
                                {
                                    Output += $"\nĐang xử lý... ({attempts}s)";
                                }
                            }
                            else
                            {
                                var lines = Output.Split('\n');
                                if (lines.Length > 0 && lines[lines.Length - 1].StartsWith("Đang xử lý"))
                                {
                                    lines[lines.Length - 1] = $"Đang xử lý... ({attempts}s)";
                                    Output = string.Join("\n", lines);
                                }
                            }
                            continue;
                        }

                        // Completed
                        var resultText = isSubmit ? "🎉 Kết quả nộp bài:\n\n" : "✅ Kết quả chạy thử:\n\n";
                        resultText += $"Status: {submission.Status}\n";
                        resultText += $"Thời gian: {submission.TotalTime}ms\n";
                        resultText += $"Bộ nhớ: {submission.TotalMemory}KB\n";

                        if (submission.Status == "Passed")
                        {
                            resultText += $"\n✅ {submission.PassedTestcase}/{submission.TotalTestcase} test cases passed";
                            if (!isSubmit)
                            {
                                HasRunSuccessfully = true;
                                LastRunCode = Code;
                            }
                        }
                        else
                        {
                            resultText += $"\n❌ {submission.PassedTestcase}/{submission.TotalTestcase} test cases passed";
                            if (!string.IsNullOrEmpty(submission.ErrorMessage))
                            {
                                resultText += $"\n\nLỗi: {submission.ErrorMessage}";
                            }
                            if (!isSubmit)
                            {
                                HasRunSuccessfully = false;
                                LastRunCode = string.Empty;
                            }
                        }

                        if (!string.IsNullOrEmpty(submission.CompareResult))
                        {
                            resultText += ParseTestCaseResults(submission.CompareResult);
                        }

                        Output = resultText;

                        if (isSubmit)
                        {
                            await LoadSubmissionsAsync();
                        }

                        break;
                    }
                }
                catch (System.Exception ex)
                {
                    Output += $"\n\n❌ Lỗi khi lấy kết quả: {ex.Message}";
                    break;
                }
            }

            if (attempts >= maxAttempts)
            {
                Output += "\n\n⏱️ Timeout: Quá trình chấm điểm mất nhiều thời gian. Vui lòng kiểm tra lại sau.";
            }
        }

        private string ParseTestCaseResults(string compareResult)
        {
            var result = "\n\n📋 Chi tiết từng test case:\n";
            result += new string('─', 40) + "\n";

            for (int i = 0; i < compareResult.Length; i++)
            {
                string status = compareResult[i] switch
                {
                    '0' => "✅ Passed",
                    '1' => "⏰ Time Limit Exceeded",
                    '2' => "💾 Memory Limit Exceeded",
                    '3' => "💥 Runtime Error",
                    '4' => "⚠️ Internal Error",
                    '5' => "❌ Wrong Answer",
                    '6' => "🔧 Compilation Error",
                    '7' => "⏭️ Skipped",
                    _ => "❓ Unknown"
                };
                result += $"Test case #{i + 1}: {status}\n";
            }

            return result;
        }

        private void NavigateBack()
        {
            _navigationService?.GoBack();
        }

        private async Task RefreshAsync()
        {
            if (string.IsNullOrEmpty(_problemId)) return;

            IsLoading = true;
            try
            {
                await LoadProblemDataAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ViewSubmissionDetail(Submission submission)
        {
            if (submission == null) return;

            var dialog = new Views.Students.SubmissionDetailDialog(submission);
            dialog.Owner = GetActiveWindow();
            dialog.ShowDialog();
        }

        private void ViewTestCaseResults(Submission submission)
        {
            if (submission == null) return;

            var dialog = new Views.Students.TestCaseResultDialog(submission);
            dialog.Owner = GetActiveWindow();
            dialog.ShowDialog();
        }

        /// <summary>
        /// Gets the currently active window to use as dialog owner
        /// </summary>
        private System.Windows.Window GetActiveWindow()
        {
            // Try to find the active window from application windows
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window.IsActive)
                    return window;
            }

            // Fallback to MainWindow
            return System.Windows.Application.Current.MainWindow;
        }
    }
}
