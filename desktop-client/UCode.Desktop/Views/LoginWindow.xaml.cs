using System;
using System.Windows;
using System.Windows.Controls;
using UCode.Desktop.Services;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService;

        public LoginWindow(LoginViewModel viewModel, AuthService authService)
        {
            InitializeComponent();
            DataContext = viewModel;
            _authService = authService;

            viewModel.LoginCompleted += OnLoginCompleted;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void OnLoginCompleted(object sender, bool success)
        {
            try
            {
                var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "login.log");
                System.IO.File.AppendAllText(logPath, $"Login completed. Success={success} at {DateTime.Now}\n");

                if (success)
                {
                    // Get current user to check role
                    var currentUser = _authService.CurrentUser;
                    var userRole = currentUser?.Role.ToString().ToLower() ?? "student";

                    System.IO.File.AppendAllText(logPath, $"User role: {userRole}\n");

                    Window targetWindow = null;

                    // Redirect based on role
                    if (userRole == "teacher")
                    {
                        System.IO.File.AppendAllText(logPath, "Getting TeacherHomeWindow from ServiceProvider...\n");
                        targetWindow = App.ServiceProvider.GetService(typeof(TeacherHomeWindow)) as TeacherHomeWindow;
                    }
                    else
                    {
                        System.IO.File.AppendAllText(logPath, "Getting MainWindow from ServiceProvider...\n");
                        targetWindow = App.ServiceProvider.GetService(typeof(MainWindow)) as MainWindow;
                    }

                    if (targetWindow != null)
                    {
                        System.IO.File.AppendAllText(logPath, $"{targetWindow.GetType().Name} created. Showing...\n");
                        targetWindow.Show();

                        // Set as the main window and enable auto-shutdown
                        System.IO.File.AppendAllText(logPath, "Setting as main window...\n");
                        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                        Application.Current.MainWindow = targetWindow;

                        System.IO.File.AppendAllText(logPath, "Closing login window...\n");
                    }
                    else
                    {
                        System.IO.File.AppendAllText(logPath, "ERROR: Target window is NULL!\n");
                        MessageBox.Show("Cannot create main window!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    this.Close();
                    System.IO.File.AppendAllText(logPath, "Login window closed.\n");
                }
                else
                {
                    System.IO.File.AppendAllText(logPath, "Login failed. Keeping login window open.\n");
                }
            }
            catch (Exception ex)
            {
                var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "login_error.log");
                System.IO.File.WriteAllText(logPath, $"ERROR in OnLoginCompleted: {ex.Message}\n{ex.StackTrace}\n");
                MessageBox.Show($"Error after login: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
