using System.Windows;
using System.Windows.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class StudentProfilePage : Page
    {
        public StudentProfilePage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is StudentProfileViewModel viewModel)
            {
                await viewModel.LoadDataAsync();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Close current profile window
                var currentWindow = Window.GetWindow(this);
                
                // Open SettingsPage
                var settingsPage = App.ServiceProvider.GetService(typeof(Pages.SettingsPage)) as Pages.SettingsPage;
                if (settingsPage != null)
                {
                    var settingsWindow = new Controls.UCodeWindow
                    {
                        Title = "Cài đặt",
                        Width = 900,
                        Height = 700,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Content = settingsPage
                    };
                    
                    // Close current window first
                    currentWindow?.Close();
                    
                    // Show settings window
                    settingsWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error opening settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
