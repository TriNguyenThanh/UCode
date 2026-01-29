using System;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Views.Dialogs;

namespace UCode.Desktop.Helpers
{
    /// <summary>
    /// Helper class providing custom dialog methods with modern UI
    /// Uses CustomMessageDialog for better appearance
    /// </summary>
    public static class DialogHelper
    {
        #region Show Custom Dialog Methods

        /// <summary>
        /// Default dialog settings with no animation for faster display
        /// </summary>
        private static MetroDialogSettings FastDialogSettings => new MetroDialogSettings
        {
            AnimateShow = false,
            AnimateHide = false,
            ColorScheme = MetroDialogColorScheme.Theme
        };

        /// <summary>
        /// Shows a custom info message dialog
        /// </summary>
        public static async Task<MessageDialogResult> ShowInfoAsync(MetroWindow window, string title, string message)
        {
            if (window == null) return MessageDialogResult.Canceled;

            var dialog = new CustomMessageDialog
            {
                Title = title,
                Message = message,
                AffirmativeButtonText = "OK",
                ShowNegativeButton = false,
                ShowIcon = true
            };

            await window.ShowMetroDialogAsync(dialog, FastDialogSettings);

            // Wait for button click
            await WaitForDialogResult(dialog);

            await window.HideMetroDialogAsync(dialog, FastDialogSettings);
            return dialog.DialogResult;
        }

        /// <summary>
        /// Shows a custom confirmation dialog
        /// </summary>
        public static async Task<MessageDialogResult> ShowConfirmAsync(MetroWindow window, string title, string message,
            string affirmativeText = "Xác nhận", string negativeText = "Hủy")
        {
            if (window == null) return MessageDialogResult.Canceled;

            var dialog = new CustomMessageDialog
            {
                Title = title,
                Message = message,
                AffirmativeButtonText = affirmativeText,
                NegativeButtonText = negativeText,
                ShowNegativeButton = true,
                ShowIcon = true
            };

            await window.ShowMetroDialogAsync(dialog, FastDialogSettings);

            // Wait for button click
            await WaitForDialogResult(dialog);

            await window.HideMetroDialogAsync(dialog, FastDialogSettings);
            return dialog.DialogResult;
        }

        /// <summary>
        /// Shows a custom error message dialog
        /// </summary>
        public static async Task ShowErrorAsync(MetroWindow window, string title, string message)
        {
            if (window == null) return;

            var dialog = new CustomMessageDialog
            {
                Title = title,
                Message = message,
                AffirmativeButtonText = "Đóng",
                ShowNegativeButton = false,
                ShowIcon = true
            };

            await window.ShowMetroDialogAsync(dialog, FastDialogSettings);

            // Wait for button click
            await WaitForDialogResult(dialog);

            await window.HideMetroDialogAsync(dialog, FastDialogSettings);
        }

        /// <summary>
        /// Shows a custom success message dialog
        /// </summary>
        public static async Task ShowSuccessAsync(MetroWindow window, string title, string message)
        {
            if (window == null) return;

            var dialog = new CustomMessageDialog
            {
                Title = title,
                Message = message,
                AffirmativeButtonText = "OK",
                ShowNegativeButton = false,
                ShowIcon = true
            };

            await window.ShowMetroDialogAsync(dialog, FastDialogSettings);

            // Wait for button click
            await WaitForDialogResult(dialog);

            await window.HideMetroDialogAsync(dialog, FastDialogSettings);
        }

        #endregion

        #region Extension Methods

        /// <summary>
        /// Extension method for MetroWindow to show info dialog
        /// </summary>
        public static async Task ShowCustomInfoAsync(this MetroWindow window, string title, string message)
        {
            await ShowInfoAsync(window, title, message);
        }

        /// <summary>
        /// Extension method for MetroWindow to show confirm dialog
        /// </summary>
        public static async Task<bool> ShowCustomConfirmAsync(this MetroWindow window, string title, string message,
            string affirmativeText = "Xác nhận", string negativeText = "Hủy")
        {
            var result = await ShowConfirmAsync(window, title, message, affirmativeText, negativeText);
            return result == MessageDialogResult.Affirmative;
        }

        /// <summary>
        /// Extension method for MetroWindow to show error dialog
        /// </summary>
        public static async Task ShowCustomErrorAsync(this MetroWindow window, string title, string message)
        {
            await ShowErrorAsync(window, title, message);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the current active MetroWindow
        /// </summary>
        public static MetroWindow GetCurrentWindow()
        {
            // Try to find active window first
            foreach (Window w in Application.Current.Windows)
            {
                if (w.IsActive && w is MetroWindow metroWindow)
                {
                    return metroWindow;
                }
            }

            // Fallback to MainWindow
            return Application.Current.MainWindow as MetroWindow;
        }

        /// <summary>
        /// Shows info dialog on current active window
        /// </summary>
        public static async Task ShowInfoAsync(string title, string message)
        {
            var window = GetCurrentWindow();
            if (window != null)
            {
                await ShowInfoAsync(window, title, message);
            }
        }

        /// <summary>
        /// Shows confirm dialog on current active window
        /// </summary>
        public static async Task<bool> ShowConfirmAsync(string title, string message)
        {
            var window = GetCurrentWindow();
            if (window != null)
            {
                var result = await ShowConfirmAsync(window, title, message);
                return result == MessageDialogResult.Affirmative;
            }
            return false;
        }

        /// <summary>
        /// Shows error dialog on current active window
        /// </summary>
        public static async Task ShowErrorAsync(string title, string message)
        {
            var window = GetCurrentWindow();
            if (window != null)
            {
                await ShowErrorAsync(window, title, message);
            }
        }

        /// <summary>
        /// Waits for the dialog result (button click)
        /// </summary>
        private static async Task WaitForDialogResult(CustomMessageDialog dialog)
        {
            var initialResult = dialog.DialogResult;

            // Poll for result change (simple approach)
            while (dialog.DialogResult == initialResult)
            {
                await Task.Delay(50);
            }
        }

        #endregion
    }
}
