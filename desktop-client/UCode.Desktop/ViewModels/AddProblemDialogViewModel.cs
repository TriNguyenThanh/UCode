using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class ProblemItemViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isEditingPoints;
        private int _points = 100;
        private int _originalPoints = 100;

        public string ProblemId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public Difficulty Difficulty { get; set; } = Difficulty.EASY;
        public List<string> TagNames { get; set; } = new();

        public List<string> TagsPreview => TagNames.Take(2).ToList();
        public string TagsTooltip => TagNames.Count > 2 ? string.Join(", ", TagNames.Skip(2)) : "";

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public bool IsEditingPoints
        {
            get => _isEditingPoints;
            set
            {
                if (_isEditingPoints != value)
                {
                    _isEditingPoints = value;
                    OnPropertyChanged(nameof(IsEditingPoints));
                }
            }
        }

        public int Points
        {
            get => _points;
            set
            {
                if (_points != value)
                {
                    _points = value;
                    OnPropertyChanged(nameof(Points));
                }
            }
        }

        public int OriginalPoints
        {
            get => _originalPoints;
            set => _originalPoints = value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AddProblemDialogViewModel : ViewModelBase
    {
        private readonly string _assignmentId;
        private readonly ProblemService _problemService;
        private readonly AssignmentService _assignmentService;
        private readonly List<Models.AssignmentProblem> _existingProblems;

        private string _searchText = string.Empty;
        private string _difficultyFilter = "all";
        private bool _allSelected;
        private bool _isLoading;
        private bool _isSaving;
        private bool _hasChanges;

        public ObservableCollection<ProblemItemViewModel> AllProblems { get; } = new();
        public ObservableCollection<ProblemItemViewModel> FilteredProblems { get; } = new();

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string DifficultyFilter
        {
            get => _difficultyFilter;
            set
            {
                if (SetProperty(ref _difficultyFilter, value))
                {
                    ApplyFilters();
                }
            }
        }

        public bool AllSelected
        {
            get => _allSelected;
            set
            {
                if (SetProperty(ref _allSelected, value))
                {
                    foreach (var problem in FilteredProblems)
                    {
                        problem.IsSelected = value;
                    }
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(HasSelectedProblems));
                    OnPropertyChanged(nameof(CanSave));
                    _hasChanges = true;
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                if (SetProperty(ref _isSaving, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public int SelectedCount => AllProblems.Count(p => p.IsSelected);
        public bool HasSelectedProblems => SelectedCount > 0;
        public bool CanSave => HasSelectedProblems && !IsSaving;

        public ICommand StartEditPointsCommand { get; }
        public ICommand SavePointsCommand { get; }
        public ICommand CancelEditPointsCommand { get; }
        public ICommand SaveCommand { get; }

        public AddProblemDialogViewModel(
            string assignmentId,
            ProblemService problemService,
            AssignmentService assignmentService,
            List<Models.AssignmentProblem> existingProblems)
        {
            _assignmentId = assignmentId;
            _problemService = problemService;
            _assignmentService = assignmentService;
            _existingProblems = existingProblems;

            StartEditPointsCommand = new RelayCommand(param => StartEditPoints(param as ProblemItemViewModel));
            SavePointsCommand = new RelayCommand(param => SavePoints(param as ProblemItemViewModel));
            CancelEditPointsCommand = new RelayCommand(param => CancelEditPoints(param as ProblemItemViewModel));
            SaveCommand = new RelayCommand(async _ => await SaveProblemsAsync());
        }

        public async Task LoadProblemsAsync()
        {
            IsLoading = true;

            try
            {
                // Load all problems (simplified - in production you'd want pagination)
                var response = await _problemService.GetMyProblemsAsync(page: 1, pageSize: 100);

                if (response?.Success == true && response.Data != null)
                {
                    AllProblems.Clear();

                    foreach (var problem in response.Data.Items)
                    {
                        var existing = _existingProblems.FirstOrDefault(p => p.ProblemId == problem.ProblemId);

                        AllProblems.Add(new ProblemItemViewModel
                        {
                            ProblemId = problem.ProblemId,
                            Code = problem.Code,
                            Title = problem.Title,
                            Difficulty = problem.Difficulty,
                            TagNames = problem.TagNames.ToList(),
                            Points = existing?.Points ?? 100,
                            OriginalPoints = existing?.Points ?? 100,
                            IsSelected = existing != null,
                            IsEditingPoints = false
                        });
                    }

                    ApplyFilters();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách bài: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilters()
        {
            FilteredProblems.Clear();

            var query = AllProblems.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(p =>
                    p.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.ProblemId.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Difficulty filter
            if (DifficultyFilter != "all")
            {
                query = query.Where(p => p.Difficulty.ToString() == DifficultyFilter);
            }

            foreach (var problem in query)
            {
                FilteredProblems.Add(problem);
            }

            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(HasSelectedProblems));
            OnPropertyChanged(nameof(CanSave));
        }

        private void StartEditPoints(ProblemItemViewModel? problem)
        {
            if (problem == null || !problem.IsSelected) return;

            problem.IsEditingPoints = true;
        }

        private void SavePoints(ProblemItemViewModel? problem)
        {
            if (problem == null) return;

            if (problem.Points <= 0 || problem.Points > 1000)
            {
                MessageBox.Show("Điểm phải từ 1 đến 1000", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            problem.OriginalPoints = problem.Points;
            problem.IsEditingPoints = false;
            _hasChanges = true;
        }

        private void CancelEditPoints(ProblemItemViewModel? problem)
        {
            if (problem == null) return;

            problem.Points = problem.OriginalPoints;
            problem.IsEditingPoints = false;
        }

        public List<ProblemItemViewModel> GetSelectedProblems()
        {
            return AllProblems
                .Where(p => p.IsSelected)
                .ToList();
        }

        private async Task SaveProblemsAsync()
        {
            IsSaving = true;

            try
            {
                var selectedProblems = GetSelectedProblems();

                if (selectedProblems.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một bài toán!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Close dialog with success and return selected problems
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext == this)
                    {
                        window.DialogResult = true;
                        window.Close();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}

