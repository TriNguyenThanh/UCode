using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using UCode.Desktop.Pages.Admin;
using UCode.Desktop.Services;

namespace UCode.Desktop.Views.Windows
{
    public partial class AdminHomeWindow : Window, INotifyPropertyChanged
    {
        private readonly AdminService _adminService;
        private readonly NavigationService _navigationService;
        private string _adminName = "Administrator";
        private bool _canGoBack;

        public AdminHomeWindow(AdminService adminService)
        {
            InitializeComponent();
            _adminService = adminService;
            _navigationService = new NavigationService();
            _navigationService.SetFrame(MainFrame);
            DataContext = this;

            // Subscribe to navigation events
            MainFrame.Navigated += MainFrame_Navigated;

            // Navigate to dashboard on load
            NavigateToAdminHomePage();
        }

        public string AdminName
        {
            get => _adminName;
            set
            {
                _adminName = value;
                OnPropertyChanged();
            }
        }

        public bool CanGoBack
        {
            get => _canGoBack;
            set
            {
                _canGoBack = value;
                OnPropertyChanged();
            }
        }

        private void NavigateToAdminHomePage()
        {
            var page = new AdminHomePage(_adminService, _navigationService);
            MainFrame.Content = page;
        }

        private void NavigateToPage(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pageName)
            {
                object? page = pageName switch
                {
                    "AdminHomePage" => new AdminHomePage(_adminService, _navigationService),
                    "AdminUsersPage" => new AdminUsersPage(_adminService),
                    "AdminClassesPage" => new AdminClassesPage(_adminService),
                    "AdminProblemsPage" => new AdminProblemsPage(_adminService),
                    "AdminSettingsPage" => new AdminSettingsPage(_adminService),
                    _ => null
                };

                if (page != null)
                {
                    MainFrame.Content = page;
                }
            }
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            CanGoBack = MainFrame.CanGoBack;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // TODO: Clear session and navigate to login
                Application.Current.Shutdown();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
