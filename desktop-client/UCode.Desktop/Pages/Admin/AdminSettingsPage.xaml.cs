using System.Windows;
using System.Windows.Controls;
using UCode.Desktop.Services;
using UCode.Desktop.ViewModels.Admin;

namespace UCode.Desktop.Pages.Admin
{
    public partial class AdminSettingsPage : Page
    {
        private readonly AdminSettingsViewModel _viewModel;

        public AdminSettingsPage(AdminService adminService)
        {
            InitializeComponent();
            _viewModel = new AdminSettingsViewModel(adminService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadSettingsAsync();
        }
    }
}
