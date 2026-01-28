using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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

            // Handle back button visibility based on current page type
            _navigationService.Navigated += (s, page) =>
            {
                // Hide back button for main tabs (Home and Problems pages)
                bool isMainTab = page is Pages.TeacherHomePage || page is Pages.TeacherProblemsPage;
                BackButton.Visibility = (!isMainTab && _navigationService.CanGoBack) ? Visibility.Visible : Visibility.Collapsed;
            };

            _navigationService.CanGoBackChanged += (s, canGoBack) =>
            {
                // Only update if we can determine the current page
                if (_navigationService.CanGoBack && NavigationFrame.Content != null)
                {
                    bool isMainTab = NavigationFrame.Content is Pages.TeacherHomePage || NavigationFrame.Content is Pages.TeacherProblemsPage;
                    BackButton.Visibility = (!isMainTab && canGoBack) ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    BackButton.Visibility = Visibility.Collapsed;
                }
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

                    _navigationService.NavigateTo(homePage, null, false); // Don't add to stack for initial page
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

            // Handle Backspace key for navigation back
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Back && _navigationService.CanGoBack)
                {
                    // Don't trigger if focus is on a TextBox or similar input control
                    var focusedElement = Keyboard.FocusedElement;
                    if (focusedElement is System.Windows.Controls.TextBox ||
                        focusedElement is System.Windows.Controls.PasswordBox ||
                        focusedElement is System.Windows.Controls.RichTextBox ||
                        focusedElement is ICSharpCode.AvalonEdit.Editing.TextArea ||
                        focusedElement is ICSharpCode.AvalonEdit.TextEditor)
                    {
                        return; // Let input control handle the Backspace
                    }

                    // Don't navigate back if on main tab pages
                    bool isMainTab = NavigationFrame.Content is Pages.TeacherHomePage || NavigationFrame.Content is Pages.TeacherProblemsPage;
                    if (isMainTab)
                    {
                        e.Handled = true;
                        return;
                    }

                    _navigationService.GoBack();
                    e.Handled = true;
                }
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

                // Clear navigation stack
                _navigationService.ClearNavigationStack();

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
                _navigationService.NavigateTo(homePage, null, false);
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
                _navigationService.NavigateTo(problemsPage, null, false);
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

