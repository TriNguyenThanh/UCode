using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class StudentItemViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public int EnrollmentYear { get; set; }
        public string Status { get; set; } = "Active";

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class YearFilterOption
    {
        public string Label { get; set; } = "Tất cả";
        public string Value { get; set; } = "";
    }

    public class VisualSelectTabViewModel : ViewModelBase
    {
        private readonly ClassService _classService;

        private string _classId = string.Empty;
        private string _searchText = string.Empty;
        private string _majorFilter = string.Empty;
        private string _statusFilter = "";
        private YearFilterOption? _yearFilter;
        private bool _allSelected;
        private bool _isLoading;
        private string? _errorMessage;
        private int _page = 1;
        private int _pageSize = 10;
        private int _totalCount;

        public ObservableCollection<StudentItemViewModel> Students { get; } = new();
        public ObservableCollection<StudentItemViewModel> FilteredStudents { get; } = new();
        public ObservableCollection<YearFilterOption> Years { get; } = new();

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyClientFilters();
                }
            }
        }

        public string MajorFilter
        {
            get => _majorFilter;
            set
            {
                if (SetProperty(ref _majorFilter, value))
                {
                    _page = 1;
                    _ = LoadStudentsAsync();
                }
            }
        }

        public string StatusFilter
        {
            get => _statusFilter;
            set
            {
                if (SetProperty(ref _statusFilter, value))
                {
                    _page = 1;
                    _ = LoadStudentsAsync();
                }
            }
        }

        public YearFilterOption? YearFilter
        {
            get => _yearFilter;
            set
            {
                if (SetProperty(ref _yearFilter, value))
                {
                    _page = 1;
                    _ = LoadStudentsAsync();
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
                    foreach (var student in FilteredStudents)
                    {
                        student.IsSelected = value;
                    }
                    OnPropertyChanged(nameof(SelectedCount));
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (SetProperty(ref _pageSize, value))
                {
                    _page = 1;
                    _ = LoadStudentsAsync();
                }
            }
        }

        public int SelectedCount => FilteredStudents.Count(s => s.IsSelected);
        public bool HasStudents => FilteredStudents.Count > 0 && !IsLoading;
        public bool HasNoStudents => FilteredStudents.Count == 0 && !IsLoading;
        public bool CanAddStudents => SelectedCount > 0 && !IsLoading;
        public string AddButtonText => IsLoading ? "Đang xử lý..." : $"Thêm {SelectedCount} sinh viên";
        public string PaginationText => $"{(_page - 1) * _pageSize + 1}-{Math.Min(_page * _pageSize, _totalCount)} trong {_totalCount}";
        public bool CanGoPrevious => _page > 1;
        public bool CanGoNext => _page * _pageSize < _totalCount;

        public ICommand AddStudentsCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }

        public VisualSelectTabViewModel(ClassService classService)
        {
            _classService = classService;

            AddStudentsCommand = new RelayCommand(async _ => await AddStudentsAsync());
            PreviousPageCommand = new RelayCommand(_ => PreviousPage());
            NextPageCommand = new RelayCommand(_ => NextPage());

            // Populate years
            var currentYear = DateTime.Now.Year;
            Years.Add(new YearFilterOption { Label = "Tất cả", Value = "" });
            for (int i = 0; i < 10; i++)
            {
                var year = currentYear - i;
                Years.Add(new YearFilterOption { Label = year.ToString(), Value = year.ToString() });
            }
        }

        public void Initialize(string classId)
        {
            _classId = classId;
            _ = LoadStudentsAsync();
        }

        private async Task LoadStudentsAsync()
        {
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                // TODO: Call API to load students
                // var response = await _studentService.GetAvailableStudentsAsync(...);
                
                // Mock data for now
                Students.Clear();
                FilteredStudents.Clear();
                
                // TODO: Replace with actual API call
                // For now, show empty state
                
                ApplyClientFilters();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi tải danh sách: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyClientFilters()
        {
            FilteredStudents.Clear();

            var query = Students.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(s =>
                    s.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    s.StudentCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var student in query)
            {
                FilteredStudents.Add(student);
            }

            OnPropertyChanged(nameof(HasStudents));
            OnPropertyChanged(nameof(HasNoStudents));
            OnPropertyChanged(nameof(SelectedCount));
        }

        private void PreviousPage()
        {
            if (_page > 1)
            {
                _page--;
                _ = LoadStudentsAsync();
            }
        }

        private void NextPage()
        {
            if (_page * _pageSize < _totalCount)
            {
                _page++;
                _ = LoadStudentsAsync();
            }
        }

        private async Task AddStudentsAsync()
        {
            // TODO: Implement bulk add students to class
            var selectedIds = FilteredStudents.Where(s => s.IsSelected).Select(s => s.UserId).ToList();
            
            if (selectedIds.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 sinh viên", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Call ClassService.BulkEnrollStudentsAsync
            MessageBox.Show(
                $"Add Students Implementation\n\n" +
                $"Selected: {selectedIds.Count} students\n\n" +
                "TODO: Call ClassService.BulkEnrollStudentsAsync\n" +
                "See: client/app/services/classService.ts (bulkEnrollStudents)",
                "Feature Not Implemented",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            await Task.CompletedTask;
        }
    }
}

