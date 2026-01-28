using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;

namespace UCode.Desktop.ViewModels
{
    /// <summary>
    /// Wrapper class for MetroWindow that intercepts ShowMessageAsync calls
    /// to use custom dialog instead of default MahApps dialog
    /// </summary>
    public class DialogWindow
    {
        private readonly MetroWindow _window;

        public DialogWindow(MetroWindow window)
        {
            _window = window;
        }

        /// <summary>
        /// Shows custom message dialog (intercepts default MahApps dialog)
        /// </summary>
        public async Task<MessageDialogResult> ShowMessageAsync(string title, string message)
        {
            if (_window == null) return MessageDialogResult.Canceled;
            return await DialogHelper.ShowInfoAsync(_window, title, message);
        }

        /// <summary>
        /// Shows custom message dialog with style (intercepts default MahApps dialog)
        /// </summary>
        public async Task<MessageDialogResult> ShowMessageAsync(string title, string message,
            MessageDialogStyle style, MetroDialogSettings settings = null)
        {
            if (_window == null) return MessageDialogResult.Canceled;

            // Use custom dialog based on style
            if (style == MessageDialogStyle.AffirmativeAndNegative)
            {
                var affirmText = settings?.AffirmativeButtonText ?? "Xác nhận";
                var negativeText = settings?.NegativeButtonText ?? "Hủy";
                return await DialogHelper.ShowConfirmAsync(_window, title, message, affirmText, negativeText);
            }
            else
            {
                return await DialogHelper.ShowInfoAsync(_window, title, message);
            }
        }

        /// <summary>
        /// Gets the underlying MetroWindow (for cases that need direct access)
        /// </summary>
        public MetroWindow Window => _window;

        /// <summary>
        /// Implicit conversion to MetroWindow for backward compatibility
        /// Allows: MetroWindow mw = GetMetroWindow();
        /// </summary>
        public static implicit operator MetroWindow(DialogWindow dialog) => dialog?._window;

        /// <summary>
        /// Implicit conversion to Window for setting dialog.Owner
        /// Allows: dialog.Owner = GetMetroWindow();
        /// </summary>
        public static implicit operator Window(DialogWindow dialog) => dialog?._window;

        /// <summary>
        /// Implicit conversion to bool for null checks
        /// Allows: if (GetMetroWindow() != null)
        /// </summary>
        public static implicit operator bool(DialogWindow dialog) => dialog?._window != null;
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Gets the active MetroWindow wrapped in DialogWindow for custom dialogs
        /// All calls to ShowMessageAsync will automatically use custom dialog
        /// </summary>
        public DialogWindow GetMetroWindow()
        {
            // Try to get the active MetroWindow first
            var activeWindow = Application.Current.Windows.OfType<MetroWindow>()
                .FirstOrDefault(w => w.IsActive);

            if (activeWindow != null)
                return new DialogWindow(activeWindow);

            // Fallback to any MetroWindow
            var metroWindow = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
            if (metroWindow != null)
                return new DialogWindow(metroWindow);


            // If no compatible window found, return null
            return null;
        }

        /// <summary>
        /// Gets the raw MetroWindow (for cases that need direct MetroWindow access)
        /// </summary>
        protected MetroWindow GetRawMetroWindow()
        {
            var activeWindow = Application.Current.Windows.OfType<MetroWindow>()
                .FirstOrDefault(w => w.IsActive);

            if (activeWindow != null)
                return activeWindow;

            return Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
        }

        #region Dialog Helper Methods (shortcuts)

        /// <summary>
        /// Shows a custom info message dialog with modern UI
        /// </summary>
        protected async Task ShowMessageAsync(string title, string message)
        {
            var window = GetRawMetroWindow();
            if (window != null)
            {
                await DialogHelper.ShowInfoAsync(window, title, message);
            }
        }

        /// <summary>
        /// Shows a custom confirmation dialog and returns true if user confirmed
        /// </summary>
        protected async Task<bool> ShowConfirmAsync(string title, string message,
            string affirmativeText = "Xác nhận", string negativeText = "Hủy")
        {
            var window = GetRawMetroWindow();
            if (window != null)
            {
                var result = await DialogHelper.ShowConfirmAsync(window, title, message, affirmativeText, negativeText);
                return result == MessageDialogResult.Affirmative;
            }
            return false;
        }

        /// <summary>
        /// Shows a custom error message dialog
        /// </summary>
        protected async Task ShowErrorAsync(string title, string message)
        {
            var window = GetRawMetroWindow();
            if (window != null)
            {
                await DialogHelper.ShowErrorAsync(window, title, message);
            }
        }

        /// <summary>
        /// Shows a custom success message dialog
        /// </summary>
        protected async Task ShowSuccessAsync(string title, string message)
        {
            var window = GetRawMetroWindow();
            if (window != null)
            {
                await DialogHelper.ShowSuccessAsync(window, title, message);
            }
        }

        #endregion
    }
}
