using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models.Admin;
using UCode.Desktop.Services;
using UCode.Desktop.Services.Admin;

namespace UCode.Desktop.ViewModels.Admin
{
    public class ClassItem : INotifyPropertyChanged
    {
        private bool _isSelected = false;

        public string ClassId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int AssignmentCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusDisplay => IsArchived ? "Đã lưu trữ" : (IsActive ? "Hoạt động" : "Không hoạt động");
        public string CreatedAtDisplay => CreatedAt.ToString("dd/MM/yyyy");

        public ClassItem(AdminClassResponse classResponse)
        {
            ClassId = classResponse.ClassId;
            Name = classResponse.Name;
            Description = classResponse.Description;
            ClassCode = classResponse.ClassCode;
            TeacherName = classResponse.TeacherName;
            StudentCount = classResponse.StudentCount;
            AssignmentCount = classResponse.AssignmentCount;
            IsActive = classResponse.IsActive;
            IsArchived = classResponse.IsArchived;
            CreatedAt = classResponse.CreatedAt;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AdminClassesViewModel : INotifyPropertyChanged
    {
        private readonly AdminClassService _classService;
        private readonly NavigationService _navigationService;
        private readonly IDialogCoordinator _dialogCoordinator;

        private bool _isLoading;
        private string _searchTerm = string.Empty;
        private string _selectedTeacherFilter = "all";
        private string _selectedStatusFilter = "all";
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _totalClasses;
        private const int PageSize = 20;

        public ObservableCollection<ClassItem> Classes { get; } = new();
        public ObservableCollection<string> TeacherFilters { get; } = new() { "Tất cả giáo viên" };
        public ObservableCollection<string> StatusFilters { get; } = new()
        {
            "Tất cả",
            "Hoạt động",
            "Đã lưu trữ",
            "Không hoạt động"
        };

        #region Properties

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        public string SelectedTeacherFilter
        {
            get => _selectedTeacherFilter;
            set
            {
                if (SetProperty(ref _selectedTeacherFilter, value))
                {
                    _ = LoadClassesAsync();
                }
            }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStatusFilter, value))
                {
                    _ = LoadClassesAsync();
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

        public int TotalClasses
        {
            get => _totalClasses;
            set => SetProperty(ref _totalClasses, value);
        }

        public bool HasSelection => Classes.Any(c => c.IsSelected);
        public int SelectedCount => Classes.Count(c => c.IsSelected);

        #endregion

        #region Commands

        public ICommand LoadClassesCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand ArchiveCommand { get; }
        public ICommand UnarchiveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand BulkArchiveCommand { get; }
        public ICommand BulkUnarchiveCommand { get; }
        public ICommand BulkDeleteCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        public AdminClassesViewModel(
            AdminClassService classService,
            NavigationService navigationService,
            IDialogCoordinator dialogCoordinator)
        {
            _classService = classService;
            _navigationService = navigationService;
            _dialogCoordinator = dialogCoordinator;

            LoadClassesCommand = new RelayCommand(async _ => await LoadClassesAsync());
            SearchCommand = new RelayCommand(async _ => await SearchClassesAsync());
            ViewDetailsCommand = new RelayCommand<ClassItem>(async cls => await ViewDetailsAsync(cls));
            ArchiveCommand = new RelayCommand<ClassItem>(async cls => await ArchiveClassAsync(cls));
            UnarchiveCommand = new RelayCommand<ClassItem>(async cls => await UnarchiveClassAsync(cls));
            DeleteCommand = new RelayCommand<ClassItem>(async cls => await DeleteClassAsync(cls));
            BulkArchiveCommand = new RelayCommand(async _ => await BulkArchiveAsync(), (Predicate<object>)(_ => HasSelection));
            BulkUnarchiveCommand = new RelayCommand(async _ => await BulkUnarchiveAsync(), (Predicate<object>)(_ => HasSelection));
            BulkDeleteCommand = new RelayCommand(async _ => await BulkDeleteAsync(), (Predicate<object>)(_ => HasSelection));
            NextPageCommand = new RelayCommand(async _ => await NextPageAsync(), (Predicate<object>)(_ => CurrentPage < TotalPages));
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPageAsync(), (Predicate<object>)(_ => CurrentPage > 1));
            RefreshCommand = new RelayCommand(async _ => await LoadClassesAsync());
        }

        public async Task InitializeAsync()
        {
            await LoadClassesAsync();
        }

        public async Task LoadClassesAsync()
        {
            IsLoading = true;

            try
            {
                bool? isArchived = SelectedStatusFilter switch
                {
                    "Đã lưu trữ" => true,
                    "Hoạt động" => false,
                    "Không hoạt động" => false,
                    _ => null
                };

                var result = await _classService.GetAllClassesAsync(
                    isArchived: isArchived,
                    searchTerm: string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                    pageNumber: CurrentPage,
                    pageSize: PageSize
                );

                if (result != null)
                {
                    Classes.Clear();
                    foreach (var classItem in result.Items)
                    {
                        var item = new ClassItem(classItem);
                        item.PropertyChanged += OnClassItemPropertyChanged;
                        Classes.Add(item);
                    }

                    TotalPages = result.TotalPages;
                    TotalClasses = result.TotalCount;
                }
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Lỗi",
                    $"Không thể tải danh sách lớp học: {ex.Message}",
                    MessageDialogStyle.Affirmative
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchClassesAsync()
        {
            CurrentPage = 1;
            await LoadClassesAsync();
        }

        private async Task ViewDetailsAsync(ClassItem? classItem)
        {
            if (classItem == null) return;

            await _dialogCoordinator.ShowMessageAsync(
                this,
                "Chi tiết lớp học",
                $"Class: {classItem.Name}\nMã lớp: {classItem.ClassCode}\nGiáo viên: {classItem.TeacherName}\nSố sinh viên: {classItem.StudentCount}",
                MessageDialogStyle.Affirmative
            );
        }

        private async Task ArchiveClassAsync(ClassItem? classItem)
        {
            if (classItem == null) return;

            var result = await _dialogCoordinator.ShowMessageAsync(
                this,
                "Xác nhận",
                $"Bạn có chắc muốn lưu trữ lớp học '{classItem.Name}'?",
                MessageDialogStyle.AffirmativeAndNegative
            );

            if (result == MessageDialogResult.Affirmative)
            {
                var success = await _classService.ArchiveClassAsync(classItem.ClassId);
                if (success)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Thành công",
                        "Lớp học đã được lưu trữ",
                        MessageDialogStyle.Affirmative
                    );
                    await LoadClassesAsync();
                }
            }
        }

        private async Task UnarchiveClassAsync(ClassItem? classItem)
        {
            if (classItem == null) return;

            var result = await _dialogCoordinator.ShowMessageAsync(
                this,
                "Xác nhận",
                $"Bạn có chắc muốn khôi phục lớp học '{classItem.Name}'?",
                MessageDialogStyle.AffirmativeAndNegative
            );

            if (result == MessageDialogResult.Affirmative)
            {
                var success = await _classService.UnarchiveClassAsync(classItem.ClassId);
                if (success)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Thành công",
                        "Lớp học đã được khôi phục",
                        MessageDialogStyle.Affirmative
                    );
                    await LoadClassesAsync();
                }
            }
        }

