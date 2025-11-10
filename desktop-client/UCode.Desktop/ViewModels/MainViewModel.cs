using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private User _currentUser;
        private int _selectedNavigationIndex;
        private object _currentView;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public int SelectedNavigationIndex
        {
            get => _selectedNavigationIndex;
            set
            {
                if (SetProperty(ref _selectedNavigationIndex, value))
                {
                    NavigateToPage(value);
                }
            }
        }

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ICommand LogoutCommand { get; }

        public MainViewModel(AuthService authService)
        {
            _authService = authService;
            _currentUser = authService.CurrentUser;

            LogoutCommand = new RelayCommand(_ => ExecuteLogout());

            // Subscribe to user changes
            _authService.UserChanged += (s, user) => CurrentUser = user;

            // Set default view
            SelectedNavigationIndex = 0;
        }

        private void NavigateToPage(int index)
        {
            // TODO: Implement navigation logic
            switch (index)
            {
                case 0: // Home
                    CurrentView = new TextBlock { Text = "Home Page - Coming Soon", FontSize = 24, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    break;
                case 1: // My Classes
                    CurrentView = new TextBlock { Text = "My Classes - Coming Soon", FontSize = 24, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    break;
                case 2: // Assignments
                    CurrentView = new TextBlock { Text = "Assignments - Coming Soon", FontSize = 24, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    break;
                case 3: // Practice
                    CurrentView = new TextBlock { Text = "Practice - Coming Soon", FontSize = 24, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    break;
                case 4: // Submissions
                    CurrentView = new TextBlock { Text = "Submissions - Coming Soon", FontSize = 24, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    break;
            }
        }

        private void ExecuteLogout()
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _authService.Logout();

                // Close main window and show login window
                var loginWindow = App.ServiceProvider.GetService(typeof(Views.LoginWindow)) as Views.LoginWindow;
                loginWindow?.Show();

                Application.Current.Windows[0]?.Close();
            }
        }
    }
}
