using System.Windows;
using System.Windows.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

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
                    System.IO.File.AppendAllText(logPath, "Getting MainWindow from ServiceProvider...\n");
                    var mainWindow = App.ServiceProvider.GetService(typeof(MainWindow)) as MainWindow;

                    if (mainWindow != null)
                    {
                        System.IO.File.AppendAllText(logPath, "MainWindow created. Showing...\n");
                        mainWindow.Show();

                        // Set main window as the main window and enable auto-shutdown
                        System.IO.File.AppendAllText(logPath, "Setting as main window...\n");
                        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                        Application.Current.MainWindow = mainWindow;

                        System.IO.File.AppendAllText(logPath, "Closing login window...\n");
                    }
                    else
                    {
                        System.IO.File.AppendAllText(logPath, "ERROR: MainWindow is NULL!\n");
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
