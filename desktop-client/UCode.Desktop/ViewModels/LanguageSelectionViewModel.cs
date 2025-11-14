using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class LanguageItemViewModel : ViewModelBase
    {
        private bool _isSelected;

        public string LanguageId { get; set; } = string.Empty;
        public string LanguageDisplayName { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public double TimeFactor { get; set; } = 1.0;
        public int MemoryKb { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class LanguageSelectionViewModel : ViewModelBase
    {
        private readonly string _problemId;
        private readonly LanguageService _languageService;
        private readonly ProblemService _problemService;
        private bool _isLoading;
        private bool _isSaving;

        public ObservableCollection<LanguageItemViewModel> AllLanguages { get; } = new();

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

        public ICommand SaveCommand { get; }

        public LanguageSelectionViewModel(
            string problemId,
            LanguageService languageService,
            ProblemService problemService,
            ObservableCollection<ProblemLanguage> currentLanguages)
        {
            _problemId = problemId;
            _languageService = languageService;
            _problemService = problemService;

            SaveCommand = new RelayCommand(async _ => await SaveLanguagesAsync());
        }

        public async Task LoadLanguagesAsync()
        {
            IsLoading = true;

            try
            {
                // Load all available languages
                var allLangsResponse = await _languageService.GetAllLanguagesAsync(true);
                if (allLangsResponse?.Success == true && allLangsResponse.Data != null)
                {
                    AllLanguages.Clear();

                    foreach (var lang in allLangsResponse.Data)
                    {
                        AllLanguages.Add(new LanguageItemViewModel
                        {
                            LanguageId = lang.LanguageId,
                            LanguageDisplayName = lang.DisplayName,
                            LanguageCode = lang.Code,
                            TimeFactor = lang.DefaultTimeFactor,
                            MemoryKb = lang.DefaultMemoryKb,
                            IsSelected = false
                        });
                    }
                }

                // Load currently enabled languages for this problem
                var problemLangsResponse = await _problemService.GetAvailableLanguagesAsync(_problemId);
                if (problemLangsResponse?.Success == true && problemLangsResponse.Data != null)
                {
                    foreach (var problemLang in problemLangsResponse.Data)
                    {
                        var lang = AllLanguages.FirstOrDefault(l => l.LanguageCode == problemLang.LanguageCode);
                        if (lang != null)
                        {
                            lang.IsSelected = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải ngôn ngữ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveLanguagesAsync()
        {
            IsSaving = true;

            try
            {
                var selectedLanguages = AllLanguages.Where(l => l.IsSelected)
                    .Select(l => new ProblemLanguageRequest
                    {
                        ProblemId = _problemId,
                        LanguageId = l.LanguageId,
                        TimeFactor = l.TimeFactor,
                        MemoryKb = l.MemoryKb,
                        Head = string.Empty,
                        Body = string.Empty,
                        Tail = string.Empty
                    })
                    .ToList();

                if (selectedLanguages.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một ngôn ngữ", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var response = await _languageService.AddOrUpdateProblemLanguagesAsync(_problemId, selectedLanguages);

                if (response?.Success == true && response.Data != null)
                {
                    MessageBox.Show("Cập nhật ngôn ngữ thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Close dialog with success
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
    }
}

