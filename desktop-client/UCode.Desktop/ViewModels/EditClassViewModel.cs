using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UCode.Desktop.Models;
using UCode.Desktop.Models.Admin;
using UCode.Desktop.Services;
using UCode.Desktop.Services.Admin;

namespace UCode.Desktop.ViewModels
{
    public class EditClassViewModel : INotifyPropertyChanged
    {
        private readonly ClassService _classService;
        private readonly AdminClassService? _adminClassService;
        private bool _isAdminMode;

        private string _classId = string.Empty;
        private string _className = string.Empty;
        private string _classCode = string.Empty;
        private string _description = string.Empty;
        private string _subject = string.Empty;
        private string _semester = string.Empty;
        private string _academicYear = string.Empty;
        private bool _isActive = true;
        private bool _isLoading;
        private string? _errorMessage;

        public event Action<bool>? CloseRequested;
        public event PropertyChangedEventHandler? PropertyChanged;

        public EditClassViewModel(ClassService classService)
        {
            _classService = classService;
            _isAdminMode = false;
        }

        public EditClassViewModel(AdminClassService adminClassService)
        {
            _classService = null!;
            _adminClassService = adminClassService;
            _isAdminMode = true;
        }

        #region Properties

        public string ClassId
        {
            get => _classId;
            set => SetProperty(ref _classId, value);
        }

        public string ClassName
        {
            get => _className;
            set
            {
                if (SetProperty(ref _className, value))
                    OnPropertyChanged(nameof(CanSave));
            }
        }

        public string ClassCode
        {
            get => _classCode;
            set => SetProperty(ref _classCode, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string Subject
        {
            get => _subject;
            set => SetProperty(ref _subject, value);
        }

        public string Semester
        {
            get => _semester;
            set => SetProperty(ref _semester, value);
        }

        public string AcademicYear
        {
            get => _academicYear;
            set => SetProperty(ref _academicYear, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
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

        public bool CanSave => !string.IsNullOrWhiteSpace(ClassName);

        #endregion

        public void LoadFromClass(Class classInfo)
        {
            if (classInfo == null) return;

            ClassId = classInfo.ClassId;
            ClassName = classInfo.ClassName ?? string.Empty;
            ClassCode = string.Empty; // Class model might not have ClassCode
            Description = classInfo.Description ?? string.Empty;
            Subject = classInfo.Subject ?? string.Empty;
            Semester = classInfo.Semester ?? string.Empty;
            AcademicYear = classInfo.AcademicYear ?? string.Empty;
            IsActive = classInfo.IsActive;

            // Force UI update
            OnPropertyChanged(nameof(ClassId));
            OnPropertyChanged(nameof(ClassName));
            OnPropertyChanged(nameof(ClassCode));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Subject));
            OnPropertyChanged(nameof(Semester));
            OnPropertyChanged(nameof(AcademicYear));
            OnPropertyChanged(nameof(IsActive));
            OnPropertyChanged(nameof(CanSave));
        }

        public void LoadFromClassDetail(ClassDetailAdmin classDetail)
        {
            if (classDetail == null) return;

            ClassId = classDetail.ClassId.ToString();
            ClassName = classDetail.ClassName ?? string.Empty;
            ClassCode = classDetail.ClassCode ?? string.Empty;
            Description = classDetail.Description ?? string.Empty;
            Semester = classDetail.Semester ?? string.Empty;
            IsActive = classDetail.IsActive;

            // Force UI update - raise PropertyChanged cho tất cả properties
            OnPropertyChanged(nameof(ClassId));
            OnPropertyChanged(nameof(ClassName));
            OnPropertyChanged(nameof(ClassCode));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Semester));
            OnPropertyChanged(nameof(IsActive));
            OnPropertyChanged(nameof(CanSave));
        }

        public async Task SaveChangesAsync()
        {
            if (!CanSave)
            {
                ErrorMessage = "Vui lòng nhập tên lớp học";
                return;
            }

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                if (_isAdminMode && _adminClassService != null)
                {
                    var request = new UpdateClassByAdminRequest
                    {
                        ClassId = Guid.Parse(ClassId),
                        Name = ClassName,
                        Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                        Subject = string.IsNullOrWhiteSpace(Subject) ? null : Subject,
                        Semester = string.IsNullOrWhiteSpace(Semester) ? null : Semester,
                        AcademicYear = string.IsNullOrWhiteSpace(AcademicYear) ? null : AcademicYear,
                        IsActive = IsActive
                    };

                    var success = await _adminClassService.UpdateClassAsync(request);

                    if (success)
                    {
                        CloseRequested?.Invoke(true);
                    }
                    else
                    {
                        ErrorMessage = "Không thể cập nhật lớp học. Vui lòng thử lại.";
                    }
                }
                else if (_classService != null)
                {
                    var request = new UpdateClassRequest
                    {
                        ClassId = ClassId,
                        Name = ClassName,
                        Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                        Subject = string.IsNullOrWhiteSpace(Subject) ? null : Subject,
                        Semester = string.IsNullOrWhiteSpace(Semester) ? null : Semester,
                        AcademicYear = string.IsNullOrWhiteSpace(AcademicYear) ? null : AcademicYear,
                        IsActive = IsActive
                    };

                    var result = await _classService.UpdateClassAsync(request);

                    if (result.Success)
                    {
                        CloseRequested?.Invoke(true);
                    }
                    else
                    {
                        ErrorMessage = result.Message ?? "Không thể cập nhật lớp học. Vui lòng thử lại.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region INotifyPropertyChanged

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
