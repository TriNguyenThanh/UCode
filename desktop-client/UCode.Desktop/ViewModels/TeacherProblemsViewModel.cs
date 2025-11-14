using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Models.Enums;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    // ViewModel wrapper for displaying Problem in UI
    public class ProblemListItem
    {
        private readonly Problem _problem;

        public ProblemListItem(Problem problem)
        {
            _problem = problem;
        }

        public string ProblemId => _problem.ProblemId;
        public string Code => _problem.Code;
        public string Title => _problem.Title;
        
        public string Difficulty => GetDifficultyDisplayText(_problem.Difficulty);
        public string DifficultyColor => GetDifficultyColor(_problem.Difficulty);
        
        public string Status => _problem.Status.ToString();
        public string StatusColor => GetStatusColor(_problem.Status);
        
        public string Visibility => _problem.Visibility.ToString();
        
        public string FilePath => $"problems/{_problem.Code}/statement.md...";
        
        public string Tags => string.Empty; // TODO: Backend cần trả về TagNames
        
        public string TimeLimit => $"{_problem.TimeLimitMs}ms / {_problem.MemoryLimitKb}KB";
        
        public DateTime CreatedAt => _problem.CreatedAt;
        public string CreatedAtDisplay => _problem.CreatedAt.ToString("dd/MM/yyyy HH:mm");

        private static string GetDifficultyDisplayText(Models.Enums.Difficulty difficulty)
        {
            return difficulty switch
            {
                Models.Enums.Difficulty.EASY => "Dễ",
                Models.Enums.Difficulty.MEDIUM => "Trung bình",
                Models.Enums.Difficulty.HARD => "Khó",
                _ => difficulty.ToString()
            };
        }

        private static string GetDifficultyColor(Models.Enums.Difficulty difficulty)
        {
            return difficulty switch
            {
                Models.Enums.Difficulty.EASY => "#28a745",
                Models.Enums.Difficulty.MEDIUM => "#ffc107",
                Models.Enums.Difficulty.HARD => "#dc3545",
                _ => "#6c757d"
            };
        }

        private static string GetStatusColor(Models.Enums.ProblemStatus status)
        {
            return status switch
            {
                Models.Enums.ProblemStatus.PUBLISHED => "#28a745",
                Models.Enums.ProblemStatus.DRAFT => "#ffc107",
                Models.Enums.ProblemStatus.ARCHIVED => "#6c757d",
                _ => "#6c757d"
            };
        }
    }

    public class TeacherProblemsViewModel : ViewModelBase
    {
        private readonly ProblemService _problemService;
        private readonly NavigationService _navigationService;
        private bool _isLoading;
        private string _searchText = string.Empty;
        private string _selectedDifficulty = "Tất cả";
        private string _selectedStatus = "Tất cả";
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _totalPages = 1;
        private int _totalProblems;
        private int _easyProblems;
        private int _mediumProblems;
        private int _hardProblems;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _ = SearchProblemsAsync();
                }
            }
        }

        public string SelectedDifficulty
        {
            get => _selectedDifficulty;
            set
            {
                if (SetProperty(ref _selectedDifficulty, value))
                {
                    // Notify all filter button states changed
                    OnPropertyChanged(nameof(IsFilterAll));
                    OnPropertyChanged(nameof(IsFilterEasy));
                    OnPropertyChanged(nameof(IsFilterMedium));
                    OnPropertyChanged(nameof(IsFilterHard));
                    _ = LoadProblemsAsync();
                }
            }
        }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    _ = LoadProblemsAsync();
                }
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        public int TotalProblems
        {
            get => _totalProblems;
            set => SetProperty(ref _totalProblems, value);
        }

        public int EasyProblems
        {
            get => _easyProblems;
            set => SetProperty(ref _easyProblems, value);
        }

        public int MediumProblems
        {
            get => _mediumProblems;
            set => SetProperty(ref _mediumProblems, value);
        }

        public int HardProblems
        {
            get => _hardProblems;
            set => SetProperty(ref _hardProblems, value);
        }

        // Filter button active states
        public bool IsFilterAll => SelectedDifficulty == "Tất cả";
        public bool IsFilterEasy => SelectedDifficulty == "EASY";
        public bool IsFilterMedium => SelectedDifficulty == "MEDIUM";
        public bool IsFilterHard => SelectedDifficulty == "HARD";

        public ObservableCollection<ProblemListItem> Problems { get; } = new();
        public ObservableCollection<string> Difficulties { get; } = new() { "Tất cả", "EASY", "MEDIUM", "HARD" };
        public ObservableCollection<string> Statuses { get; } = new() { "Tất cả", "DRAFT", "PUBLISHED", "ARCHIVED" };

        public ICommand CreateProblemCommand { get; }
        public ICommand EditProblemCommand { get; }
        public ICommand DeleteProblemCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand FilterAllCommand { get; }
        public ICommand FilterEasyCommand { get; }
        public ICommand FilterMediumCommand { get; }
        public ICommand FilterHardCommand { get; }

        public TeacherProblemsViewModel(ProblemService problemService, NavigationService navigationService)
        {
            _problemService = problemService;
            _navigationService = navigationService;

            CreateProblemCommand = new RelayCommand(_ => ExecuteCreateProblem());
            EditProblemCommand = new RelayCommand(param => ExecuteEditProblem(param as string ?? string.Empty));
            DeleteProblemCommand = new RelayCommand(param => ExecuteDeleteProblem(param as string ?? string.Empty));
            RefreshCommand = new RelayCommand(async _ => await LoadProblemsAsync());
            PreviousPageCommand = new RelayCommand(_ => ExecutePreviousPage(), _ => CurrentPage > 1);
            NextPageCommand = new RelayCommand(_ => ExecuteNextPage(), _ => CurrentPage < TotalPages);
            FilterAllCommand = new RelayCommand(_ => ExecuteFilterAll());
            FilterEasyCommand = new RelayCommand(_ => ExecuteFilterEasy());
            FilterMediumCommand = new RelayCommand(_ => ExecuteFilterMedium());
            FilterHardCommand = new RelayCommand(_ => ExecuteFilterHard());
        }

        public async Task LoadProblemsAsync()
        {
            IsLoading = true;
            try
            {
                var response = await _problemService.GetMyProblemsAsync(CurrentPage, _pageSize);
                Problems.Clear();

                if (response?.Success == true && response.Data?.Items != null)
                {
                    // Update pagination info from backend
                    CurrentPage = response.Data.Page;
                    TotalPages = response.Data.TotalPages;

                    // Update stats from backend
                    TotalProblems = response.Data.TotalCount;
                    EasyProblems = response.Data.Items.Count(p => p.Difficulty == Models.Enums.Difficulty.EASY);
                    MediumProblems = response.Data.Items.Count(p => p.Difficulty == Models.Enums.Difficulty.MEDIUM);
                    HardProblems = response.Data.Items.Count(p => p.Difficulty == Models.Enums.Difficulty.HARD);

                    foreach (var problem in response.Data.Items)
                    {
                        // Apply filters
                        if (SelectedDifficulty != "Tất cả" && problem.Difficulty.ToString() != SelectedDifficulty)
                            continue;

                        if (SelectedStatus != "Tất cả" && problem.Status.ToString() != SelectedStatus)
                            continue;

                        Problems.Add(new ProblemListItem(problem));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải danh sách đề bài: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchProblemsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadProblemsAsync();
                return;
            }

            IsLoading = true;
            try
            {
                // Load more items for search
                var response = await _problemService.GetMyProblemsAsync(1, 100);
                Problems.Clear();

                if (response?.Success == true && response.Data?.Items != null)
                {
                    // Filter problems by search text
                    var filtered = response.Data.Items
                        .Where(p => p.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                   p.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

                    // Update stats based on filtered results
                    TotalProblems = filtered.Count();
                    EasyProblems = filtered.Count(p => p.Difficulty == Models.Enums.Difficulty.EASY);
                    MediumProblems = filtered.Count(p => p.Difficulty == Models.Enums.Difficulty.MEDIUM);
                    HardProblems = filtered.Count(p => p.Difficulty == Models.Enums.Difficulty.HARD);

                    foreach (var problem in filtered)
                    {
                        Problems.Add(new ProblemListItem(problem));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteCreateProblem()
        {
            try
            {
                var viewModel = App.ServiceProvider?.GetService(typeof(ProblemCreateViewModel)) as ProblemCreateViewModel;
                if (viewModel != null)
                {
                    var page = new Pages.ProblemCreatePage(viewModel);
                    _navigationService.NavigateTo(page);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEditProblem(string problemId)
        {
            if (string.IsNullOrEmpty(problemId)) return;

            try
            {
                var viewModel = App.ServiceProvider?.GetService(typeof(ProblemEditViewModel)) as ProblemEditViewModel;
                if (viewModel != null)
                {
                    var page = new Pages.ProblemEditPage(viewModel);
                    _navigationService.NavigateTo(page, problemId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở trang chỉnh sửa đề bài: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExecuteDeleteProblem(string problemId)
        {
            if (string.IsNullOrEmpty(problemId)) return;

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa đề bài này?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    var response = await _problemService.DeleteProblemAsync(problemId);
                    if (response?.Success == true)
                    {
                        MessageBox.Show("Xóa đề bài thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadProblemsAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Xóa đề bài thất bại: {response?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xóa đề bài: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void ExecutePreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                _ = LoadProblemsAsync();
            }
        }

        private void ExecuteNextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                _ = LoadProblemsAsync();
            }
        }

        private void ExecuteFilterAll()
        {
            SelectedDifficulty = "Tất cả";
        }

        private void ExecuteFilterEasy()
        {
            SelectedDifficulty = "EASY";
        }

        private void ExecuteFilterMedium()
        {
            SelectedDifficulty = "MEDIUM";
        }

        private void ExecuteFilterHard()
        {
            SelectedDifficulty = "HARD";
        }
    }
}

