using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using UCode.Desktop.Models;
using UCode.Desktop.Services;
using UCode.Desktop.Helpers;

namespace UCode.Desktop.ViewModels.Students
{
    public class StudentSubmissionsViewModel : ViewModelBase
    {
        private readonly SubmissionService _submissionService;
        private readonly ProblemService _problemService;
        private readonly NavigationService _navigationService;
        private bool _isLoading;
        private ObservableCollection<Submission> _submissions;
        private readonly System.Collections.Generic.Dictionary<string, string> _problemTitleCache = new();

        // Pagination
        private int _pageNumber = 1;
        public int PageSize { get; private set; } = 20;
        private bool _hasMoreData = true; // Simple check, ideally API returns total count

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public ObservableCollection<Submission> Submissions
        {
            get => _submissions;
            set
            {
                if (SetProperty(ref _submissions, value))
                {
                    OnPropertyChanged(nameof(Submissions));
                }
            }
        }

        public int PageNumber
        {
            get => _pageNumber;
            set
            {
                if (SetProperty(ref _pageNumber, value))
                {
                    OnPropertyChanged(nameof(PageNumber));
                    OnPropertyChanged(nameof(CanGoPrevious));
                }
            }
        }

        public bool CanGoPrevious => PageNumber > 1;

        public ICommand LoadDataCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand ViewSubmissionDetailCommand { get; }
        public ICommand ViewTestCaseResultsCommand { get; }
        public ICommand NavigateToProblemCommand { get; }

        public StudentSubmissionsViewModel()
        {
            var apiService = App.ServiceProvider.GetService(typeof(ApiService)) as ApiService;
            _submissionService = new SubmissionService(apiService);
            _problemService = new ProblemService(apiService);
            _navigationService = App.ServiceProvider.GetService(typeof(NavigationService)) as NavigationService;

            Submissions = new ObservableCollection<Submission>();

            LoadDataCommand = new RelayCommand(async _ => await LoadSubmissionsAsync());
            RefreshCommand = new RelayCommand(async _ =>
            {
                PageNumber = 1;
                await LoadSubmissionsAsync();
            });

            NextPageCommand = new RelayCommand(async _ =>
            {
                PageNumber++;
                await LoadSubmissionsAsync();
            });

            PreviousPageCommand = new RelayCommand(async _ =>
            {
                if (PageNumber > 1)
                {
                    PageNumber--;
                    await LoadSubmissionsAsync();
                }
            }, _ => CanGoPrevious);

            ViewSubmissionDetailCommand = new RelayCommand<Submission>(submission => ViewSubmissionDetail(submission));
            ViewTestCaseResultsCommand = new RelayCommand<Submission>(submission => ViewTestCaseResults(submission));
            NavigateToProblemCommand = new RelayCommand<Submission>(async submission => await NavigateToProblem(submission));

            // Initial load
            _ = LoadSubmissionsAsync();
        }

        private async Task LoadSubmissionsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                Submissions.Clear();

                var response = await _submissionService.GetUserSubmissionsAsync(PageNumber, PageSize);

                if (response.Success && response.Data != null)
                {
                    foreach (var sub in response.Data)
                    {
                        // Try to get problem title if missing
                        if (string.IsNullOrEmpty(sub.ProblemTitle) && !string.IsNullOrEmpty(sub.ProblemId))
                        {
                            if (_problemTitleCache.ContainsKey(sub.ProblemId))
                            {
                                sub.ProblemTitle = _problemTitleCache[sub.ProblemId];
                            }
                            else
                            {
                                // Fetch problem details
                                try
                                {
                                    var problemResponse = await _problemService.GetProblemForStudentAsync(sub.ProblemId);
                                    if (problemResponse.Success && problemResponse.Data != null)
                                    {
                                        sub.ProblemTitle = problemResponse.Data.Title;
                                        _problemTitleCache[sub.ProblemId] = problemResponse.Data.Title;
                                    }
                                }
                                catch
                                {
                                    // Ignore error, just leave empty
                                }
                            }
                        }

                        Submissions.Add(sub);
                    }

                    // Logic to enable/disable Next button could be refined if API returned total count
                    // For now assuming if regular count < pageSize, it's end
                    _hasMoreData = response.Data.Count == PageSize;
                }
                else
                {
                    Debug.WriteLine($"Failed to load submissions: {response.Message}");
                    if (PageNumber > 1) PageNumber--; // Revert if fail on next page
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading submissions: {ex.Message}");
                await ShowMessageAsync("Lỗi", $"Không thể tải lịch sử nộp bài: {ex.Message}");
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

        private async Task NavigateToProblem(Submission submission)
        {
            try
            {
                if (submission == null) return;

                if (string.IsNullOrEmpty(submission.ProblemId))
                {
                    await ShowMessageAsync("Lỗi", "Không tìm thấy Problem ID");
                    return;
                }

                if (string.IsNullOrEmpty(submission.AssignmentId))
                {
                    await ShowMessageAsync("Lỗi", "Không tìm thấy Assignment ID. Không thể mở bài toán.");
                    return;
                }

                // Navigate to ProblemSolverPage
                var problemPage = App.ServiceProvider.GetService(typeof(Pages.ProblemSolverPage)) as Pages.ProblemSolverPage;
                if (problemPage == null)
                {
                    await ShowMessageAsync("Lỗi", "Không thể tạo ProblemSolverPage.");
                    return;
                }

                var viewModel = problemPage.DataContext as ProblemSolverViewModel;
                if (viewModel == null)
                {
                    await ShowMessageAsync("Lỗi", "Không thể tạo ProblemSolverViewModel.");
                    return;
                }

                await viewModel.InitializeAsync(submission.AssignmentId, submission.ProblemId);
                _navigationService.NavigateTo(problemPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to problem: {ex.Message}");
                await ShowMessageAsync("Lỗi", $"Lỗi khi mở bài toán: {ex.Message}");
            }
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
