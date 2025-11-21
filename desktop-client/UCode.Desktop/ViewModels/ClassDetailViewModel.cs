using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class ClassDetailViewModel : ViewModelBase
    {
        private readonly ClassService _classService;
        private readonly AssignmentService _assignmentService;
        private readonly AuthService _authService;
        private Class _classData;
        private bool _isLoading;
        private string _classId;

        public ClassDetailViewModel(ClassService classService, AssignmentService assignmentService, AuthService authService)
        {
            _classService = classService;
            _assignmentService = assignmentService;
            _authService = authService;
            _classData = new Class();
            
            NavigateToAssignmentCommand = new RelayCommand<string>(NavigateToAssignment);
            NavigateBackCommand = new RelayCommand(_ => NavigateBack());
        }

        public Class ClassData
        {
            get => _classData;
            set => SetProperty(ref _classData, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ObservableCollection<Assignment> Assignments { get; } = new();

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(Assignments))
            {
                OnPropertyChanged(nameof(TotalPoints));
                OnPropertyChanged(nameof(PublishedCount));
            }
        }

        public ICommand NavigateToAssignmentCommand { get; }
        public ICommand NavigateBackCommand { get; }

        public async Task InitializeAsync(string classId)
        {
            _classId = classId;
            IsLoading = true;
            try
            {
                await LoadClassDataAsync();
                await LoadAssignmentsAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadClassDataAsync()
        {
            try
            {
                var response = await _classService.GetClassByIdAsync(_classId);
                if (response?.Success == true && response.Data != null)
                {
                    ClassData = response.Data;
                }
                else
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Không thể tải thông tin lớp học: {response?.Message}");
                }
            }
            catch (System.Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Lỗi khi tải lớp học: {ex.Message}");
            }
        }

        private async Task LoadAssignmentsAsync()
        {
            try
            {
                var response = await _assignmentService.GetAssignmentsByClassAsync(_classId);
                if (response?.Success == true && response.Data != null)
                {
                    Assignments.Clear();
                    foreach (var assignment in response.Data)
                    {
                        Assignments.Add(assignment);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading assignments: {ex.Message}");
            }
        }

        private void NavigateToAssignment(string assignmentId)
        {
            if (string.IsNullOrEmpty(assignmentId)) return;

            var assignmentWindow = App.ServiceProvider.GetService(typeof(Views.AssignmentDetailWindow)) as Views.AssignmentDetailWindow;
            if (assignmentWindow != null)
            {
                var viewModel = assignmentWindow.DataContext as AssignmentDetailViewModel;
                if (viewModel != null)
                {
                    _ = viewModel.InitializeAsync(assignmentId);
                    assignmentWindow.Show();
                }
            }
        }

        private void NavigateBack()
        {
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window is Views.ClassDetailWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        public int GetDaysUntilDue(DateTime? endTime)
        {
            if (!endTime.HasValue) return 0;
            var now = System.DateTime.Now;
            var diff = endTime.Value - now;
            return diff.Days > 0 ? diff.Days : 0;
        }

        public string GetStatusLabel(Assignment assignment)
        {
            switch (assignment.Status)
            {
                case AssignmentStatus.DRAFT:
                    return "Bản nháp";
                case AssignmentStatus.PUBLISHED:
                    var daysLeft = GetDaysUntilDue(assignment.EndTime);
                    if (daysLeft < 0) return "Quá hạn";
                    if (daysLeft == 0) return "Hết hạn hôm nay";
                    if (daysLeft == 1) return "Còn 1 ngày";
                    if (daysLeft <= 3) return $"Còn {daysLeft} ngày";
                    if (daysLeft <= 7) return $"Còn {daysLeft} ngày";
                    return $"Còn {daysLeft} ngày";
                case AssignmentStatus.CLOSED:
                    return "Đã đóng";
                default:
                    return assignment.Status.ToString();
            }
        }

        public string GetStatusColor(Assignment assignment)
        {
            switch (assignment.Status)
            {
                case AssignmentStatus.DRAFT:
                    return "#9E9E9E";
                case AssignmentStatus.PUBLISHED:
                    var daysLeft = GetDaysUntilDue(assignment.EndTime);
                    if (daysLeft < 0) return "#F44336";
                    if (daysLeft <= 3) return "#F44336";
                    if (daysLeft <= 7) return "#FF9800";
                    return "#2196F3";
                case AssignmentStatus.CLOSED:
                    return "#F44336";
                default:
                    return "#9E9E9E";
            }
        }

        public int TotalPoints
        {
            get
            {
                int sum = 0;
                foreach (var assignment in Assignments)
                {
                    sum += assignment.TotalPoints;
                }
                return sum;
            }
        }

        public int PublishedCount
        {
            get
            {
                int count = 0;
                foreach (var assignment in Assignments)
                {
                    if (assignment.Status == AssignmentStatus.PUBLISHED)
                        count++;
                }
                return count;
            }
        }
    }
}