        private async Task DeleteClassAsync(ClassItem? classItem)
        {
            if (classItem == null) return;

            var result = await _dialogCoordinator.ShowMessageAsync(
                this,
                "Xác nhận xóa",
                $"Bạn có chắc muốn XÓA VĨNH VIỄN lớp học '{classItem.Name}'?\n\nHành động này không thể hoàn tác!",
                MessageDialogStyle.AffirmativeAndNegative
            );

            if (result == MessageDialogResult.Affirmative)
            {
                var success = await _classService.DeleteClassAsync(classItem.ClassId);
                if (success)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Thành công",
                        "Lớp học đã được xóa",
                        MessageDialogStyle.Affirmative
                    );
                    await LoadClassesAsync();
                }
            }
        }

        private async Task BulkArchiveAsync()
        {
            var selected = Classes.Where(c => c.IsSelected).ToList();
            if (!selected.Any()) return;

            var result = await _dialogCoordinator.ShowMessageAsync(
                this,
                "Xác nhận",
                $"Bạn có chắc muốn lưu trữ {selected.Count} lớp học đã chọn?",
                MessageDialogStyle.AffirmativeAndNegative
            );

            if (result == MessageDialogResult.Affirmative)
            {
                var classIds = selected.Select(c => c.ClassId).ToList();
                var bulkResult = await _classService.BulkActionAsync("archive", classIds);
                if (bulkResult != null)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Thành công",
                        $"Đã lưu trữ {selected.Count} lớp học",
                        MessageDialogStyle.Affirmative
                    );
                    await LoadClassesAsync();
                }
            }
        }

        private async Task BulkUnarchiveAsync()
        {
            var selected = Classes.Where(c => c.IsSelected).ToList();
            if (!selected.Any()) return;

            var result = await _dialogCoordinator.ShowMessageAsync(
                this,
                "Xác nhận",
                $"Bạn có chắc muốn khôi phục {selected.Count} lớp học đã chọn?",
                MessageDialogStyle.AffirmativeAndNegative
            );

            if (result == MessageDialogResult.Affirmative)
            {
                var classIds = selected.Select(c => c.ClassId).ToList();
                var bulkResult = await _classService.BulkActionAsync("unarchive", classIds);
                if (bulkResult != null)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Thành công",
                        $"Đã khôi phục {selected.Count} lớp học",
                        MessageDialogStyle.Affirmative
                    );
                    await LoadClassesAsync();
                }
            }
        }

        private async Task BulkDeleteAsync()
        {
            var selected = Classes.Where(c => c.IsSelected).ToList();
            if (!selected.Any()) return;

            var result = await _dialogCoordinator.ShowMessageAsync(
                this,
                "Xác nhận xóa",
                $"Bạn có chắc muốn XÓA VĨNH VIỄN {selected.Count} lớp học đã chọn?\n\nHành động này không thể hoàn tác!",
                MessageDialogStyle.AffirmativeAndNegative
            );

            if (result == MessageDialogResult.Affirmative)
            {
                var classIds = selected.Select(c => c.ClassId).ToList();
                var bulkResult = await _classService.BulkActionAsync("delete", classIds);
                if (bulkResult != null)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Thành công",
                        $"Đã xóa {selected.Count} lớp học",
                        MessageDialogStyle.Affirmative
                    );
                    await LoadClassesAsync();
                }
            }
        }

        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadClassesAsync();
            }
        }

        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadClassesAsync();
            }
        }

        private void OnClassItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ClassItem.IsSelected))
            {
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(SelectedCount));

                (BulkArchiveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (BulkUnarchiveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (BulkDeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
