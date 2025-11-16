using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro.Controls;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models.Admin;
using UCode.Desktop.Services;

namespace UCode.Desktop.Views.Dialogs
{
    public partial class ClassEditDialog : MetroWindow, INotifyPropertyChanged
    {
        private readonly AdminService _adminService;
        private readonly ClassManagement _class;
        private string _className = string.Empty;
        private string _description = string.Empty;
        private UserManagement? _selectedTeacher;
        private bool _isActive = true;
        private ObservableCollection<UserManagement> _teachers = new();
        private bool _isLoading;
        private string _validationMessage = string.Empty;
        private bool _hasValidationError;

        public ClassEditDialog(AdminService adminService, ClassManagement classToEdit)
        {
            InitializeComponent();
            _adminService = adminService;
            _class = classToEdit;
            
            // Initialize with existing class data
            ClassName = classToEdit.Name;
            Description = classToEdit.Description ?? string.Empty;
            IsActive = classToEdit.IsActive;

            DataContext = this;
        }

        public string ClassName
        {
            get => _className;
            set
            {
                _className = value;
                OnPropertyChanged();
                UpdateCanUpdate();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public UserManagement? SelectedTeacher
        {
            get => _selectedTeacher;
            set
            {
                _selectedTeacher = value;
                OnPropertyChanged();
                UpdateCanUpdate();
            }
        }

        public new bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<UserManagement> Teachers
        {
            get => _teachers;
            set
            {
                _teachers = value;
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

        public string ValidationMessage
        {
            get => _validationMessage;
            set
            {
                _validationMessage = value;
                OnPropertyChanged();
            }
        }

        public bool HasValidationError
        {
            get => _hasValidationError;
            set
            {
                _hasValidationError = value;
                OnPropertyChanged();
            }
        }

        private bool _canUpdate;
        public bool CanUpdate
        {
            get => _canUpdate;
            set
            {
                _canUpdate = value;
                OnPropertyChanged();
            }
        }

        private void UpdateCanUpdate()
        {
            CanUpdate = !string.IsNullOrWhiteSpace(ClassName) && SelectedTeacher != null;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTeachersAsync();
        }

        private async System.Threading.Tasks.Task LoadTeachersAsync()
        {
            try
            {
                IsLoading = true;
                var teachers = await _adminService.GetAllUsersAsync(role: "Teacher", pageSize: 100);
                
                Teachers.Clear();
                foreach (var teacher in teachers)
                {
                    Teachers.Add(teacher);
                }

                // Select the current teacher
                SelectedTeacher = Teachers.FirstOrDefault(t => t.Id == _class.TeacherId);
            }
            catch (Exception ex)
            {
                ShowValidationError($"Failed to load teachers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            HasValidationError = false;
            ValidationMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(ClassName))
            {
                ShowValidationError("Class name is required");
                return;
            }

            if (SelectedTeacher == null)
            {
                ShowValidationError("Please select a teacher");
                return;
            }

            try
            {
                IsLoading = true;

                var request = new Models.Admin.UpdateClassRequest
                {
                    Name = ClassName.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    TeacherId = SelectedTeacher.Id,
                    IsActive = IsActive
                };

                var success = await _adminService.UpdateClassAsync(_class.Id, request);

                if (success)
                {
                    ModernMessageBox.Show(
                        "Class updated successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowValidationError("Failed to update class. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowValidationError($"Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowValidationError(string message)
        {
            ValidationMessage = message;
            HasValidationError = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
