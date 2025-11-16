using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models.Admin;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels.Admin
{
    public class AdminClassesViewModel : INotifyPropertyChanged
    {
        private readonly AdminService _adminService;
        private ObservableCollection<ClassManagement> _classes = new();
        private ObservableCollection<UserManagement> _teachers = new();
        private ClassManagement? _selectedClass;
        private string _searchText = string.Empty;
        private Guid? _selectedTeacherFilter;
        private bool? _selectedStatusFilter;
        private bool _isLoading;
        private int _currentPage = 1;
        private int _pageSize = 20;

        public AdminClassesViewModel(AdminService adminService)
        {
            _adminService = adminService;

            SearchCommand = new RelayCommand(async _ => await SearchClassesAsync());
            CreateClassCommand = new RelayCommand(_ => CreateClass());
            EditClassCommand = new RelayCommand(async _ => await EditClassAsync(), _ => SelectedClass != null);
            DeleteClassCommand = new RelayCommand(async _ => await DeleteClassAsync(), _ => SelectedClass != null);
            ViewStatisticsCommand = new RelayCommand(async _ => await ViewStatisticsAsync(), _ => SelectedClass != null);
            ReassignTeacherCommand = new RelayCommand(async _ => await ReassignTeacherAsync(), _ => SelectedClass != null);
            RefreshCommand = new RelayCommand(async _ => await LoadClassesAsync());
            ExportCommand = new RelayCommand(async _ => await ExportClassesAsync());
            NextPageCommand = new RelayCommand(async _ => await NextPageAsync());
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPageAsync(), _ => CurrentPage > 1);
        }

        #region Properties

        public ObservableCollection<ClassManagement> Classes
        {
            get => _classes;
            set
            {
                _classes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ClassesCount));
            }
        }

        public int ClassesCount => _classes?.Count ?? 0;

        public ObservableCollection<UserManagement> Teachers
        {
            get => _teachers;
            set
            {
                _teachers = value;
                OnPropertyChanged();
            }
        }

        public ClassManagement? SelectedClass
        {
            get => _selectedClass;
            set
            {
                _selectedClass = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public Guid? SelectedTeacherFilter
        {
            get => _selectedTeacherFilter;
            set
            {
                _selectedTeacherFilter = value;
                OnPropertyChanged();
            }
        }

        public bool? SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                _selectedStatusFilter = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand SearchCommand { get; }
        public ICommand CreateClassCommand { get; }
        public ICommand EditClassCommand { get; }
        public ICommand DeleteClassCommand { get; }
        public ICommand ViewStatisticsCommand { get; }
        public ICommand ReassignTeacherCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        #endregion

        #region Methods

        public async Task LoadClassesAsync()
        {
            await LoadTeachersAsync();
            await SearchClassesAsync();
        }

        private async Task LoadTeachersAsync()
        {
            try
            {
                // Load all teachers for filter dropdown
                var users = await _adminService.GetAllUsersAsync(role: "Teacher", pageSize: 100);
                Teachers = new ObservableCollection<UserManagement>(users);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load teachers: {ex.Message}");
            }
        }

        private async Task SearchClassesAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine($"[AdminClassesViewModel] Loading classes - Page: {CurrentPage}, Search: '{SearchText}'");
                var classes = await _adminService.GetAllClassesAsync(
                    SearchText,
                    SelectedTeacherFilter,
                    SelectedStatusFilter,
                    CurrentPage,
                    PageSize);

                Classes = new ObservableCollection<ClassManagement>(classes);
                System.Diagnostics.Debug.WriteLine($"[AdminClassesViewModel] Loaded {classes.Count} classes");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminClassesViewModel] Error: {ex}");
                ModernMessageBox.Show(
                    $"Không thể tải danh sách lớp học.\n\nLỗi: {ex.Message}\n\nVui lòng kiểm tra backend đang chạy.",
                    "Lỗi Tải Dữ Liệu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CreateClass()
        {
            var dialog = new Views.Dialogs.ClassCreateDialog(_adminService);
            if (dialog.ShowDialog() == true)
            {
                // Refresh the list after successful creation
                _ = LoadClassesAsync();
            }
        }

        private async Task EditClassAsync()
        {
            if (SelectedClass == null) return;

            var dialog = new Views.Dialogs.ClassEditDialog(_adminService, SelectedClass);
            if (dialog.ShowDialog() == true)
            {
                // Refresh the list after successful edit
                await LoadClassesAsync();
            }
        }

        private async Task DeleteClassAsync()
        {
            if (SelectedClass == null) return;

            var result = ModernMessageBox.Show(
                $"Are you sure you want to delete class '{SelectedClass.Name}'?\n" +
                $"This will affect {SelectedClass.StudentCount} students and {SelectedClass.AssignmentCount} assignments.\n" +
                "This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    var success = await _adminService.DeleteClassAsync(SelectedClass.Id);
                    
                    if (success)
                    {
                        ModernMessageBox.Show(
                            "Class deleted successfully",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        await LoadClassesAsync();
                    }
                    else
                    {
                        ModernMessageBox.Show(
                            "Failed to delete class",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    ModernMessageBox.Show(
                        $"Error deleting class: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task ViewStatisticsAsync()
        {
            if (SelectedClass == null) return;

            try
            {
                IsLoading = true;
                var stats = await _adminService.GetClassStatisticsAsync(SelectedClass.Id);
                
                if (stats != null)
                {
                    // TODO: Show statistics in a dialog or navigate to statistics page
                    ModernMessageBox.Show(
                        $"Class: {stats.ClassName}\n" +
                        $"Total Students: {stats.TotalStudents}\n" +
                        $"Active Students: {stats.ActiveStudents}\n" +
                        $"Total Assignments: {stats.TotalAssignments}\n" +
                        $"Completed: {stats.CompletedAssignments}\n" +
                        $"Average Score: {stats.AverageScore:F2}\n" +
                        $"Completion Rate: {stats.CompletionRate:F1}%",
                        "Class Statistics",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(
                    $"Failed to load statistics: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ReassignTeacherAsync()
        {
            if (SelectedClass == null) return;

            // TODO: Open teacher selection dialog
            ModernMessageBox.Show(
                $"Reassign teacher dialog will be implemented for: {SelectedClass.Name}",
                "Info",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async Task ExportClassesAsync()
        {
            try
            {
                IsLoading = true;
                var data = await _adminService.ExportClassesToExcelAsync();
                
                if (data != null && data.Length > 0)
                {
                    // Show save file dialog
                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel Files (*.xlsx)|*.xlsx",
                        FileName = $"Classes_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                        DefaultExt = ".xlsx"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        await System.IO.File.WriteAllBytesAsync(saveFileDialog.FileName, data);
                        
                        ModernMessageBox.Show(
                            $"Classes exported successfully to:\n{saveFileDialog.FileName}",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                else
                {
                    ModernMessageBox.Show(
                        "No data to export",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(
                    $"Export failed: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task NextPageAsync()
        {
            CurrentPage++;
            await SearchClassesAsync();
        }

        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await SearchClassesAsync();
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
