using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
        private readonly AuthService _authService;
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

        public ProblemSolverViewModel(ProblemService problemService, SubmissionService submissionService, AuthService authService)
        {
            _problemService = problemService;
            _submissionService = submissionService;
            _authService = authService;
            _problem = new Problem();
            _code = "// Your code here";
            _output = string.Empty;

            NavigateBackCommand = new RelayCommand(_ => NavigateBack());
            RunCodeCommand = new RelayCommand(_ => RunCode());
            SubmitCodeCommand = new RelayCommand(_ => SubmitCode());
            ResetCodeCommand = new RelayCommand(_ => ResetCode());
            ViewSubmissionDetailCommand = new RelayCommand<Submission>(submission => ViewSubmissionDetail(submission));
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

        public string Code
        {
            get => _code;
            set
            {
                SetProperty(ref _code, value);
                ((RelayCommand)RunCodeCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SubmitCodeCommand).RaiseCanExecuteChanged();
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
                    Code = GetCodeTemplate(value);
                    Output = string.Empty;
                    HasRunSuccessfully = false;
                    LastRunCode = string.Empty;
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
        public ICommand RunCodeCommand { get; }
        public ICommand SubmitCodeCommand { get; }
        public ICommand ResetCodeCommand { get; }
        public ICommand ViewSubmissionDetailCommand { get; }

        public async Task InitializeAsync(string assignmentId, string problemId)
        {
            _assignmentId = assignmentId;
            _problemId = problemId;
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
                        Console.WriteLine($"[ProblemSolver] Found {Problem.DatasetSample.TestCases.Count} sample test cases.");
                        foreach (var testCase in Problem.DatasetSample.TestCases)
                        {
                            Console.WriteLine($"[ProblemSolver] TestCase: InputRef={testCase.InputRef}, OutputRef={testCase.OutputRef}");
                            SampleTestCases.Add(testCase);
                        }
                    }
                    else
                    {
                        Console.WriteLine("[ProblemSolver] No sample test cases found (DatasetSample or TestCases is null).");
                        if (Problem.DatasetSample == null) Console.WriteLine("[ProblemSolver] DatasetSample is NULL");
                        else if (Problem.DatasetSample.TestCases == null) Console.WriteLine("[ProblemSolver] DatasetSample.TestCases is NULL");
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
                var response = await _submissionService.GetSubmissionsByProblemAsync(_problemId, 1, 10);
                if (response?.Success == true && response.Data != null)
                {
                    Submissions.Clear();
                    foreach (var submission in response.Data)
                    {
                        Submissions.Add(submission);
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

            var parts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrEmpty(language.Head)) parts.Add(language.Head);
            if (!string.IsNullOrEmpty(language.Body)) parts.Add(language.Body);
            if (!string.IsNullOrEmpty(language.Tail)) parts.Add(language.Tail);

            return parts.Count > 0 ? string.Join("\n\n", parts) : "// Your code here";
        }

        private void ResetCode()
        {
            if (SelectedLanguage != null)
            {
                Code = GetCodeTemplate(SelectedLanguage);
                Output = string.Empty;
                HasRunSuccessfully = false;
                LastRunCode = string.Empty;
            }
        }

        private async void RunCode()
        {
            if (SelectedLanguage == null)
            {
                Output = "❌ Vui lòng chọn ngôn ngữ lập trình";
                return;
            }

            if (string.IsNullOrWhiteSpace(Code))
            {
                Output = "❌ Vui lòng nhập code";
                return;
            }

            // Reset validation if code changed
            if (Code != LastRunCode)
            {
                HasRunSuccessfully = false;
            }

            IsRunning = true;
            Output = "⏳ Đang biên dịch và chạy code...\n";

            try
            {
                var response = await _submissionService.RunCodeAsync(new RunCodeRequest
                {
                    ProblemId = _problemId,
                    LanguageId = SelectedLanguage.LanguageId,
                    SourceCode = Code,
                    AssignmentId = _assignmentId
                });

                if (response?.Success == true && response.Data != null)
                {
                    Output = $"✅ Đã gửi code để chạy thử!\n\nSubmission ID: {response.Data.SubmissionId}\nStatus: {response.Data.Status}\n\nĐang xử lý... (0s)";
                    await PollSubmissionResult(response.Data.SubmissionId, Code, false);
                }
                else
                {
                    var errorMsg = response?.Message ?? "Không thể chạy code";
                    var errorDetails = response != null ? $"\n\nChi tiết:\nSuccess: {response.Success}\nMessage: {response.Message}" : "";
                    Output = $"❌ Lỗi khi chạy code:\n{errorMsg}{errorDetails}";
                    HasRunSuccessfully = false;
                    LastRunCode = string.Empty;
                    System.Diagnostics.Debug.WriteLine($"RunCode failed: {errorMsg}");
                }
            }
            catch (System.Exception ex)
            {
                var detailedError = $"❌ Lỗi Exception:\n{ex.Message}\n\nType: {ex.GetType().Name}";
                if (ex.InnerException != null)
                {
                    detailedError += $"\n\nInner Exception:\n{ex.InnerException.Message}";
                }
                Output = detailedError;
                HasRunSuccessfully = false;
                LastRunCode = string.Empty;
                System.Diagnostics.Debug.WriteLine($"RunCode exception: {ex}");
            }
            finally
            {
                IsRunning = false;
            }
        }

        private async void SubmitCode()
        {
            if (SelectedLanguage == null)
            {
                Output = "❌ Vui lòng chọn ngôn ngữ lập trình";
                return;
            }

            if (string.IsNullOrWhiteSpace(Code))
            {
                Output = "❌ Vui lòng nhập code";
                return;
            }

            if (!HasRunSuccessfully)
            {
                Output = "❌ Vui lòng chạy thử code thành công trước khi nộp bài!";
                return;
            }

            if (Code != LastRunCode)
            {
                Output = "⚠️ Code đã thay đổi sau lần chạy thử cuối!\n\nVui lòng chạy thử lại trước khi nộp bài.";
                return;
            }

            IsSubmitting = true;
            Output = "📤 Đang nộp bài...\n";

            try
            {
                var response = await _submissionService.SubmitCodeAsync(new SubmitCodeRequest
                {
                    ProblemId = _problemId,
                    LanguageId = SelectedLanguage.LanguageId,
                    SourceCode = Code,
                    AssignmentId = _assignmentId
                });

                if (response?.Success == true && response.Data != null)
                {
                    Output = $"🎉 Đã nộp bài thành công!\n\nSubmission ID: {response.Data.SubmissionId}\nStatus: {response.Data.Status}\nThời gian nộp: {response.Data.SubmittedAt:dd/MM/yyyy HH:mm:ss}\n\nĐang chấm điểm... (0s)";
                    await PollSubmissionResult(response.Data.SubmissionId, Code, true);
                }
                else
                {
                    Output = $"❌ Lỗi: {response?.Message ?? "Không thể nộp bài"}";
                }
            }
            catch (System.Exception ex)
            {
                Output = $"❌ Lỗi: {ex.Message}";
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        private async Task PollSubmissionResult(string submissionId, string sourceCode, bool isSubmit)
        {
            int maxAttempts = 30;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                await Task.Delay(2000);

                try
                {
                    var response = await _submissionService.GetSubmissionAsync(submissionId);
                    if (response?.Success == true && response.Data != null)
                    {
                        var submission = response.Data;

                        if (submission.Status == "Pending" || submission.Status == "Running")
                        {
                            attempts++;
                            var lines = Output.Split('\n');
                            if (lines.Length > 0)
                            {
                                lines[lines.Length - 1] = $"Đang xử lý... ({attempts}s)";
                                Output = string.Join("\n", lines);
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
                                LastRunCode = sourceCode;
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
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window is Views.Students.ProblemSolverWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        private void ViewSubmissionDetail(Submission submission)
        {
            if (submission == null) return;

            var dialog = new Views.Students.SubmissionDetailDialog(submission);
            dialog.ShowDialog();
        }
    }
}

