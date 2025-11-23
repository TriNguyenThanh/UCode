using System;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class CreateClassViewModel : ViewModelBase
    {
        private readonly ClassService _classService;
        private bool _isLoading;
        private string _className = string.Empty;
        private string _classCode = string.Empty;
        private string _description = string.Empty;

        public event Action<bool, string?>? CloseRequested;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ClassName
        {
            get => _className;
            set => SetProperty(ref _className, value);
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

        public CreateClassViewModel(ClassService classService)
        {
            _classService = classService;
        }

        public async Task CreateClassAsync()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(ClassName))
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Lỗi",
                    "Vui lòng nhập tên lớp học.");
                return;
            }

            if (string.IsNullOrWhiteSpace(ClassCode))
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Lỗi",
                    "Vui lòng nhập mã lớp.");
                return;
            }

            IsLoading = true;
            try
            {
                var response = await _classService.CreateClassAsync(
                    ClassName,
                    ClassCode,
                    Description
                );

                if (response?.Success == true && response.Data != null)
                {
                    await GetMetroWindow()?.ShowMessageAsync(
                        "Thành công",
                        $"Đã tạo lớp học '{ClassName}' thành công!");
                    
                    CloseRequested?.Invoke(true, response.Data.ClassId);
                }
                else
                {
                    await GetMetroWindow()?.ShowMessageAsync(
                        "Lỗi",
                        response?.Message ?? "Không thể tạo lớp học. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Lỗi",
                    $"Đã xảy ra lỗi: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
