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

        public MetroWindow GetMetroWindow()
        {
            // Try to get the active MetroWindow first (for dialogs)
            var activeWindow = Application.Current.Windows.OfType<MetroWindow>()
                .FirstOrDefault(w => w.IsActive);
            
            if (activeWindow != null)
                return activeWindow;
            
            // Fallback to any MetroWindow
            var metroWindow = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
            if (metroWindow != null)
                return metroWindow;
            
            // If no MetroWindow found, try to find the active window and see if we can convert it
            var anyActiveWindow = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.IsActive);
            
            // For UCodeWindow or other windows, we'll need to use standard MessageBox
            // Return null to signal that we should use MessageBox instead
            return null;
        }
    }
}
