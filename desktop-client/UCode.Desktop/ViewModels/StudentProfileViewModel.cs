using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UCode.Desktop.Helpers;
using UCode.Desktop.Services;
using UCode.Desktop.Models;

namespace UCode.Desktop.ViewModels
{
    public class StudentProfileViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private readonly ApiService _apiService;
        private bool _isLoading;
        private string _fullName = string.Empty;
        private string _email = string.Empty;
        private string _studentCode = string.Empty;
        private string _major = string.Empty;
        private string _classYear = string.Empty;
        private string _enrollmentYear = string.Empty;
        private int _totalClasses;
        private int _completedAssignments;
        private int _totalAssignments;
        private int _problemsSolved;
        private int _totalProblems;
        private int _currentStreak;
        private string _rank = "Silver";
        private int _points = 1250;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                SetProperty(ref _fullName, value);
                OnPropertyChanged(nameof(FirstCharacter));
            }
        }

        public string FirstCharacter => !string.IsNullOrEmpty(FullName) ? FullName.Substring(0, 1).ToUpper() : "S";

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string StudentCode
        {
            get => _studentCode;
            set => SetProperty(ref _studentCode, value);
        }

        public string Major
        {
            get => _major;
            set => SetProperty(ref _major, value);
        }

        public string ClassYear
        {
            get => _classYear;
            set => SetProperty(ref _classYear, value);
        }

        public string EnrollmentYear
        {
            get => _enrollmentYear;
            set => SetProperty(ref _enrollmentYear, value);
        }

        public int TotalClasses
        {
            get => _totalClasses;
            set => SetProperty(ref _totalClasses, value);
        }

        public int CompletedAssignments
        {
            get => _completedAssignments;
            set
            {
                SetProperty(ref _completedAssignments, value);
                RecalculateCompletionRate();
            }
        }

        public int TotalAssignments
        {
            get => _totalAssignments;
            set
            {
                SetProperty(ref _totalAssignments, value);
                RecalculateCompletionRate();
            }
        }

        private void RecalculateCompletionRate()
        {
            OnPropertyChanged(nameof(CompletionRate));
        }

        public int ProblemsSolved
        {
            get => _problemsSolved;
            set
            {
                SetProperty(ref _problemsSolved, value);
                OnPropertyChanged(nameof(ProblemSolvedRate));
            }
        }

        public int TotalProblems
        {
            get => _totalProblems;
            set
            {
                SetProperty(ref _totalProblems, value);
                OnPropertyChanged(nameof(ProblemSolvedRate));
            }
        }

        public int CurrentStreak
        {
            get => _currentStreak;
            set => SetProperty(ref _currentStreak, value);
        }

        public string Rank
        {
            get => _rank;
            set => SetProperty(ref _rank, value);
        }

        public int Points
        {
            get => _points;
            set => SetProperty(ref _points, value);
        }

        public int CompletionRate => TotalAssignments > 0 ? (CompletedAssignments * 100 / TotalAssignments) : 0;
        public int ProblemSolvedRate => TotalProblems > 0 ? (ProblemsSolved * 100 / TotalProblems) : 0;

        public StudentProfileViewModel(AuthService authService, ApiService apiService)
        {
            _authService = authService;
            _apiService = apiService;

            var currentUser = authService.CurrentUser;
            Email = currentUser?.Email ?? string.Empty;
            FullName = currentUser?.Email?.Split('@')[0] ?? "Student";

            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
        }
        public ICommand RefreshCommand { get; }

        private async Task RefreshAsync()
        {
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                await LoadStudentProfileAsync();
                await LoadStatisticsAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadStudentProfileAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<StudentProfileResponse>("/api/v1/students/me");
                if (response?.Success == true && response.Data != null)
                {
                    FullName = response.Data.FullName ?? FullName;
                    Email = response.Data.Email ?? Email;
                    StudentCode = response.Data.StudentCode ?? "N/A";
                    Major = response.Data.Major ?? "N/A";
                    ClassYear = response.Data.ClassYear ?? "N/A";
                    EnrollmentYear = response.Data.EnrollmentYear?.ToString() ?? "N/A";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading student profile: {ex.Message}");
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                // TODO: Replace with real API when available
                // Mock data for now
                TotalClasses = 3;
                CompletedAssignments = 5;
                TotalAssignments = 8;
                ProblemsSolved = 23;
                TotalProblems = 50;
                CurrentStreak = 7;
                Rank = "Silver";
                Points = 1250;

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading statistics: {ex.Message}");
            }
        }
    }

    // Response models
    public class StudentProfileResponse
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public string StudentCode { get; set; }
        public string Email { get; set; }
        public string Major { get; set; }
        public string ClassYear { get; set; }
        public int? EnrollmentYear { get; set; }
    }
}
