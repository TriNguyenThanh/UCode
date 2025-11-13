using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UCode.Desktop.Helpers;
using UCode.Desktop.Services;
using UCode.Desktop.Models;
using UCode.Desktop.Models.Enums;

namespace UCode.Desktop.ViewModels
{
    public class ProblemCreateViewModel : ViewModelBase
    {
        private readonly ProblemService _problemService;
        private readonly NavigationService _navigationService;
        private bool _isLoading;
        private string _title = string.Empty;
        private string _code = string.Empty;
        private Difficulty _difficulty = Difficulty.EASY;
        private Models.Enums.Visibility _visibility = Models.Enums.Visibility.PRIVATE;
        private string _error = string.Empty;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        public Difficulty Difficulty
        {
            get => _difficulty;
            set => SetProperty(ref _difficulty, value);
        }

        public Models.Enums.Visibility Visibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }

        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ProblemCreateViewModel(ProblemService problemService, NavigationService navigationService)
        {
            _problemService = problemService;
            _navigationService = navigationService;

            SaveCommand = new RelayCommand(async _ => await ExecuteSaveAsync());
            CancelCommand = new RelayCommand(_ => ExecuteCancel());
        }

        private async Task ExecuteSaveAsync()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(Title))
            {
                MessageBox.Show("Vui lòng nhập tên bài toán", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            Error = string.Empty;

            try
            {
                var request = new Models.CreateProblemRequest
                {
                    Title = Title.Trim(),
                    Code = string.IsNullOrWhiteSpace(Code) ? null : Code.Trim(),
                    Difficulty = Difficulty,
                    Visibility = Visibility
                };

                var response = await _problemService.CreateProblemAsync(request);

                if (response?.Success == true && response.Data != null)
                {
                    MessageBox.Show(
                        "Tạo bài tập thành công!\n\nBạn sẽ được chuyển đến trang chỉnh sửa để tiếp tục thêm chi tiết.",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Navigate to problem edit page
                    var editViewModel = App.ServiceProvider?.GetService(typeof(ProblemEditViewModel)) as ProblemEditViewModel;
                    if (editViewModel != null)
                    {
                        var editPage = new Pages.ProblemEditPage(editViewModel);
                        _navigationService.NavigateTo(editPage, response.Data.ProblemId);
                    }
                }
                else
                {
                    Error = response?.Message ?? "Không thể tạo bài toán";
                    MessageBox.Show(Error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Error = $"Lỗi: {ex.Message}";
                MessageBox.Show(Error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteCancel()
        {
            _navigationService.GoBack();
        }
    }

    // CreateProblemRequest moved to Models/Problem.cs
}

