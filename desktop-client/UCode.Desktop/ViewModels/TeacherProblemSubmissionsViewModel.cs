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
        private int _page;
        private int _rowsPerPage = 10;

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
                    UpdatePaginatedSubmissions();
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
                    Page = 0;
                    UpdatePaginatedSubmissions();
                }
            }
        }

        public ObservableCollection<BestSubmission> BestSubmissions { get; } = new();
        public ObservableCollection<BestSubmission> PaginatedSubmissions { get; } = new();

        // Statistics
        public int PassedCount => BestSubmissions.Count(s => s.PassedTestcase == s.TotalTestcase);
        public int PartialCount => BestSubmissions.Count(s => s.PassedTestcase > 0 && s.PassedTestcase < s.TotalTestcase);
        public int FailedCount => BestSubmissions.Count(s => s.PassedTestcase == 0);

        public ICommand BackCommand { get; }
        public ICommand SubmissionClickCommand { get; }
        public ICommand ChangePageCommand { get; }

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
                // Fetch all data in parallel
                var assignmentTask = _assignmentService.GetAssignmentAsync(_assignmentId);
                var problemTask = _problemService.GetProblemAsync(_problemId);
                var submissionsTask = _submissionService.GetBestSubmissionsAsync(_assignmentId, _problemId, 1, 100);

                await Task.WhenAll(assignmentTask, problemTask, submissionsTask);

                var assignmentResponse = await assignmentTask;
                var problemResponse = await problemTask;
                var submissionsResponse = await submissionsTask;

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

                if (submissionsResponse?.Success == true && submissionsResponse.Data != null)
                {
                    BestSubmissions.Clear();
                    foreach (var submission in submissionsResponse.Data)
                    {
                        BestSubmissions.Add(submission);
                    }

                    UpdatePaginatedSubmissions();
                    OnPropertyChanged(nameof(PassedCount));
                    OnPropertyChanged(nameof(PartialCount));
                    OnPropertyChanged(nameof(FailedCount));
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Không thể tải dữ liệu chấm bài: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdatePaginatedSubmissions()
        {
            PaginatedSubmissions.Clear();
            var skip = Page * RowsPerPage;
            var items = BestSubmissions.Skip(skip).Take(RowsPerPage);
            foreach (var item in items)
            {
                PaginatedSubmissions.Add(item);
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

        private void ExecuteChangePage(object parameter)
        {
            if (parameter is string direction)
            {
                if (direction == "next" && (Page + 1) * RowsPerPage < BestSubmissions.Count)
                {
                    Page++;
                }
                else if (direction == "prev" && Page > 0)
                {
                    Page--;
                }
            }
        }
    }
}
