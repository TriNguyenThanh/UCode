using System.Collections.ObjectModel;
using System.Linq;
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
        private readonly NavigationService _navigationService;
        private Class _classData;
        private bool _isLoading;
        private string _classId;

        public ClassDetailViewModel(ClassService classService, AssignmentService assignmentService, AuthService authService, NavigationService navigationService)
        {
            _classService = classService;
            _assignmentService = assignmentService;
            _authService = authService;
            _navigationService = navigationService;
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
                var currentUser = _authService.CurrentUser;
                ApiResponse<System.Collections.Generic.List<Assignment>> response = null;

                if (currentUser?.Role.ToString().ToLower() == "student")
                {
                    // Student: Get all student assignments and filter by class
                    response = await _assignmentService.GetStudentAssignmentsAsync();
                }
                else
                {
                    // Teacher/Admin: Get assignments for this class
                    response = await _assignmentService.GetAssignmentsByClassAsync(_classId);
                }

                if (response?.Success == true && response.Data != null)
                {
                    Assignments.Clear();
                    foreach (var assignment in response.Data)
                    {
                        // For students, we need to filter by classId
                        if (currentUser?.Role.ToString().ToLower() == "student")
                        {
                            if (assignment.ClassId == _classId)
                            {
                                Assignments.Add(assignment);
                            }
                        }
                        else
                        {
                            Assignments.Add(assignment);
                        }
                    }
                    OnPropertyChanged(nameof(TotalPoints));
                    OnPropertyChanged(nameof(PublishedCount));
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading assignments: {ex.Message}");
            }
        }

        private async void NavigateToAssignment(string assignmentId)
        {
            try
            {
                if (string.IsNullOrEmpty(assignmentId)) return;

                var assignment = Assignments.FirstOrDefault(a => a.AssignmentId == assignmentId);
                if (assignment != null && assignment.AssignmentType == AssignmentType.EXAMINATION)
                {
                    var result = await GetMetroWindow()?.ShowMessageAsync(
                       "Bài kiểm tra - Lưu ý quan trọng",
                       "Bài kiểm tra sẽ kiểm soát hành vi của bạn trong quá trình làm bài:\n" +
                       "- Hệ thống sẽ ghi lại số lần bạn chuyển tab hoặc rời khỏi màn hình làm bài\n" +
                       "- Mọi hoạt động bất thường sẽ được báo cáo cho giáo viên\n" +
                       "- Việc chuyển tab nhiều lần có thể ảnh hưởng đến kết quả của bạn\n\n" +
                       "Bạn có chắc chắn muốn bắt đầu làm bài kiểm tra này không?",
                       MessageDialogStyle.AffirmativeAndNegative,
                       new MetroDialogSettings
                       {
                           AffirmativeButtonText = "Xác nhận và bắt đầu",
                           NegativeButtonText = "Hủy",
                           DefaultButtonFocus = MessageDialogResult.Affirmative
                       });

                    if (result != MessageDialogResult.Affirmative)
                    {
                        return;
                    }
                }

                // Sử dụng Navigation thay vì mở window mới
                var assignmentDetailPage = new Views.Students.AssignmentDetailPage();
                _navigationService.NavigateTo(assignmentDetailPage, assignmentId);
            }
            catch (System.Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Đã xảy ra lỗi khi mở bài tập: {ex.Message}\n\nChi tiết:\n{ex.StackTrace}");
            }
        }

        private void NavigateBack()
        {
            _navigationService.GoBack();
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

