using System.Windows;
using System.Windows.Controls.Primitives;
using UCode.Desktop.Services;
using UCode.Desktop.Helpers;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace UCode.Desktop.Views
{
    public partial class TeacherHomeWindow : MetroWindow
    {
        private readonly NavigationService _navigationService;
        private readonly AuthService _authService;
        private readonly AIDetectorService _aiDetectorService;

        public TeacherHomeWindow(NavigationService navigationService, AuthService authService, AIDetectorService aiDetectorService)
        {
            InitializeComponent();
            
            _navigationService = navigationService;
            _authService = authService;
            _aiDetectorService = aiDetectorService;
            
            // Set up navigation frame
            _navigationService.SetFrame(NavigationFrame);

            // Handle back button visibility
            _navigationService.CanGoBackChanged += (s, canGoBack) =>
            {
                BackButton.Visibility = canGoBack ? Visibility.Visible : Visibility.Collapsed;
            };

            // Navigate to home page on load
            Loaded += (s, e) =>
            {
                var homePage = App.ServiceProvider?.GetService(typeof(Pages.TeacherHomePage)) as Pages.TeacherHomePage;
                if (homePage != null)
                {
                    // Set user name and email from ViewModel
                    if (homePage.DataContext is ViewModels.TeacherHomeViewModel viewModel)
                    {
                        UserNameText.Text = viewModel.TeacherName;
                        UserEmailText.Text = _authService.CurrentUser?.Email ?? "teacher@ucode.io.vn";
                    }
                    
                    _navigationService.NavigateTo(homePage);
                }
            };

            // Cleanup when window closes
            Closing += (s, e) =>
            {
                try
                {
                    _aiDetectorService?.StopAIDetector();
                }
                catch { /* Ignore cleanup errors */ }
            };
        }

        private void UserMenuButton_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = !UserMenuPopup.IsOpen;
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;
            var profilePage = App.ServiceProvider?.GetService(typeof(Pages.TeacherProfilePage)) as Pages.TeacherProfilePage;
            if (profilePage != null)
            {
                _navigationService.NavigateTo(profilePage);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;
            var settingsPage = App.ServiceProvider?.GetService(typeof(Pages.SettingsPage)) as Pages.SettingsPage;
            if (settingsPage != null)
            {
                _navigationService.NavigateTo(settingsPage);
            }
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;
            
            var result = await this.ShowMessageAsync(
                "Đăng xuất",
                "Bạn có chắc chắn muốn đăng xuất?",
                MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "Đăng xuất",
                    NegativeButtonText = "Hủy",
                    DefaultButtonFocus = MessageDialogResult.Negative
                });

            if (result == MessageDialogResult.Affirmative)
            {
                // Clear user session
                _authService.Logout();
                
                // Reset shutdown mode to prevent app from closing
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                
                // Close this window and show login
                var loginViewModel = App.ServiceProvider?.GetService(typeof(ViewModels.LoginViewModel)) as ViewModels.LoginViewModel;
                var loginWindow = new LoginWindow(loginViewModel, _authService);
                loginWindow.Show();
                this.Close();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.GoBack();
        }

        private void DashboardTab_Click(object sender, RoutedEventArgs e)
        {
            // Clear navigation stack and go to home
            _navigationService.ClearNavigationStack();
            
            var homePage = App.ServiceProvider?.GetService(typeof(Pages.TeacherHomePage)) as Pages.TeacherHomePage;
            if (homePage != null)
            {
                _navigationService.NavigateTo(homePage);
            }

            // Update tab styles
            DashboardTab.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FACB01"));
            DashboardTab.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FACB01"));
            DashboardTab.BorderThickness = new Thickness(0, 0, 0, 3);
            
            ProblemsTab.Foreground = System.Windows.Media.Brushes.White;
            ProblemsTab.BorderThickness = new Thickness(0);
        }

        private void ProblemsTab_Click(object sender, RoutedEventArgs e)
        {
            // Clear navigation stack and go to problems page
            _navigationService.ClearNavigationStack();
            
            var problemsPage = App.ServiceProvider?.GetService(typeof(Pages.TeacherProblemsPage)) as Pages.TeacherProblemsPage;
            if (problemsPage != null)
            {
                _navigationService.NavigateTo(problemsPage);
            }

            // Update tab styles
            ProblemsTab.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FACB01"));
            ProblemsTab.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FACB01"));
            ProblemsTab.BorderThickness = new Thickness(0, 0, 0, 3);
            
            DashboardTab.Foreground = System.Windows.Media.Brushes.White;
            DashboardTab.BorderThickness = new Thickness(0);
        }
    }
}

