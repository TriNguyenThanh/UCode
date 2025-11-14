using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Lỗi khi tải ngôn ngữ: {ex.Message}");
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
                    await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng chọn ít nhất một ngôn ngữ");
                    return;
                }

                var response = await _languageService.AddOrUpdateProblemLanguagesAsync(_problemId, selectedLanguages);

                if (response?.Success == true && response.Data != null)
                {
                    await GetMetroWindow()?.ShowMessageAsync("Thành công", "Cập nhật ngôn ngữ thành công!");

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
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Cập nhật thất bại: {response?.Message}");
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Lỗi: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}

