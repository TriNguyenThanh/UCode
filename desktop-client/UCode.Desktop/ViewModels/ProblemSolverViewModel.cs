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
        private Problem _problem;
        private bool _isLoading;
        private string _assignmentId;
        private string _problemId;
        private string _code;
        private string _selectedLanguage;

        public ProblemSolverViewModel(ProblemService problemService, SubmissionService submissionService)
        {
            _problemService = problemService;
            _submissionService = submissionService;
            _problem = new Problem();
            _code = "// Your code here";
            
            NavigateBackCommand = new RelayCommand(_ => NavigateBack());
            RunCodeCommand = new RelayCommand(_ => RunCode(), _ => !string.IsNullOrEmpty(_code) && !string.IsNullOrEmpty(_selectedLanguage));
            SubmitCodeCommand = new RelayCommand(_ => SubmitCode(), _ => !string.IsNullOrEmpty(_code) && !string.IsNullOrEmpty(_selectedLanguage));
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

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                SetProperty(ref _selectedLanguage, value);
                ((RelayCommand)RunCodeCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SubmitCodeCommand).RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<Submission> Submissions { get; } = new();

        public ICommand NavigateBackCommand { get; }
        public ICommand RunCodeCommand { get; }
        public ICommand SubmitCodeCommand { get; }

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
                var response = await _problemService.GetProblemAsync(_problemId);
                if (response?.Success == true && response.Data != null)
                {
                    Problem = response.Data;
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

        private async void RunCode()
        {
            // TODO: Implement run code
            await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Tính năng chạy code đang được phát triển");
        }

        private async void SubmitCode()
        {
            // TODO: Implement submit code
            await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Tính năng nộp bài đang được phát triển");
        }

        private void NavigateBack()
        {
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window is Views.ProblemSolverWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}

