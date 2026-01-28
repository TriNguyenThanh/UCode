using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using UCode.Desktop.Models;
using UCode.Desktop.Services;
using UCode.Desktop.Views.Students;
using UCode.Desktop.Helpers;

namespace UCode.Desktop.ViewModels.Students
{
    public class StudentAssignmentsViewModel : ViewModelBase
    {
        private readonly AssignmentService _assignmentService;
        private bool _isLoading;
        private ObservableCollection<Assignment> _assignments;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public ObservableCollection<Assignment> Assignments
        {
            get => _assignments;
            set
            {
                if (SetProperty(ref _assignments, value))
                {
                    OnPropertyChanged(nameof(Assignments));
                }
            }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand OpenAssignmentCommand { get; }
        public ICommand RefreshCommand { get; }

        private readonly NavigationService _navigationService;
        private readonly AIDetectorService _aiDetectorService;

        public StudentAssignmentsViewModel()
        {
            // Resolve service manually since we are creating this VM dynamically or via DI
            _assignmentService = new AssignmentService(App.ServiceProvider.GetService(typeof(ApiService)) as ApiService);
            _navigationService = App.ServiceProvider.GetService(typeof(NavigationService)) as NavigationService;
            _aiDetectorService = App.ServiceProvider.GetService(typeof(AIDetectorService)) as AIDetectorService;

            Assignments = new ObservableCollection<Assignment>();

            LoadDataCommand = new RelayCommand(async _ => await LoadAssignmentsAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadAssignmentsAsync());
            OpenAssignmentCommand = new RelayCommand(OpenAssignment);

            // Initial load
            _ = LoadAssignmentsAsync();
        }

        private async Task LoadAssignmentsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                Assignments.Clear();

                var response = await _assignmentService.GetStudentAssignmentsAsync();

                if (response.Success && response.Data != null)
                {
                    foreach (var assignment in response.Data)
                    {
                        Assignments.Add(assignment);
                    }
                }
                else
                {
                    Debug.WriteLine($"Failed to load assignments: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading assignments: {ex.Message}");
                await ShowMessageAsync("Lỗi", $"Không thể tải danh sách bài tập: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void OpenAssignment(object parameter)
        {
            if (parameter is Assignment assignment)
            {
                try
                {
                    if (_navigationService == null)
                    {
                        await ShowMessageAsync("Lỗi", "Dịch vụ điều hướng chưa được khởi tạo.");
                        return;
                    }

                    // Fetch fresh assignment data to check status
                    var response = await _assignmentService.GetAssignmentAsync(assignment.AssignmentId);
                    if (!response.Success || response.Data == null)
                    {
                        await ShowMessageAsync("Lỗi", $"Không thể tải thông tin bài tập: {response.Message}");
                        return;
                    }

                    var freshAssignment = response.Data;

                    // Check if assignment is closed
                    if (freshAssignment.Status == AssignmentStatus.CLOSED)
                    {
                        await ShowMessageAsync("Thông báo", "Bài tập này đã đóng, bạn không thể truy cập.");
                        return;
                    }

                    // Check for AI Detector if Examination
                    if (freshAssignment.AssignmentType == AssignmentType.EXAMINATION)
                    {
                        if (_aiDetectorService != null)
                        {
                            if (await _aiDetectorService.ConfirmMessageAIDetector(assignment.AssignmentId) == false)
                            {
                                return;
                            }
                            _aiDetectorService.StartAutoMonitor();
                        }
                    }

                    var page = new Views.Students.AssignmentDetailPage();
                    _navigationService.NavigateTo(page, assignment.AssignmentId);
                }
                catch (Exception ex)
                {
                    await ShowMessageAsync("Lỗi", $"Không thể mở bài tập: {ex.Message}");
                }
            }
        }
    }
}
