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

namespace UCode.Desktop.ViewModels
{
    public class TeacherProblemSubmissionsViewModel : ViewModelBase
    {
        private readonly AssignmentService _assignmentService;
        private readonly SubmissionService _submissionService;
        private readonly ProblemService _problemService;
        private readonly ClassService _classService;
        private readonly NavigationService _navigationService;

        private bool _isLoading;
        private string _assignmentId = string.Empty;
        private string _problemId = string.Empty;
        private Assignment _assignment;
        private Class _classData;
        private Problem _problem;
        private int _page = 1; // API uses 1-based pagination
        private int _rowsPerPage = 10;
        private string _selectedFilter = "best";
        private int _totalCount = 0;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
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

        public int Page
        {
            get => _page;
            set
            {
                if (SetProperty(ref _page, value))
                {
                    OnPropertyChanged(nameof(CanGoPrevious));
                    OnPropertyChanged(nameof(CanGoNext));
                }
            }
        }

        public int RowsPerPage
        {
            get => _rowsPerPage;
            set
            {
                if (SetProperty(ref _rowsPerPage, value))
                {
                    _page = 1;
                    OnPropertyChanged(nameof(Page));
                    OnPropertyChanged(nameof(TotalPages));
                    OnPropertyChanged(nameof(CanGoPrevious));
                    OnPropertyChanged(nameof(CanGoNext));
                    _ = LoadPageDataAsync(); // Fire and forget
                }
            }
        }

        public int TotalCount
        {
            get => _totalCount;
            set
            {
                if (SetProperty(ref _totalCount, value))
                {
                    OnPropertyChanged(nameof(TotalPages));
                    OnPropertyChanged(nameof(CanGoPrevious));
                    OnPropertyChanged(nameof(CanGoNext));
                }
            }
        }

        public int TotalPages => TotalCount > 0 ? (int)Math.Ceiling((double)TotalCount / RowsPerPage) : 1;

        public bool CanGoPrevious
        {
            get
            {
                var result = Page > 1;
                return result;
            }
        }

