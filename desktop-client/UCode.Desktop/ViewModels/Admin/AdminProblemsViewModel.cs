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
using UCode.Desktop.Models.Enums;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels.Admin
{
    public class AdminProblemsViewModel : INotifyPropertyChanged
    {
        private readonly AdminService _adminService;
        private ObservableCollection<ProblemManagement> _problems = new();
        private ProblemManagement? _selectedProblem;
        private string _searchText = string.Empty;
        private Difficulty? _selectedDifficultyFilter;
        private AdminProblemStatus? _selectedStatusFilter;
        private ProblemVisibility? _selectedVisibilityFilter;
        private bool _isLoading;
        private int _currentPage = 1;
        private int _pageSize = 20;

        public AdminProblemsViewModel(AdminService adminService)
        {
            _adminService = adminService;

            SearchCommand = new RelayCommand(async _ => await SearchProblemsAsync());
            ApproveProblemCommand = new RelayCommand(async _ => await ApproveProblemAsync(), _ => SelectedProblem != null);
            RejectProblemCommand = new RelayCommand(async _ => await RejectProblemAsync(), _ => SelectedProblem != null);
            DeleteProblemCommand = new RelayCommand(async _ => await DeleteProblemAsync(), _ => SelectedProblem != null);
            ChangeVisibilityCommand = new RelayCommand(async _ => await ChangeVisibilityAsync(), _ => SelectedProblem != null);
            RefreshCommand = new RelayCommand(async _ => await LoadProblemsAsync());
            ExportCommand = new RelayCommand(async _ => await ExportProblemsAsync());
            NextPageCommand = new RelayCommand(async _ => await NextPageAsync());
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPageAsync(), _ => CurrentPage > 1);
        }

        #region Properties

        public ObservableCollection<ProblemManagement> Problems
        {
            get => _problems;
            set
            {
                _problems = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProblemsCount));
            }
        }

        public int ProblemsCount => _problems?.Count ?? 0;

        public ProblemManagement? SelectedProblem
        {
            get => _selectedProblem;
            set
            {
                _selectedProblem = value;
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

        public Difficulty? SelectedDifficultyFilter
        {
            get => _selectedDifficultyFilter;
            set
            {
                _selectedDifficultyFilter = value;
                OnPropertyChanged();
            }
        }

        public AdminProblemStatus? SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                _selectedStatusFilter = value;
                OnPropertyChanged();
            }
        }

        public ProblemVisibility? SelectedVisibilityFilter
        {
            get => _selectedVisibilityFilter;
            set
            {
                _selectedVisibilityFilter = value;
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
        public ICommand ApproveProblemCommand { get; }
        public ICommand RejectProblemCommand { get; }
        public ICommand DeleteProblemCommand { get; }
        public ICommand ChangeVisibilityCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        #endregion

        #region Methods

        public async Task LoadProblemsAsync()
        {
            await SearchProblemsAsync();
        }

        private async Task SearchProblemsAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine($"[AdminProblemsViewModel] Loading problems - Page: {CurrentPage}, Search: '{SearchText}'");
                var problems = await _adminService.GetAllProblemsAsync(
                    SearchText,
                    SelectedDifficultyFilter?.ToString(),
                    SelectedStatusFilter,
                    SelectedVisibilityFilter,
                    CurrentPage,
                    PageSize);

                Problems = new ObservableCollection<ProblemManagement>(problems);
                System.Diagnostics.Debug.WriteLine($"[AdminProblemsViewModel] Loaded {problems.Count} problems");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminProblemsViewModel] Error: {ex}");
                ModernMessageBox.Show(
                    $"Không thể tải danh sách bài tập.\n\nLỗi: {ex.Message}\n\nVui lòng kiểm tra backend đang chạy.",
                    "Lỗi Tải Dữ Liệu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ApproveProblemAsync()
        {
            if (SelectedProblem == null) return;

            var result = ModernMessageBox.Show(
                $"Approve problem '{SelectedProblem.Title}'?",
                "Confirm Approval",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    var request = new ApproveProblemRequest
                    {
                        ProblemId = SelectedProblem.Id,
                        IsApproved = true,
                        ReviewNotes = "Approved by admin"
                    };

                    var success = await _adminService.ApproveProblemAsync(request);
                    
                    if (success)
                    {
                        ModernMessageBox.Show(
                            "Problem approved successfully",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        await LoadProblemsAsync();
                    }
                    else
                    {
                        ModernMessageBox.Show(
                            "Failed to approve problem",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    ModernMessageBox.Show(
                        $"Error approving problem: {ex.Message}",
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

        private async Task RejectProblemAsync()
        {
            if (SelectedProblem == null) return;

            // TODO: Open dialog to get rejection reason
            var result = ModernMessageBox.Show(
                $"Reject problem '{SelectedProblem.Title}'?",
                "Confirm Rejection",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    var request = new ApproveProblemRequest
                    {
                        ProblemId = SelectedProblem.Id,
                        IsApproved = false,
                        ReviewNotes = "Rejected by admin" // TODO: Get from dialog
                    };

                    var success = await _adminService.ApproveProblemAsync(request);
                    
                    if (success)
                    {
                        ModernMessageBox.Show(
                            "Problem rejected",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        await LoadProblemsAsync();
                    }
                    else
                    {
                        ModernMessageBox.Show(
                            "Failed to reject problem",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    ModernMessageBox.Show(
                        $"Error rejecting problem: {ex.Message}",
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

        private async Task DeleteProblemAsync()
        {
            if (SelectedProblem == null) return;

            var result = ModernMessageBox.Show(
                $"Are you sure you want to delete problem '{SelectedProblem.Title}'?\n" +
                $"This problem has {SelectedProblem.TotalSubmissions} submissions.\n" +
                "This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    var success = await _adminService.DeleteProblemAsync(SelectedProblem.Id);
                    
                    if (success)
                    {
                        ModernMessageBox.Show(
                            "Problem deleted successfully",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        await LoadProblemsAsync();
                    }
                    else
                    {
                        ModernMessageBox.Show(
                            "Failed to delete problem",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    ModernMessageBox.Show(
                        $"Error deleting problem: {ex.Message}",
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

        private async Task ChangeVisibilityAsync()
        {
            if (SelectedProblem == null) return;

            // TODO: Open visibility selection dialog
            ModernMessageBox.Show(
                $"Change visibility dialog will be implemented for: {SelectedProblem.Title}",
                "Info",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async Task ExportProblemsAsync()
        {
            try
            {
                IsLoading = true;
                var data = await _adminService.ExportProblemsToExcelAsync();
                
                if (data != null)
                {
                    // TODO: Save file dialog
                    ModernMessageBox.Show(
                        "Problems exported successfully",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
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
            await SearchProblemsAsync();
        }

        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await SearchProblemsAsync();
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
