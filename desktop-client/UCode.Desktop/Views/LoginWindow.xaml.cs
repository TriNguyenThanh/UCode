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
            if (success)
            {
                var mainWindow = App.ServiceProvider.GetService(typeof(MainWindow)) as MainWindow;
                mainWindow?.Show();
                this.Close();
            }
        }
    }
}
