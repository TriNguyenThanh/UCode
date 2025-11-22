using System;
using System.Collections.Generic;
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
    public class TagItem : ViewModelBase
    {
        private bool _isSelected;

        public string TagId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class TagSelectionViewModel : ViewModelBase
    {
        private readonly string _problemId;
        private readonly TagService _tagService;
        private bool _isLoading;
        private bool _isSaving;

        public ObservableCollection<TagItem> AllTags { get; } = new();
        public HashSet<string> SelectedTagIds { get; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(HasNoSelectedTags));
                }
            }
        }

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public IEnumerable<TagItem> SelectedTags => AllTags.Where(t => t.IsSelected);
        public bool HasNoSelectedTags => !SelectedTags.Any();

        public ICommand ToggleTagCommand { get; }
        public ICommand SaveCommand { get; }

        public TagSelectionViewModel(string problemId, TagService tagService, List<string> currentTags)
        {
            _problemId = problemId;
            _tagService = tagService;

            // Initialize selected tags from current problem tags
            foreach (var tagName in currentTags)
            {
                // Will be mapped to tagIds after loading all tags
            }

            ToggleTagCommand = new RelayCommand(param => ToggleTag(param as string));
            SaveCommand = new RelayCommand(async _ => await SaveTagsAsync());
        }

        public async Task LoadTagsAsync()
        {
            IsLoading = true;

            try
            {
                var response = await _tagService.GetAllTagsAsync();
                if (response?.Success == true && response.Data != null)
                {
                    AllTags.Clear();
                    SelectedTagIds.Clear();

                    foreach (var tag in response.Data)
                    {
                        var tagItem = new TagItem
                        {
                            TagId = tag.TagId,
                            Name = tag.Name,
                            IsSelected = false // Will be set below
                        };

                        AllTags.Add(tagItem);
                    }

                    OnPropertyChanged(nameof(SelectedTags));
                    OnPropertyChanged(nameof(HasNoSelectedTags));
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Lỗi khi tải tags: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void InitializeSelectedTags(List<string> currentTagNames)
        {
            foreach (var tag in AllTags)
            {
                if (currentTagNames.Contains(tag.Name))
                {
                    tag.IsSelected = true;
                    SelectedTagIds.Add(tag.TagId);
                }
            }

            OnPropertyChanged(nameof(SelectedTags));
            OnPropertyChanged(nameof(HasNoSelectedTags));
        }

        private void ToggleTag(string tagId)
        {
            if (string.IsNullOrEmpty(tagId)) return;

            var tag = AllTags.FirstOrDefault(t => t.TagId == tagId);
            if (tag != null)
            {
                tag.IsSelected = !tag.IsSelected;

                if (tag.IsSelected)
                {
                    SelectedTagIds.Add(tagId);
                }
                else
                {
                    SelectedTagIds.Remove(tagId);
                }

                OnPropertyChanged(nameof(SelectedTags));
                OnPropertyChanged(nameof(HasNoSelectedTags));
            }
        }

        private async Task SaveTagsAsync()
        {
            IsSaving = true;

            try
            {
                // Get currently selected tag IDs
                var newTagIds = SelectedTagIds.ToList();

                // Clear existing tags and add new ones
                // Note: This is a simplified approach. In production, you might want to:
                // 1. Get current tags from problem
                // 2. Calculate diff (tags to add vs tags to remove)
                // 3. Call API accordingly

                if (newTagIds.Any())
                {
                    var response = await _tagService.AddTagsToProblemAsync(_problemId, newTagIds);

                    if (response?.Success == true)
                    {
                        await GetMetroWindow()?.ShowMessageAsync("Thành công", "Cập nhật tags thành công!");
                        
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
                else
                {
                    await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng chọn ít nhất một tag");
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

