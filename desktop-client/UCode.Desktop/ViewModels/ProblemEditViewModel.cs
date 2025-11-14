using System;
using System.Collections.Generic;
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
    public class ProblemEditViewModel : ViewModelBase
    {
        private readonly ProblemService _problemService;
        private readonly LanguageService _languageService;
        private readonly DatasetService _datasetService;
        private readonly TagService _tagService;
        private readonly NavigationService _navigationService;

        private bool _isLoading;
        private bool _isSaving;
        private string _problemId = string.Empty;
        private int _selectedTabIndex;

        // Basic Info (Tab 0)
        private string _title = string.Empty;
        private string _code = string.Empty;
        private Difficulty _difficulty = Difficulty.EASY;
        private Models.Enums.Visibility _visibility = Models.Enums.Visibility.PRIVATE;
        private ProblemStatus _status = ProblemStatus.DRAFT;

        // Content (Tab 1)
        private string _statement = string.Empty;
        private string _inputFormat = string.Empty;
        private string _outputFormat = string.Empty;
        private string _constraints = string.Empty;
        private string _solution = string.Empty;

        // Limits (Tab 2)
        private int _timeLimitMs = 1000;
        private int _memoryLimitKb = 262144;
        private IoMode _ioMode = IoMode.STDIO;

        // Languages (Tab 3)
        public ObservableCollection<Language> AllLanguages { get; } = new();
        public ObservableCollection<ProblemLanguage> ProblemLanguages { get; } = new();

        // Datasets (Tab 4)
        public ObservableCollection<Dataset> Datasets { get; } = new();

        // Tags (Tab 5)
        public ObservableCollection<string> CurrentTags { get; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public string ProblemId
        {
            get => _problemId;
            set => SetProperty(ref _problemId, value);
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        // Basic Info Properties
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        public Difficulty Difficulty
        {
            get => _difficulty;
            set => SetProperty(ref _difficulty, value);
        }

        public Models.Enums.Visibility Visibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }

        public ProblemStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        // Content Properties
        public string Statement
        {
            get => _statement;
            set => SetProperty(ref _statement, value);
        }

        public string InputFormat
        {
            get => _inputFormat;
            set => SetProperty(ref _inputFormat, value);
        }

        public string OutputFormat
        {
            get => _outputFormat;
            set => SetProperty(ref _outputFormat, value);
        }

        public string Constraints
        {
            get => _constraints;
            set => SetProperty(ref _constraints, value);
        }

        public string Solution
        {
            get => _solution;
            set => SetProperty(ref _solution, value);
        }

        // Limits Properties
        public int TimeLimitMs
        {
            get => _timeLimitMs;
            set => SetProperty(ref _timeLimitMs, value);
        }

        public int MemoryLimitKb
        {
            get => _memoryLimitKb;
            set => SetProperty(ref _memoryLimitKb, value);
        }

        public IoMode IoMode
        {
            get => _ioMode;
            set => SetProperty(ref _ioMode, value);
        }

        public ObservableCollection<Difficulty> Difficulties { get; } = new() { Difficulty.EASY, Difficulty.MEDIUM, Difficulty.HARD };
        public ObservableCollection<Models.Enums.Visibility> Visibilities { get; } = new() { Models.Enums.Visibility.PRIVATE, Models.Enums.Visibility.COURSE, Models.Enums.Visibility.PUBLIC };
        public ObservableCollection<ProblemStatus> Statuses { get; } = new() { ProblemStatus.DRAFT, ProblemStatus.PUBLISHED, ProblemStatus.ARCHIVED };
        public ObservableCollection<IoMode> IoModes { get; } = new() { IoMode.STDIO, IoMode.FILE };

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand RefreshLanguagesCommand { get; }
        public ICommand RefreshDatasetsCommand { get; }
        public ICommand RefreshTagsCommand { get; }
        public ICommand OpenTagDialogCommand { get; }
        public ICommand RemoveTagCommand { get; }
        public ICommand OpenLanguageDialogCommand { get; }
        public ICommand EditLanguageCommand { get; }
        public ICommand OpenDatasetDialogCommand { get; }
        public ICommand DeleteDatasetCommand { get; }

        public bool HasTags => CurrentTags.Count > 0;
        public bool HasNoTags => CurrentTags.Count == 0;
        public bool HasLanguages => ProblemLanguages.Count > 0;
        public bool HasNoLanguages => ProblemLanguages.Count == 0;
        public bool HasDatasets => Datasets.Count > 0;
        public bool HasNoDatasets => Datasets.Count == 0;

        public ProblemEditViewModel(
            ProblemService problemService,
            LanguageService languageService,
            DatasetService datasetService,
            TagService tagService,
            NavigationService navigationService)
        {
            _problemService = problemService;
            _languageService = languageService;
            _datasetService = datasetService;
            _tagService = tagService;
            _navigationService = navigationService;

            SaveCommand = new RelayCommand(async _ => await SaveProblemAsync());
            BackCommand = new RelayCommand(_ => ExecuteBack());
            RefreshLanguagesCommand = new RelayCommand(async _ => await LoadLanguagesAsync());
            RefreshDatasetsCommand = new RelayCommand(async _ => await LoadDatasetsAsync());
            RefreshTagsCommand = new RelayCommand(async _ => await LoadTagsAsync());
            OpenTagDialogCommand = new RelayCommand(_ => OpenTagDialog());
            RemoveTagCommand = new RelayCommand(async param => await RemoveTagAsync(param as string));
            OpenLanguageDialogCommand = new RelayCommand(_ => OpenLanguageDialog());
            EditLanguageCommand = new RelayCommand(param => EditLanguage(param as ProblemLanguage));
            OpenDatasetDialogCommand = new RelayCommand(param => OpenDatasetDialog(param as Dataset));
            DeleteDatasetCommand = new RelayCommand(async param => await DeleteDatasetAsync(param as string));
        }

        public async Task InitializeAsync(string problemId)
        {
            ProblemId = problemId;
            IsLoading = true;

            try
            {
                await Task.WhenAll(
                    LoadProblemAsync(),
                    LoadAllLanguagesAsync(),
                    LoadLanguagesAsync(),
                    LoadDatasetsAsync()
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadProblemAsync()
        {
            try
            {
                var response = await _problemService.GetProblemAsync(ProblemId);
                if (response?.Success == true && response.Data != null)
                {
                    var problem = response.Data;
                    Title = problem.Title;
                    Code = problem.Code;
                    Difficulty = problem.Difficulty;
                    Visibility = problem.Visibility;
                    Status = problem.Status;
                    Statement = problem.Statement;
                    InputFormat = problem.InputFormat;
                    OutputFormat = problem.OutputFormat;
                    Constraints = problem.Constraints;
                    Solution = problem.Solution;
                    TimeLimitMs = problem.TimeLimitMs;
                    MemoryLimitKb = problem.MemoryLimitKb;
                    IoMode = problem.IoMode;

                    CurrentTags.Clear();
                    foreach (var tag in problem.TagNames)
                    {
                        CurrentTags.Add(tag);
                    }

                    OnPropertyChanged(nameof(HasTags));
                    OnPropertyChanged(nameof(HasNoTags));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading problem: {ex.Message}");
            }
        }

        private void OpenTagDialog()
        {
            var viewModel = new TagSelectionViewModel(ProblemId, _tagService, CurrentTags.ToList());
            var dialog = new Views.TagSelectionDialog(viewModel);
            dialog.Owner = Application.Current.MainWindow;

            // After loading all tags, initialize selection
            dialog.Loaded += (s, e) =>
            {
                viewModel.InitializeSelectedTags(CurrentTags.ToList());
            };

            if (dialog.ShowDialog() == true)
            {
                // Reload problem to get updated tags
                _ = LoadProblemAsync();
            }
        }

        private async Task RemoveTagAsync(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) return;

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa tag \"{tagName}\"?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                // First, get all tags to find the tagId by name
                var tagsResponse = await _tagService.GetAllTagsAsync();
                if (tagsResponse?.Success == true && tagsResponse.Data != null)
                {
                    var tag = tagsResponse.Data.FirstOrDefault(t => t.Name == tagName);
                    if (tag != null)
                    {
                        var response = await _tagService.RemoveTagFromProblemAsync(ProblemId, tag.TagId);
                        if (response?.Success == true)
                        {
                            CurrentTags.Remove(tagName);
                            OnPropertyChanged(nameof(HasTags));
                            OnPropertyChanged(nameof(HasNoTags));
                            MessageBox.Show($"Đã xóa tag \"{tagName}\"", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Không thể xóa tag: {response?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAllLanguagesAsync()
        {
            try
            {
                var response = await _languageService.GetAllLanguagesAsync(false);
                if (response?.Success == true && response.Data != null)
                {
                    AllLanguages.Clear();
                    foreach (var lang in response.Data)
                    {
                        AllLanguages.Add(lang);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading all languages: {ex.Message}");
            }
        }

        private async Task LoadLanguagesAsync()
        {
            try
            {
                var response = await _languageService.GetProblemLanguagesAsync(ProblemId);
                if (response?.Success == true && response.Data != null)
                {
                    ProblemLanguages.Clear();
                    foreach (var lang in response.Data)
                    {
                        ProblemLanguages.Add(lang);
                    }

                    OnPropertyChanged(nameof(HasLanguages));
                    OnPropertyChanged(nameof(HasNoLanguages));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading problem languages: {ex.Message}");
            }
        }

        private void OpenLanguageDialog()
        {
            var viewModel = new LanguageSelectionViewModel(
                ProblemId,
                _languageService,
                _problemService,
                ProblemLanguages);
            
            var dialog = new Views.LanguageSelectionDialog(viewModel);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                // Reload languages
                _ = LoadLanguagesAsync();
            }
        }

        private void EditLanguage(ProblemLanguage language)
        {
            if (language == null) return;

            MessageBox.Show(
                $"Edit Language: {language.LanguageDisplayName}\n\n" +
                $"Time Factor: {language.TimeFactor}x\n" +
                $"Memory: {language.MemoryKb} KB\n\n" +
                "Language Detail Dialog - To be implemented",
                "Chỉnh sửa cấu hình ngôn ngữ",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // TODO: Implement LanguageDetailDialog
            // Allow editing timeFactor, memoryKb, head, body, tail
        }

        private async Task LoadDatasetsAsync()
        {
            try
            {
                var response = await _datasetService.GetDatasetsByProblemAsync(ProblemId);
                if (response?.Success == true && response.Data != null)
                {
                    Datasets.Clear();
                    foreach (var dataset in response.Data)
                    {
                        Datasets.Add(dataset);
                    }

                    OnPropertyChanged(nameof(HasDatasets));
                    OnPropertyChanged(nameof(HasNoDatasets));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading datasets: {ex.Message}");
            }
        }

        private void OpenDatasetDialog(Dataset? dataset = null)
        {
            var viewModel = new DatasetEditViewModel(ProblemId, _datasetService);
            viewModel.Initialize(dataset);

            var dialog = new Views.DatasetEditDialog(viewModel);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                // Reload datasets
                _ = LoadDatasetsAsync();
            }
        }

        private async Task DeleteDatasetAsync(string? datasetId)
        {
            if (string.IsNullOrEmpty(datasetId)) return;

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa dataset này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var response = await _datasetService.DeleteDatasetAsync(datasetId);
                if (response?.Success == true)
                {
                    await LoadDatasetsAsync();
                    MessageBox.Show("Xóa dataset thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Không thể xóa dataset: {response?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadTagsAsync()
        {
            // Tags are already loaded in LoadProblemAsync
            await Task.CompletedTask;
        }

        private async Task SaveProblemAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                MessageBox.Show("Vui lòng nhập tên bài toán", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsSaving = true;

            try
            {
                var request = new UpdateProblemRequest
                {
                    ProblemId = ProblemId,
                    Title = Title.Trim(),
                    Code = string.IsNullOrWhiteSpace(Code) ? null : Code.Trim(),
                    Difficulty = Difficulty,
                    Visibility = Visibility,
                    Status = Status,
                    Statement = Statement,
                    InputFormat = InputFormat,
                    OutputFormat = OutputFormat,
                    Constraints = Constraints,
                    Solution = Solution,
                    TimeLimitMs = TimeLimitMs,
                    MemoryLimitKb = MemoryLimitKb,
                    IoMode = IoMode
                };

                var response = await _problemService.UpdateProblemAsync(request);

                if (response?.Success == true)
                {
                    MessageBox.Show("Cập nhật bài toán thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Cập nhật thất bại: {response?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void ExecuteBack()
        {
            _navigationService.GoBack();
        }
    }
}

