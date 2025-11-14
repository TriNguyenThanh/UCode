using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro.Controls;

namespace UCode.Desktop.ViewModels
{
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

        protected MetroWindow GetMetroWindow()
        {
            // Try to get the active window first (for dialogs)
            var activeWindow = Application.Current.Windows.OfType<MetroWindow>()
                .FirstOrDefault(w => w.IsActive);
            
            if (activeWindow != null)
                return activeWindow;
            
            // Fallback to main window
            return Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
        }
    }
}
