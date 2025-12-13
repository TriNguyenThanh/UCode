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
            
            // Try to find UCodeWindow (which has MahApps dialog support via attached properties)
            var uCodeWindow = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.GetType().Name == "UCodeWindow");
            
            if (uCodeWindow != null)
            {
                // UCodeWindow supports MahApps dialogs via attached properties
                // We can use it as if it were a MetroWindow
                return uCodeWindow as MetroWindow ?? CreateMetroWindowProxy(uCodeWindow);
            }
            
            // If no compatible window found, return null
            return null;
        }
        
        private MetroWindow CreateMetroWindowProxy(Window window)
        {
            // For UCodeWindow with MahApps dialog support, we can use DialogCoordinator
            // which works with any window that has the proper template parts
            // Return the window cast as MetroWindow (will be null but DialogCoordinator will handle it)
            return null;
        }
    }
}
