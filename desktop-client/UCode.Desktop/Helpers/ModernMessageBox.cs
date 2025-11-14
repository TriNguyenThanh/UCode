using System.Windows;
using ModernWpf.Controls;

namespace UCode.Desktop.Helpers
{
    /// <summary>
    /// Modern MessageBox replacement using ModernWpf ContentDialog
    /// Looks like Windows 11 instead of old Windows XP style
    /// </summary>
    public static class ModernMessageBox
    {
        public static async Task<ContentDialogResult> ShowAsync(
            string message, 
            string title = "Thông báo",
            string primaryButtonText = "OK",
            string secondaryButtonText = "",
            string closeButtonText = "")
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = string.IsNullOrEmpty(closeButtonText) && string.IsNullOrEmpty(secondaryButtonText) ? "" : closeButtonText
            };

            if (!string.IsNullOrEmpty(secondaryButtonText))
            {
                dialog.SecondaryButtonText = secondaryButtonText;
            }

            return await dialog.ShowAsync();
        }

        public static async Task ShowInfoAsync(string message, string title = "Thông báo")
        {
            await ShowAsync(message, title, "OK");
        }

        public static async Task ShowErrorAsync(string message, string title = "Lỗi")
        {
            await ShowAsync(message, title, "OK");
        }

        public static async Task ShowWarningAsync(string message, string title = "Cảnh báo")
        {
            await ShowAsync(message, title, "OK");
        }

        public static async Task<bool> ShowConfirmAsync(string message, string title = "Xác nhận")
        {
            var result = await ShowAsync(message, title, "Có", "Không");
            return result == ContentDialogResult.Primary;
        }

        public static async Task<ContentDialogResult> ShowYesNoCancelAsync(string message, string title = "Xác nhận")
        {
            return await ShowAsync(message, title, "Có", "Không", "Hủy");
        }

        // Synchronous versions for compatibility with existing code
        public static void Show(string message, string title = "Thông báo")
        {
            Application.Current.Dispatcher.Invoke(async () => await ShowAsync(message, title));
        }

        public static void ShowInfo(string message, string title = "Thông báo")
        {
            Application.Current.Dispatcher.Invoke(async () => await ShowInfoAsync(message, title));
        }

        public static void ShowError(string message, string title = "Lỗi")
        {
            Application.Current.Dispatcher.Invoke(async () => await ShowErrorAsync(message, title));
        }

        public static void ShowWarning(string message, string title = "Cảnh báo")
        {
            Application.Current.Dispatcher.Invoke(async () => await ShowWarningAsync(message, title));
        }
    }
}