        public bool CanGoNext
        {
            get
            {
                // If we're on page 1 and TotalCount is 0, assume we can try to go next
                // This handles the case where TotalCount hasn't loaded yet
                if (Page == 1 && TotalCount == 0)
                {
                    return true;
                }
                
                var result = Page < TotalPages;
                return result;
            }
        }

        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                {
                    _page = 1;
                    OnPropertyChanged(nameof(Page));
                    _ = OnFilterChangedAsync(); // Fire and forget
                }
            }
        }

        public ObservableCollection<BestSubmission> AllBestSubmissions { get; } = new(); // For client-side pagination
        public ObservableCollection<BestSubmission> DisplayedSubmissions { get; } = new();

        // Statistics - now based on total count from API
        public int PassedCount { get; private set; }
        public int PartialCount { get; private set; }
        public int FailedCount { get; private set; }

        public ICommand BackCommand { get; }
        public ICommand SubmissionClickCommand { get; }
        public ICommand ChangePageCommand { get; }
        public ICommand SetFilterCommand { get; }

        public TeacherProblemSubmissionsViewModel(
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
            SubmissionClickCommand = new RelayCommand(param => ExecuteSubmissionClick(param));
            ChangePageCommand = new RelayCommand(param => ExecuteChangePage(param));
            SetFilterCommand = new RelayCommand(param => ExecuteSetFilter(param));
        }

        public async Task InitializeAsync(string assignmentId, string problemId)
        {
            _assignmentId = assignmentId;
            _problemId = problemId;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;

            try
            {
                // Fetch metadata in parallel
                var assignmentTask = _assignmentService.GetAssignmentAsync(_assignmentId);
                var problemTask = _problemService.GetProblemAsync(_problemId);
                
                await Task.WhenAll(assignmentTask, problemTask);

                var assignmentResponse = await assignmentTask;
                var problemResponse = await problemTask;

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

                // Load initial data based on filter
                if (SelectedFilter == "best")
                {
                    // For "best" filter: Load ALL best submissions for client-side pagination
                    await LoadAllBestSubmissionsAsync();
                }
                else
                {
                    // For "all" filter: Get stats then load first page (server-side pagination)
                    var statsResponse = await _submissionService.GetStatsPerProblemAsync(_assignmentId, _problemId);
                    if (statsResponse?.Success == true)
                    {
                        TotalCount = statsResponse.Data.Total;
                        PassedCount = statsResponse.Data.Passed;
                        PartialCount = statsResponse.Data.Partial;
                        FailedCount = statsResponse.Data.Failed;
                    }
                    else
                    {
                        TotalCount = 0;
                        PassedCount = 0;
                        PartialCount = 0;
                        FailedCount = 0;
                    }

                    OnPropertyChanged(nameof(PassedCount));
                    OnPropertyChanged(nameof(PartialCount));
                    OnPropertyChanged(nameof(FailedCount));
                    
                    await LoadPageDataAsync();
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Không thể tải dữ liệu: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnFilterChangedAsync()
        {
            if (SelectedFilter == "best")
            {
                await LoadAllBestSubmissionsAsync();
            }
            else
            {
                // Switch to server-side
                try 
                {
                    // Get stats first
                    var statsResponse = await _submissionService.GetStatsPerProblemAsync(_assignmentId, _problemId);
                    if (statsResponse?.Success == true)
                    {
                        TotalCount = statsResponse.Data.Total;
                        PassedCount = statsResponse.Data.Passed;
                        PartialCount = statsResponse.Data.Partial;
                        FailedCount = statsResponse.Data.Failed;
                    }
                    else 
                    {
                        TotalCount = 0;
                        PassedCount = 0;
                        PartialCount = 0;
                        FailedCount = 0;
                    }

                    OnPropertyChanged(nameof(PassedCount));
                    OnPropertyChanged(nameof(PartialCount));
                    OnPropertyChanged(nameof(FailedCount));
                    
                    // Then load page data (it handles IsLoading internally)
                    await LoadPageDataAsync();
                }
                catch (Exception ex)
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Không thể tải dữ liệu: {ex.Message}");
                }
            }
        }

        private async Task LoadAllBestSubmissionsAsync()
        {
            // Don't set IsLoading here if it's already set by caller (LoadDataAsync)
            // But if called from Filter change, we might need to set it.
            bool localLoading = !IsLoading;
            if (localLoading) IsLoading = true;

            try
            {
                // Fetch all (using large page size to get everything)
                var response = await _submissionService.GetBestSubmissionsAsync(_assignmentId, _problemId, 1, 10000);
                
                if (response?.Success == true && response.Data != null)
                {
                    AllBestSubmissions.Clear();
                    var items = response.Data;
                    for(int i=0; i<items.Count; i++) 
                    {
                         items[i].TotalSubmission = i + 1;
                         AllBestSubmissions.Add(items[i]);
                    }
                    
                    TotalCount = AllBestSubmissions.Count;
                    
                    // Calculate stats for ALL best submissions locally to ensure consistency with the displayed list
                    PassedCount = AllBestSubmissions.Count(s => s.PassedTestcase == s.TotalTestcase);
                    PartialCount = AllBestSubmissions.Count(s => s.PassedTestcase > 0 && s.PassedTestcase < s.TotalTestcase);
                    FailedCount = AllBestSubmissions.Count(s => s.PassedTestcase == 0);

                    OnPropertyChanged(nameof(PassedCount));
                    OnPropertyChanged(nameof(PartialCount));
                    OnPropertyChanged(nameof(FailedCount));

                    UpdateDisplayedSubmissionsFromCache();
                }
                else
                {
                    AllBestSubmissions.Clear();
                    TotalCount = 0;
                    DisplayedSubmissions.Clear();
                }
            }
            catch (Exception ex)
            {
                 await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Không thể tải danh sách tốt nhất: {ex.Message}");
            }
            finally 
            { 
                if (localLoading) IsLoading = false; 
            }
        }

        private void UpdateDisplayedSubmissionsFromCache()
        {
            var startIndex = (Page - 1) * RowsPerPage;
            var items = AllBestSubmissions.Skip(startIndex).Take(RowsPerPage).ToList();
            
            DisplayedSubmissions.Clear();
            foreach (var item in items)
            {
                DisplayedSubmissions.Add(item);
            }
        }

        private async Task LoadPageDataAsync()
        {
            if (SelectedFilter == "best")
            {
                // Client-side pagination
                UpdateDisplayedSubmissionsFromCache();
                return;
            }

            // Server-side pagination for "all"
            // Prevent re-entrant calls
            if (IsLoading)
            {
                return;
            }

            IsLoading = true;

            try
            {
                ApiResponse<PagedResultDto<Submission>> allSubmissionsResponse;

                // Load all submissions with pagination
                allSubmissionsResponse = await _submissionService.GetSubmissionsByAssignmentAndProblemAsync(
                    _assignmentId, _problemId, Page, RowsPerPage);

                if (allSubmissionsResponse?.Success == true && allSubmissionsResponse.Data?.Items != null)
                {
                    // Batch update to avoid multiple CollectionChanged events
                    var startIndex = (Page - 1) * RowsPerPage;
                    var newItems = allSubmissionsResponse.Data.Items.Select((submission, index) => new BestSubmission
                    {
                        SubmissionId = submission.SubmissionId,
                        UserId = submission.UserId,
                        UserFullName = submission.UserFullName,
                        UserCode = submission.UserCode,
                        ProblemId = submission.ProblemId,
                        SourceCode = submission.SourceCode,
                        LanguageCode = submission.LanguageCode,
                        Status = submission.Status,
                        CompareResult = submission.CompareResult,
                        ErrorCode = submission.ErrorCode,
                        ErrorMessage = submission.ErrorMessage,
                        TotalTestcase = submission.TotalTestcase,
                        PassedTestcase = submission.PassedTestcase,
                        Score = submission.Score,
                        TotalTime = submission.TotalTime,
                        TotalMemory = submission.TotalMemory,
                        SubmittedAt = submission.SubmittedAt,
                        ResultFileRef = submission.ResultFileRef,
                        TotalSubmission = startIndex + index + 1 // Using TotalSubmission as RowNumber
                    }).ToList();

                    DisplayedSubmissions.Clear();
                    foreach (var item in newItems)
                    {
                        DisplayedSubmissions.Add(item);
                    }
                }
                else
                {
                    // No data or error
                    DisplayedSubmissions.Clear();
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Không thể tải dữ liệu trang: {ex.Message}");
                DisplayedSubmissions.Clear();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteBack()
        {
            _navigationService.GoBack();
        }

        private void ExecuteSubmissionClick(object parameter)
        {
            if (parameter is BestSubmission submission)
            {
                // Navigate to submission detail
                var detailViewModel = new SubmissionDetailViewModel(
                    _assignmentService,
                    _submissionService,
                    _problemService,
                    _classService,
                    _navigationService);
                
                var detailPage = new Pages.SubmissionDetailPage(detailViewModel);
                var parameters = new { assignmentId = _assignmentId, problemId = _problemId, submissionId = submission.SubmissionId };
                _navigationService.NavigateTo(detailPage, parameters);
            }
        }

        private async void ExecuteChangePage(object parameter)
        {
            if (parameter is string direction)
            {
                if (direction == "next" && CanGoNext)
                {
                    Page++;
                    await LoadPageDataAsync();
                }
                else if (direction == "prev" && CanGoPrevious)
                {
                    Page--;
                    await LoadPageDataAsync();
                }
            }
        }

        private void ExecuteSetFilter(object parameter)
        {
            if (parameter is string filter && (filter == "all" || filter == "best"))
            {
                SelectedFilter = filter;
            }
        }
    }
}
