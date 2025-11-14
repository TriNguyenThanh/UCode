                                                                                                                                                                                                using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public event PropertyChangedEventHandler PropertyChanged;
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
        private string _yearFilter = string.Empty;
        private string _majorFilter = string.Empty;
        private string _statusFilter = "";
        private bool _allSelected;
        private bool _isLoading;
        private bool _isAdding;
        private string _errorMessage;
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
                    ApplyClientSearchFilter();
                }
            }
        }

        public string YearFilter
        {
            get => _yearFilter;
            set
            {
                if (SetProperty(ref _yearFilter, value))
                {
                    _page = 1; // Reset to first page
                    _ = LoadStudentsAsync(); // Reload from server
                }
            }
        }

        public string MajorFilter
        {
            get => _majorFilter;
            set
            {
                if (SetProperty(ref _majorFilter, value ?? string.Empty))
                {
                    _page = 1; // Reset to first page
                    _ = LoadStudentsAsync(); // Reload from server
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
                    _page = 1; // Reset to first page
                    _ = LoadStudentsAsync(); // Reload from server
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

        public bool IsAdding
        {
            get => _isAdding;
            set
            {
                if (SetProperty(ref _isAdding, value))
                {
                    OnPropertyChanged(nameof(CanAddStudents));
                    OnPropertyChanged(nameof(AddButtonText));
                }
            }
        }

        public string ErrorMessage
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
        public bool CanAddStudents => SelectedCount > 0 && !IsAdding;
        public string AddButtonText => IsAdding ? "Đang thêm..." : $"Thêm sinh viên";
        public string PaginationText => $"{(_page - 1) * _pageSize + 1}-{Math.Min(_page * _pageSize, _totalCount)} trong {_totalCount}";
        public bool CanGoPrevious => _page > 1;
        public bool CanGoNext => _page * _pageSize < _totalCount;

        public ICommand AddStudentsCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }

        public VisualSelectTabViewModel(ClassService classService)
        {
            _classService = classService;

            AddStudentsCommand = new RelayCommand(async _ => await AddStudentsAsync(), _ => CanAddStudents);
            PreviousPageCommand = new RelayCommand(_ => PreviousPage(), _ => CanGoPrevious);
            NextPageCommand = new RelayCommand(_ => NextPage(), _ => CanGoNext);

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
            ErrorMessage = string.Empty;

            try
            {
                System.Diagnostics.Debug.WriteLine($"LoadStudentsAsync: Loading students for class {_classId}, page {_page}, pageSize {_pageSize}");
                
                // Use paged API with filters (like web version)
                int? yearFilter = null;
                if (!string.IsNullOrEmpty(_yearFilter) && int.TryParse(_yearFilter, out int year))
                {
                    yearFilter = year;
                }

                var response = await _classService.GetAvailableStudentsPagedAsync(
                    pageNumber: _page,
                    pageSize: _pageSize,
                    excludeClassId: _classId,
                    year: yearFilter,
                    major: string.IsNullOrWhiteSpace(_majorFilter) ? null : _majorFilter.Trim(),
                    status: string.IsNullOrEmpty(_statusFilter) ? null : _statusFilter
                );
                
                System.Diagnostics.Debug.WriteLine($"LoadStudentsAsync: Response Success={response?.Success}, Items Count={response?.Data?.Items?.Count ?? 0}, Total={response?.Data?.TotalCount ?? 0}");
                
                Students.Clear();
                
                if (response?.Success == true && response.Data != null)
                {
                    _totalCount = response.Data.TotalCount;
                    
                    if (response.Data.Items != null && response.Data.Items.Count > 0)
                    {
                        foreach (var student in response.Data.Items)
                        {
                            var studentItem = new StudentItemViewModel
                            {
                                UserId = student.UserId,
                                FullName = student.FullName,
                                StudentCode = student.StudentCode ?? "N/A",
                                Email = student.Email,
                                Major = student.Major ?? "N/A",
                                EnrollmentYear = student.EnrollmentYear ?? DateTime.Now.Year,
                                Status = student.Status ?? "Active"
                            };
                            
                            // Subscribe to property changes to update SelectedCount
                            studentItem.PropertyChanged += Student_PropertyChanged;
                            
                            Students.Add(studentItem);
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"LoadStudentsAsync: Added {Students.Count} students to Students collection");
                    }
                    else
                    {
                        FilteredStudents.Clear();
                        ErrorMessage = "Không có sinh viên nào khả dụng";
                    }
                }
                else
                {
                    FilteredStudents.Clear();
                    System.Diagnostics.Debug.WriteLine($"LoadStudentsAsync: Failed - Message: {response?.Message}");
                    ErrorMessage = $"Lỗi tải danh sách: {response?.Message ?? "Unknown error"}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi tải danh sách: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading students: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
                
                // Apply filter and notify UI after IsLoading is set to false
                if (Students.Count > 0)
                {
                    ApplyClientSearchFilter();
                }
                
                OnPropertyChanged(nameof(HasStudents));
                OnPropertyChanged(nameof(HasNoStudents));
                OnPropertyChanged(nameof(SelectedCount));
                OnPropertyChanged(nameof(PaginationText));
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(CanGoNext));
            }
        }

        private void ApplyClientSearchFilter()
        {
            System.Diagnostics.Debug.WriteLine($"ApplyClientSearchFilter START: Students.Count={Students.Count}, SearchText='{SearchText}'");
            
            // Client-side search filtering on loaded data
            FilteredStudents.Clear();

            var query = Students.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(s =>
                    s.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    s.StudentCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var student in query)
            {
                // Subscribe to property changes for filtered students too
                student.PropertyChanged -= Student_PropertyChanged; // Remove old subscription
                student.PropertyChanged += Student_PropertyChanged;
                FilteredStudents.Add(student);
            }

            System.Diagnostics.Debug.WriteLine($"ApplyClientSearchFilter END: FilteredStudents.Count={FilteredStudents.Count}, HasStudents={HasStudents}, IsLoading={IsLoading}");

            OnPropertyChanged(nameof(HasStudents));
            OnPropertyChanged(nameof(HasNoStudents));
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(CanAddStudents));
        }

        private void Student_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StudentItemViewModel.IsSelected))
            {
                OnPropertyChanged(nameof(SelectedCount));
                OnPropertyChanged(nameof(CanAddStudents));
            }
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
            IsAdding = true;
            ErrorMessage = string.Empty;

            try
            {
                var selectedIds = FilteredStudents.Where(s => s.IsSelected).Select(s => s.UserId).ToList();
                
                if (selectedIds.Count == 0)
                {
                    await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng chọn ít nhất một sinh viên");
                    return;
                }

                var response = await _classService.BulkEnrollStudentsAsync(_classId, selectedIds);
                
                if (response?.Success == true)
                {
                    var successMsg = $"Đã thêm thành công {selectedIds.Count} sinh viên vào lớp!";
                                        
                    await GetMetroWindow()?.ShowMessageAsync("Thành công", successMsg);
                    
                    await LoadStudentsAsync();
                }
                else
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Thêm sinh viên thất bại: {response?.Message}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi thêm sinh viên: {ex.Message}";
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", ErrorMessage);
            }
            finally
            {
                IsAdding = false;
            }
        }
    }
}
