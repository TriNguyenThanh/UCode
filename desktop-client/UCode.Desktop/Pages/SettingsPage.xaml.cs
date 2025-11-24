using System;
using System.Windows;
using System.Windows.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class SettingsPage : UserControl
    {
        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Load user data when page loads
            Loaded += async (s, e) =>
            {
                try
                {
                    await viewModel.LoadUserDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.CurrentPassword = ((PasswordBox)sender).Password;
            }
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.NewPassword = ((PasswordBox)sender).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}
