using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using UCode.Desktop.Services;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class MainWindow : MetroWindow
    {
        private readonly NavigationService _navigationService;
        private readonly AIDetectorService _aiDetectorService;

        public MainWindow(MainViewModel viewModel, NavigationService navigationService, AIDetectorService aiDetectorService)
        {
            _aiDetectorService = aiDetectorService;
            try
            {
                var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mainwindow.log");
                System.IO.File.AppendAllText(logPath, $"MainWindow constructor started at {DateTime.Now}\n");

                InitializeComponent();
                System.IO.File.AppendAllText(logPath, "InitializeComponent completed\n");

                DataContext = viewModel;
                _navigationService = navigationService;
                System.IO.File.AppendAllText(logPath, "DataContext and NavigationService set\n");

                // Load data when window loads
                Loaded += async (s, e) =>
                {
                    try
                    {
                        System.IO.File.AppendAllText(logPath, "Window Loaded event fired. Initializing NavigationService...\n");

                        // Setup NavigationFrame
                        _navigationService.SetFrame(NavigationFrame);

                        // Hide HomeScrollViewer when navigating, show it when going back to home
                        _navigationService.CanGoBackChanged += (sender, canGoBack) =>
                        {
                            HomeScrollViewer.Visibility = canGoBack ? Visibility.Collapsed : Visibility.Visible;
                            // Clear NavigationFrame content when going back to home
                            if (!canGoBack)
                            {
                                NavigationFrame.Content = null;
                            }
                        };


                        System.IO.File.AppendAllText(logPath, "NavigationService initialized. Loading data...\n");
                        await viewModel.LoadDataAsync();
                        System.IO.File.AppendAllText(logPath, "LoadDataAsync completed\n");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.AppendAllText(logPath, $"ERROR in Loaded event: {ex.Message}\n{ex.StackTrace}\n");
                        MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                System.IO.File.AppendAllText(logPath, "MainWindow constructor completed\n");
            }
            catch (Exception ex)
            {
                var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mainwindow_error.log");
                System.IO.File.WriteAllText(logPath, $"FATAL ERROR in MainWindow constructor: {ex.Message}\n{ex.StackTrace}\n\nInner: {ex.InnerException?.Message}\n{ex.InnerException?.StackTrace}");
                MessageBox.Show($"Fatal error creating main window: {ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }

            // Cleanup when window closes
            Closing += (s, e) =>
            {
                try
                {
                    _aiDetectorService?.StopAIDetector();
                }
                catch (Exception ex)
                {
                    var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window_cleanup_error.log");
                    System.IO.File.WriteAllText(logPath, $"Error during window cleanup: {ex.Message}\n{ex.StackTrace}");
                }
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
                        focusedElement is System.Windows.Controls.RichTextBox)
                    {
                        return; // Let TextBox handle the Backspace
                    }

                    _navigationService.GoBack();
                    e.Handled = true;
                }
            };
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsPage = App.ServiceProvider.GetService(typeof(Pages.SettingsPage)) as Pages.SettingsPage;
                if (settingsPage != null)
                {
                    var settingsWindow = new MetroWindow
                    {
                        Title = "Cài đặt",
                        Width = 900,
                        Height = 700,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Content = settingsPage
                    };
                    settingsWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var authService = App.ServiceProvider.GetService(typeof(AuthService)) as AuthService;
                var currentUser = authService?.CurrentUser;
                
                // Check user role - Teachers use TeacherProfilePage, Students use StudentProfilePage
                if (currentUser?.Role == Models.UserRole.Teacher)
                {
                    var profilePage = App.ServiceProvider.GetService(typeof(Pages.TeacherProfilePage)) as Pages.TeacherProfilePage;
                    if (profilePage != null)
                    {
                        var profileWindow = new MetroWindow
                        {
                            Title = "Hồ sơ",
                            Width = 900,
                            Height = 700,
                            WindowStartupLocation = WindowStartupLocation.CenterScreen,
                            Content = profilePage
                        };
                        profileWindow.ShowDialog();
                    }
                }
                else
                {
                    // Students use StudentProfilePage
                    var profilePage = App.ServiceProvider.GetService(typeof(Pages.StudentProfilePage)) as Pages.StudentProfilePage;
                    if (profilePage != null)
                    {
                        var profileWindow = new MetroWindow
                        {
                            Title = "Hồ sơ",
                            Width = 1000,
                            Height = 750,
                            WindowStartupLocation = WindowStartupLocation.CenterScreen,
                            Content = profilePage
                        };
                        profileWindow.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel?.LogoutCommand?.CanExecute(null) == true)
                {
                    viewModel.LogoutCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserMenuButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button?.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}
